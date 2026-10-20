import { Room, type Client, type Deferred } from "colyseus";

/**
 * Base room for every Country Fair scene.
 *
 * The session is a pairing between exactly two devices: the Meta Quest headset running the
 * Unity game (`platform: "game"`) and the therapist's browser running the React client
 * (`platform: "web"`). This class only holds the pairing bookkeeping; the concrete rooms
 * (see {@link FairSceneRoom}) enforce it in their lifecycle hooks.
 */
export class CountryFairRoom extends Room {
    /** The only accepted `options.platform` values — one client is expected per entry. */
    protected static platforms: Array<string> = ["web", "game"];

    /**
     * How long a dropped client keeps its platform slot while it may reconnect. Covers a Wi-Fi
     * blip on the hotspot and the web SDK's automatic reconnection; a client that does not come
     * back in time is treated as having left (see {@link onLeave}).
     */
    static reconnectionSeconds: number = 20;

    /**
     * Which platforms are already in the room. Guards against a second browser tab or a
     * second headset stealing a slot that belongs to the other half of the pair.
     */
     protected clientsEntered: Record<string, boolean> = {
        ['web']: false,
        ['game']: false
    };

    /** True once every platform in {@link platforms} has joined; the room is then closed to newcomers. */
    protected allPlatformsentered: boolean = false;

    /**
     * Session id of the headset client, so state broadcasts can target everyone *but* the
     * game (the game is the source of truth, not a consumer of these updates).
     */
    protected gameClientId : string = '';

    /**
     * Session id -> platform of every *admitted* client. Clients dropped in `onJoin` (duplicate
     * platform or room already full) are never added, so their `onLeave` frees no slot.
     */
    protected clientPlatforms: Record<string, string> = {};

    /**
     * Session id -> pending reconnection of every admitted client that dropped and still holds
     * its slot (see {@link onDrop}). Removed when it reconnects, leaves for good, or is taken over.
     */
    protected pendingReconnections: Record<string, Deferred<Client>> = {};

    maxClients = 10;

    /**
     * Canonical form of a raw `options.platform`, or `null` when it is missing, not a string or
     * not one the room accepts. The value arrives verbatim from the client join options, so it is
     * trimmed and lower-cased; callers must use the returned value for all bookkeeping, so that
     * e.g. `"Game"` and `"game"` share the same slot in {@link clientsEntered}.
     */
    protected normalizePlatform(platform: unknown) : string | null {
        if (typeof platform !== "string") {
            return null;
        }

        const normalized = platform.trim().toLowerCase();

        return CountryFairRoom.platforms.includes(normalized) ? normalized : null;
    }


    /**
     * Platform of an admitted client, or `undefined` for one that was rejected in `onJoin`
     * (and may still send messages until its `client.leave()` completes).
     */
    protected platformOf(client: { sessionId: string }) : string | undefined {
        return this.clientPlatforms[client.sessionId];
    }

    /**
     * Whether `client` is an admitted client of one of the `allowed` platforms. Logs and returns
     * `false` otherwise, so message handlers can drop the message with a plain `return`.
     */
    protected isSenderAllowed(client: { sessionId: string }, messageType: string, allowed: readonly string[]) : boolean {
        const platform = this.platformOf(client);

        if (platform === undefined || !allowed.includes(platform)) {
            console.warn(`Ignoring "${messageType}" from client ${client.sessionId} (platform: ${platform ?? "not admitted"}).`);
            return false;
        }

        return true;
    }

    /**
     * Gives the dropped client of `platform` up and frees its slot right away, so a fresh join of
     * the same platform can take it instead of being rejected as a duplicate. Needed because the
     * Unity build never resumes a session: after a drop, `ConnectToWebApp.ConnectLoop()` simply
     * calls `JoinOrCreate` again, which gets a new session id. The same happens when the therapist
     * reloads the page instead of letting the SDK reconnect.
     *
     * Returns whether a pending reconnection was found. The given-up client's later `onLeave`
     * finds no slot and does nothing (no `"gameDisconnected"`: its replacement is already in).
     */
    protected takeOverDroppedSlot(platform: string) : boolean {
        const sessionId = Object.keys(this.pendingReconnections)
            .find((id) => this.clientPlatforms[id] === platform);

        if (sessionId === undefined) {
            return false;
        }

        const reconnection = this.pendingReconnections[sessionId];

        this.releaseSlot(sessionId);
        reconnection.reject(new Error(`Replaced by a new ${platform} client.`));

        console.log(`A new ${platform} client replaces dropped client ${sessionId}.`);

        return true;
    }

    /**
     * A client dropped without consent (network blip, ping timeout, page reload): its slot is kept
     * for {@link reconnectionSeconds} so it can resume the same session. If it does not, Colyseus
     * calls {@link onLeave}; if it does, {@link onReconnect}.
     */
    onDrop(client: Client, _code?: number) {
        if (this.platformOf(client) === undefined) {
            return;
        }

        console.log(`Client ${client.sessionId} (${this.platformOf(client)}) dropped; holding its slot for ${CountryFairRoom.reconnectionSeconds}s.`);

        const reconnection = this.allowReconnection(client, CountryFairRoom.reconnectionSeconds);

        // Rejected on expiry, on takeover, and right away while the room is disposing (server
        // shutdown); Colyseus then calls onLeave itself, so the rejection needs no handling here.
        reconnection.catch(() => {});

        this.pendingReconnections[client.sessionId] = reconnection;
    }

    onReconnect(client: Client) {
        delete this.pendingReconnections[client.sessionId];

        console.log(`Client ${client.sessionId} (${this.platformOf(client)}) reconnected.`);
    }

    /**
     * Frees the leaving client's platform slot (web or game) so the same platform can join again,
     * e.g. after the therapist refreshes the page. Called on a consented leave, and after a drop
     * whose reconnection window expired. Rejected and taken-over clients own no slot and are ignored.
     */
    async onLeave(client: Client, _code?: number) {
        const platform = this.clientPlatforms[client.sessionId];

        if (platform === undefined) {
            return;
        }

        this.releaseSlot(client.sessionId);

        if (platform === "game") {
            this.broadcast("gameDisconnected", { message: "The game has disconnected." }, { except: client });
        }
    }

    /** Forgets `sessionId`'s platform slot, so that platform counts as absent again. */
    private releaseSlot(sessionId: string) {
        const platform = this.clientPlatforms[sessionId];

        delete this.clientPlatforms[sessionId];
        delete this.pendingReconnections[sessionId];

        this.clientsEntered[platform] = false;

        this.allPlatformsentered = false;

        if (platform === "game") {
            this.gameClientId = '';
        }
    }
}
