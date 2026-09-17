using UnityEngine;
using UnityEngine.Events;
using Colyseus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Joins the Colyseus <c>fairsceneroom</c> as the <c>"game"</c> platform, keeping the game and the
/// companion web app (<c>CountryFairWebApp</c>) in sync on the mini-game tent order. Retries the join
/// on failure and reconnects automatically if an already-established connection drops, forwards local
/// tent reorders to the room via <see cref="UpdateTentsOrder"/>, and relays the room's
/// <c>"updateTentsOrder"</c> broadcasts (i.e. reorders made from the web app) through
/// <see cref="updateTentsOrder"/>.
/// </summary>
public class ConnectToWebApp : MonoBehaviour
{
    [Header("Server Connection Settings")]
    // "localhost" resolves to the DEVICE the build runs on. On the Quest 3 that is the headset
    // itself, which runs no Colyseus server, so the connection silently fails. A standalone build
    // must point at the LAN IP of the machine running CountryFairWebApp/ServerSide
    // (ipconfig -> IPv4 Address of the Wi-Fi adapter the headset is also connected to).
    /// <summary>LAN IP of the PC running <c>CountryFairWebApp/ServerSide</c>; ignored in the Editor when <see cref="_useLocalhostInEditor"/> is set.</summary>
    [SerializeField]
    [Tooltip("Use the IP of the PC's Mobile Hotspot (Settings > Network & Internet > Mobile Hotspot, then run ipconfig and check the virtual Wi-Fi adapter) since the server runs on that same PC. Connect the Quest to that hotspot too. Default Windows hotspot IP is 192.168.137.1; 'localhost' only ever works in the Editor.")]
    private string _serverHost = "192.168.137.1";

    /// <summary>When true, <see cref="GetEndpoint"/> substitutes <c>localhost</c> for <see cref="_serverHost"/> while running in the Editor.</summary>
    [SerializeField]
    [Tooltip("When running in the Editor, ignore Server Host and use localhost instead.")]
    private bool _useLocalhostInEditor = true;

    /// <summary>Port the Colyseus server listens on; must match <c>CountryFairWebApp/ServerSide</c>'s configuration.</summary>
    [SerializeField]
    private int _serverPort = 2567;

    [Header("Reconnection")]
    /// <summary>Delay between failed join attempts in <see cref="ConnectLoop"/>.</summary>
    [SerializeField]
    [Tooltip("Seconds to wait before retrying after a failed connection attempt. 0 disables retrying.")]
    private float _retryDelaySeconds = 3f;

    /// <summary>Enforces the singleton: the first instance survives scene loads, later ones self-destroy.</summary>
    private static ConnectToWebApp s_instance = null;

    /// <summary>The joined <c>fairsceneroom</c> instance; null until <see cref="ConnectLoop"/> succeeds, and while disconnected.</summary>
    private Room<FairState> _room;

    /// <summary>Colyseus client bound to <see cref="GetEndpoint"/>; created once per <see cref="ConnectLoop"/> run.</summary>
    private Client _client;

    /// <summary>Cancels the pending <see cref="ConnectLoop"/> retry delay and stops further retries on <see cref="OnDestroy"/>.</summary>
    private CancellationTokenSource _cancellation;


    /// <summary>
    /// Raised with the mini-game-name-to-slot-number map whenever the room broadcasts an
    /// <c>"updateTentsOrder"</c> message, i.e. the tent order changed on the web app side.
    /// Public and <c>[HideInInspector]</c> rather than an Inspector-wired <c>[SerializeField]</c>:
    /// this object is <c>DontDestroyOnLoad</c> but <c>TentPlaceHolderManager</c> is not, so a listener
    /// baked into the scene would keep pointing at the <c>TentPlaceHolderManager</c> instance destroyed
    /// on the previous <c>CountryFair</c> load once the player returns from a mini-game. Listeners must
    /// instead call <c>AddListener</c> at runtime (see <c>TentPlaceHolderManager.Start()</c>), which
    /// re-registers on every scene load instead of going stale.
    /// </summary>
    [HideInInspector]
    public UnityEvent<Dictionary<string, string>> updateTentsOrder;

