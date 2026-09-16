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
/// on failure, forwards local tent reorders to the room via <see cref="UpdateFairState"/>, and relays
/// the room's <c>"updateFairState"</c> broadcasts (i.e. reorders made from the web app) to
/// <see cref="_updateGameFairState"/>.
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

    /// <summary>Raised with the mini-game-name-to-slot-number map whenever the room broadcasts an <c>"updateFairState"</c> message (i.e. the tent order changed on the web app side), but only once the intro has finished.</summary>
    [SerializeField]
    private UnityEvent<Dictionary<string, string>> _updateGameFairState;

    /// <summary>Enforces the singleton: the first instance survives scene loads, later ones self-destroy.</summary>
    private static ConnectToWebApp s_instance = null;

    /// <summary>The joined <c>fairsceneroom</c> instance; null until <see cref="ConnectLoop"/> succeeds, and while disconnected.</summary>
    private Room<FairState> _room;

    /// <summary>Colyseus client bound to <see cref="GetEndpoint"/>; created once per <see cref="ConnectLoop"/> run.</summary>
    private Client _client;

    /// <summary>Cancels the pending <see cref="ConnectLoop"/> retry delay and stops further retries on <see cref="OnDestroy"/>.</summary>
    private CancellationTokenSource _cancellation;

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
    /// Keeps trying to join the <c>fairsceneroom</c> as platform <c>"game"</c> until it succeeds or the
    /// object is destroyed. On success, subscribes to the room's <c>"updateFairState"</c> broadcasts
    /// (reorders made from the web app) and forwards them through <see cref="_updateGameFairState"/>,
    /// gated on <see cref="GameManager.IntroCompleted"/> so the tent order can't change mid-intro.
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

                _room.OnMessage<Dictionary<string, string>>("updateFairState", (fairState) =>
                {
                    if (GameManager.GetInstance().IntroCompleted)
                    {
                        _updateGameFairState.Invoke(fairState);
                    }
                });

                return;
            }
            catch (Exception exception)
            {
                _room = null;

                Debug.LogWarning($"ConnectToWebApp: could not connect to {endpoint} ({exception.Message}). " +
                                 "Check that the server is running and that Server Host is the LAN IP of the server machine, not localhost.");

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
    }

    /// <summary>Sends the current mini-game-name-to-slot-number map to the room so the web app mirrors the game's tent order.</summary>
    /// <param name="fairState">Mini-game name to placeholder slot number, as built by <c>TentPlaceHolderManager.GetFairState()</c>.</param>
    /// <remarks>Invoked via the Inspector in the <c>_updateOtherManagers</c> UnityEvent of the world-side <c>TentPlaceHolderManager</c> (<c>TentsPlaceHolderManager</c> GameObject) in the CountryFair scene.</remarks>
    public async void UpdateFairState(Dictionary<string, string> fairState)
    {
        if (_room != null)
        {
            try
            {
                await _room.Send("updateFairState", fairState);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ConnectToWebApp: failed to send fair state ({exception.Message}).");
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