using UnityEngine;
using UnityEngine.Events;
using Colyseus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ConnectToWebApp : MonoBehaviour
{
    [Header("Server Connection Settings")]
    // "localhost" resolves to the DEVICE the build runs on. On the Quest 3 that is the headset
    // itself, which runs no Colyseus server, so the connection silently fails. A standalone build
    // must point at the LAN IP of the machine running CountryFairWebApp/ServerSide
    // (ipconfig -> IPv4 Address of the Wi-Fi adapter the headset is also connected to).
    [SerializeField]
    [Tooltip("Use the IP of the PC's Mobile Hotspot (Settings > Network & Internet > Mobile Hotspot, then run ipconfig and check the virtual Wi-Fi adapter) since the server runs on that same PC. Connect the Quest to that hotspot too. Default Windows hotspot IP is 192.168.137.1; 'localhost' only ever works in the Editor.")]
    private string serverHost = "192.168.137.1";

    [SerializeField]
    [Tooltip("When running in the Editor, ignore Server Host and use localhost instead.")]
    private bool useLocalhostInEditor = true;

    [SerializeField]
    private int serverPort = 2567;

    [Header("Reconnection")]
    [SerializeField]
    [Tooltip("Seconds to wait before retrying after a failed connection attempt. 0 disables retrying.")]
    private float retryDelaySeconds = 3f;

    [SerializeField]
    private UnityEvent<Dictionary<string, string>> updateGameFairState;

    private static ConnectToWebApp instance = null;

    private Room<FairState> room;

    private Client _client;

    private CancellationTokenSource _cancellation;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            return;
        }


        Destroy(gameObject);
    }

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
        string host = serverHost;

#if UNITY_EDITOR
        if (useLocalhostInEditor)
        {
            host = "localhost";
        }
#endif

        if (string.IsNullOrWhiteSpace(host))
        {
            host = "localhost";
        }

        return $"ws://{host.Trim()}:{serverPort}";
    }

    /// <summary>
    /// Keeps trying to join the room until it succeeds or the object is destroyed.
    /// The original version awaited JoinOrCreate inside an async void Start, so on device any
    /// failure (wrong host, server not up yet, headset on another network) was swallowed and the
    /// room stayed null forever with no feedback.
    /// </summary>
    private async Task ConnectLoop(CancellationToken token)
    {
        string endpoint = GetEndpoint();
        string plataform = "game";

        _client = new Client(endpoint);

        while (!token.IsCancellationRequested)
        {
            try
            {
                room = await _client.JoinOrCreate<FairState>("fairsceneroom", new Dictionary<string, object> { { "platform", plataform } });

                if (token.IsCancellationRequested)
                {
                    await room.Leave();
                    return;
                }

                Debug.Log($"ConnectToWebApp: connected to {endpoint} (room {room.RoomId})");

                room.OnMessage<Dictionary<string, string>>("updateFairState", (fairState) =>
                {
                    if (GameManager.GetInstance().IntroCompleted)
                    {
                        updateGameFairState.Invoke(fairState);
                    }
                });

                return;
            }
            catch (Exception exception)
            {
                room = null;

                Debug.LogWarning($"ConnectToWebApp: could not connect to {endpoint} ({exception.Message}). " +
                                 "Check that the server is running and that Server Host is the LAN IP of the server machine, not localhost.");

                if (retryDelaySeconds <= 0f)
                {
                    return;
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }


    public async void UpdateFairState(Dictionary<string, string> fairState)
    {
        if (room != null)
        {
            try
            {
                await room.Send("updateFairState", fairState);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ConnectToWebApp: failed to send fair state ({exception.Message}).");
            }
        }
    }


    // Without this, stopping Play Mode (or quitting the build) tears down this object
    // without ever closing the socket, so the server only notices the game is gone after
    // the ping/pong heartbeat times out — the web client's switch back to the waiting
    // screen (CountryFairRoom.onLeave -> "gameDisconnected") is delayed by that same amount.
    private async void OnDestroy()
    {
        _cancellation?.Cancel();
        _cancellation?.Dispose();
        _cancellation = null;

        if (room != null)
        {
            await room.Leave();
            room = null;
        }
    }
}