    /// <summary>Global accessor for the singleton established in <see cref="Awake"/>; null until the first <see cref="ConnectToWebApp"/> instance in the scene has run its Awake.</summary>
    public static ConnectToWebApp Instance => s_instance;

    /// <summary>Establishes the singleton and marks this object to persist across the hub/mini-game scene loads, so the web app connection survives scene transitions.</summary>
    private void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this;
            DontDestroyOnLoad(gameObject);

            return;
        }

        Destroy(gameObject);
    }

    /// <summary>Starts the background join/retry loop for the winning singleton instance.</summary>
    private void Start()
    {
        _cancellation = new CancellationTokenSource();

        // Fire and forget: ConnectLoop never throws, it logs and retries on its own.
        _ = ConnectLoop(_cancellation.Token);
    }

    /// <summary>
    /// Builds the websocket endpoint. In the Editor we may fall back to localhost for convenience,
    /// but a device build always uses the Inspector-configured host.
    /// </summary>
    private string GetEndpoint()
    {
        string host = _serverHost;

#if UNITY_EDITOR
        if (_useLocalhostInEditor)
        {
            host = "localhost";
        }
#endif

        if (string.IsNullOrWhiteSpace(host))
        {
            host = "localhost";
        }

        return $"ws://{host.Trim()}:{_serverPort}";
    }

    /// <summary>
    /// Keeps the <c>fairsceneroom</c> connection (platform <c>"game"</c>) alive for as long as this
    /// object is alive: joins, and once the join succeeds, waits for that specific connection to end
    /// (<see cref="Room.OnLeave"/>) and immediately loops back to join again, applying the same
    /// <see cref="_retryDelaySeconds"/> backoff whether the previous attempt failed to join or was an
    /// established connection that dropped (e.g. a ping/pong timeout from the main thread stalling
    /// too long). Without this, any drop after the first successful join was final: the room stayed
    /// null forever and the web client was stuck on its waiting screen for the rest of the session.
    /// While connected, subscribes to the room's <c>"updateTentsOrder"</c> broadcasts (reorders made
    /// from the web app) and forwards them through <see cref="updateTentsOrder"/>.
    /// The original version awaited JoinOrCreate inside an async void Start, so on device any
    /// failure (wrong host, server not up yet, headset on another network) was swallowed and the
    /// room stayed null forever with no feedback.
    /// </summary>
    /// <param name="token">Cancelled by <see cref="OnDestroy"/> to stop retrying and abandon a pending retry delay.</param>
    private async Task ConnectLoop(CancellationToken token)
    {
        string endpoint = GetEndpoint();
        string plataform = "game";

        _client = new Client(endpoint);

        while (!token.IsCancellationRequested)
        {
            try
            {
                _room = await _client.JoinOrCreate<FairState>("fairsceneroom", new Dictionary<string, object> { { "platform", plataform } });

                if (token.IsCancellationRequested)
                {
                    await _room.Leave();
                    return;
                }

                Debug.Log($"ConnectToWebApp: connected to {endpoint} (room {_room.RoomId})");

                _room.OnMessage<Dictionary<string, string>>("updateTentsOrder", (fairState) =>
                {
                    updateTentsOrder.Invoke(fairState);
                });

                // TrySetResult (not SetResult) because a Leave() triggered from OnDestroy while this
                // is being awaited would otherwise race the callback and throw on a second completion.
                var disconnected = new TaskCompletionSource<bool>();

                _room.OnLeave += (code) =>
                {
                    Debug.LogWarning($"ConnectToWebApp: disconnected from {endpoint} (code {code}). Will retry in {_retryDelaySeconds} seconds.");
                    _room = null;
                    disconnected.TrySetResult(true);
                };

                await disconnected.Task;

                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                _room = null;

                Debug.LogWarning($"ConnectToWebApp: could not connect to {endpoint} ({exception.Message}). " +
                                 "Check that the server is running and that Server Host is the LAN IP of the server machine, not localhost.");
            }

            if (_retryDelaySeconds <= 0f)
            {
                return;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds), token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    /// <summary>Sends the current mini-game-name-to-slot-number map to the room so the web app mirrors the game's tent order.</summary>
    /// <param name="fairState">Mini-game name to placeholder slot number, as built by <c>TentPlaceHolderManager.GetFairState()</c>.</param>
    /// <remarks>Registered at runtime as a listener on <c>_updateOtherManagers</c> in <c>TentPlaceHolderManager.Start()</c> — called by both the world and wrist-menu instances — rather than wired via the Inspector, so the registration happens again on every <c>CountryFair</c> scene load instead of going stale.</remarks>
    public async void UpdateTentsOrder(Dictionary<string, string> fairState)
    {
        if (_room != null)
        {
            try
            {
                await _room.Send("updateTentsOrder", fairState);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ConnectToWebApp: failed to send tents order ({exception.Message}).");
            }
        }
    }

    /// <summary>Notifies the room that the current dialogue (intro or session-completed) has finished, so the web app can advance its own state.</summary>
    public async void PlayerFinishedDialogue()
    {
        if (_room != null)
        {
            try
            {
                await _room.Send("playerFinishedDialogue");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ConnectToWebApp: failed to send playerFinishedDialogue ({exception.Message}).");
            }
        }
    }

    /// <summary>Notifies the room that the mini-game tutorial has finished, so the web app can advance its own state.</summary>
    public async void PlayerFinishedTutorial()
    {
        if (_room != null)
        {
            try
            {
                await _room.Send("playerFinishedTutorial");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ConnectToWebApp: failed to send playerFinishedTutorial ({exception.Message}).");
            }
        }
    }


    /// <summary>Notifies the room which Unity scene is now active, so the web app can mirror the player's location in the fair/mini-games.</summary>
    /// <param name="sceneName">Lower-case scene name (e.g. <c>"archerygame"</c>, <c>"frisbeegame"</c>).</param>
    public async void UpdateScene(string sceneName)
    {
        if (_room != null)
        {
            try
            {
                await _room.Send("updateScene", new { sceneName });
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ConnectToWebApp: failed to send updateScene ({exception.Message}).");
            }
        }
    }


    /// <summary>Sends the player's current mini-game progress to the room, so the web app can mirror score/goal/streak live.</summary>
    /// <param name="score">Current score, as tracked by <c>ScoreAndStreakSystem</c>.</param>
    /// <param name="goal">Session score goal (<c>PlayerPrefs</c> key <c>SessionGoal</c>).</param>
    /// <param name="streak">Current consecutive-success streak, as tracked by <c>ScoreAndStreakSystem</c>.</param>
    public async void UpdatePlayerProgessOnMiniGame(int score, int goal, int streak){
       
        if (_room != null)
        {
            try
            {
                await _room.Send("updatePlayerProgressOnMiniGame", new { score, goal, streak });
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ConnectToWebApp: failed to send updatePlayerProgressOnMiniGame ({exception.Message}).");
            }
        }
    }

    /// <summary>Stops the retry loop and leaves the room cleanly on shutdown.</summary>
    /// <remarks>
    /// Without this, stopping Play Mode (or quitting the build) tears down this object
    /// without ever closing the socket, so the server only notices the game is gone after
    /// the ping/pong heartbeat times out — the web client's switch back to the waiting
    /// screen (CountryFairRoom.onLeave -> "gameDisconnected") is delayed by that same amount.
    /// </remarks>
    private async void OnDestroy()
    {
        _cancellation?.Cancel();
        _cancellation?.Dispose();
        _cancellation = null;

        if (_room != null)
        {
            await _room.Leave();
            _room = null;
        }
    }
}