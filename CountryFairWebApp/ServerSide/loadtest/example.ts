import { Client, Room } from "@colyseus/sdk";
import { cli, Options } from "@colyseus/loadtest";

/**
 * Smoke load test for `fairsceneroom`: every simulated client joins with a valid `platform`,
 * alternating web / game, so each pair of clients forms one session. The room only admits
 * one client per platform, so with `--numClients 2` both seats of the pair are filled; any
 * further client fails its join, which is the expected behaviour.
 */
let joined = 0;

export async function main(options: Options) {
    const platform = joined++ % 2 === 0 ? "web" : "game";
    const client = new Client(options.endpoint);
    const room: Room = await client.joinOrCreate(options.roomName, { platform });

    console.log(`joined successfully as "${platform}"!`);

    room.onMessage("gamejoined", () => console.log("pair complete"));
    room.onMessage("gameDisconnected", () => console.log("game disconnected"));

    room.onLeave((code: number) => {
        console.log(`left (code ${code})`);
    });
}

cli(main);
