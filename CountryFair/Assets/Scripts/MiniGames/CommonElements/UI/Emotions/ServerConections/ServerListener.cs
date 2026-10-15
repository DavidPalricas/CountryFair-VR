using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using Unity.VisualScripting;
using UnityEngine.Serialization;

/// <summary>
/// Connects to a TCP server to listen for emoji commands.
/// Handles connection lifecycle and threading.
/// </summary>
/// <remarks>
/// Also owns the choice of emotion display mode: one of the three displays is picked at random on load and the other
/// two are deactivated. A failed connection is non-fatal (logged as a warning), so the mini-games run without the server.
/// </remarks>
public class ServerListener : MonoBehaviour
{
    /// <summary>IP of the external emotion-recognition server.</summary>
    [Header("Server Connection Settings")]
    [SerializeField] 
    [FormerlySerializedAs("serverIP")]
    private string _serverIP = "172.20.10.2";
    /// <summary>TCP port of the emotion-recognition server.</summary>
    [SerializeField]
     [FormerlySerializedAs("serverPort")]
     private int _serverPort = 50050;
    /// <summary>Emoji-mode display.</summary>
    [SerializeField] 
    [FormerlySerializedAs("emojiDisplay")]
    private ExpressionDisplay _emojiDisplay;
    /// <summary>Slider-mode (cumulative mood bar) display.</summary>
    [SerializeField] 
    [FormerlySerializedAs("sliderDisplay")]
    private SliderDisplay _sliderDisplay;

    /// <summary>Face-mode display.</summary>
    [SerializeField]
    [FormerlySerializedAs("faceDisplay")]
    private ExpressionDisplay _faceDisplay;

    /// <summary>Seconds between main-thread polls of the latest received message; messages arriving in between are dropped except the last.</summary>
    [Header("Update Rate")]
    [Tooltip("Interval in seconds between processing received messages")]
    [FormerlySerializedAs("pollIntervalSeconds")]
    [SerializeField] private float _pollIntervalSeconds = 0.5f;

    /// <summary>
    /// Socket to the emotion server. Created on the connect thread in <see cref="ConnectToTcpServer"/>; null when the
    /// connection failed or after <see cref="OnDestroy"/> closed it. Reads time out after 5 s.
    /// </summary>
    private TcpClient _client;

    /// <summary>
    /// Background thread running <see cref="ListenForData"/>; only started after a successful connection. Marked as a
    /// background thread so it never keeps the app alive; it ends when <see cref="_isRunning"/> goes false or the socket closes.
    /// </summary>
    private Thread _receiveThread;

    /// <summary>
    /// Keep-running flag of the receive loop: set true by the connect thread on success, false by <see cref="OnDestroy"/>
    /// or a failed connection. <c>volatile</c> because it is written and read on different threads.
    /// </summary>
    /// <remarks>While false, socket errors are not logged, so closing the socket on scene exit does not raise warnings.</remarks>
    private volatile bool _isRunning = false;

    /// <summary>
    /// Latest-value buffer: the receive thread overwrites it, <see cref="FlushLatestMessage"/> takes it on the main thread.
    /// No lock is needed: at worst the main thread reads a slightly stale value, which is acceptable for an emotion display.
    /// </summary>
    private volatile string _latestMessage = null;

    /// <summary>The single active display that receives server messages.</summary>
    private EmotionDisplay _currentDisplayMode;

    /// <summary>Available emotion display modes.</summary>
    public enum DISPLAYMODE
    {
        Emoji,
        Face,
        Slider
    }

    /// <summary>Validates the three displays and picks the random display mode for this session.</summary>
    private void Awake()
    {
        if (_sliderDisplay == null || _emojiDisplay == null || _faceDisplay == null)
        {
            Debug.LogError("One or more types of emotion display are missing in ServerListener");
            return;
        }

        SetCurrentDisplayMode();
    }

    /// <summary>Starts the background connection and the repeating main-thread poll.</summary>
    private void Start()
    {
        ConnectToTcpServer();
        InvokeRepeating(nameof(FlushLatestMessage), _pollIntervalSeconds, _pollIntervalSeconds);
    }

    /// <summary>
    /// Called on the main thread every <see cref="_pollIntervalSeconds"/>.
    /// Atomically swaps the latest message out and dispatches it to the active display.
    /// </summary>
    /// <remarks>
    /// <see cref="Interlocked.Exchange{T}(ref T, T)"/> takes exactly one message and clears the buffer in one step, so a
    /// message is never processed twice. Does nothing if no new message arrived since the last poll.
    /// </remarks>
    private void FlushLatestMessage()
    {
        string message = Interlocked.Exchange(ref _latestMessage, null);
        if (message == null)
        {
            return;
        } 

        _currentDisplayMode.ProcessServerString(message);
    }

    /// <summary>
    /// Connects on a background thread (3 s timeout) so the main thread never blocks, then starts <see cref="ListenForData"/>
    /// on another background thread.
    /// </summary>
    private void ConnectToTcpServer()
    {
        string sceneName = gameObject.scene.name;

        Thread connectThread = new(() =>
        {
            try
            {
                _client = new TcpClient();
                IAsyncResult result = _client.BeginConnect(_serverIP, _serverPort, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                if (!success || !_client.Connected)
                {
                    _client.Close();
                    _client = null;
                    Debug.LogWarning($"[Optional] Could not connect to emotion server in scene {sceneName}. Timeout.");
                    return;
                }

                _client.EndConnect(result);
                _client.ReceiveTimeout = 5000;
                _isRunning = true;

                _receiveThread = new Thread(ListenForData) { IsBackground = true };
                _receiveThread.Start();
            }
            catch (Exception e)
            {
                _isRunning = false;
                Debug.LogWarning($"[Optional] Could not connect to emotion server in scene {sceneName}. Reason: {e.Message}");
            }
        })
        { IsBackground = true };

        connectThread.Start();
    }

    /// <summary>
    /// Receive loop (background thread): stores each chunk read from the socket as the latest message, overwriting any
    /// older unprocessed one on purpose.
    /// </summary>
    /// <remarks>Each read is treated as one whole message; there is no framing, so a message split across reads is not reassembled.</remarks>
    private void ListenForData()
    {
        try
        {
            byte[] bytes = new byte[1024];

            while (_isRunning && _client != null && _client.Connected)
            {
                using NetworkStream stream = _client.GetStream();
                int length;

                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    if (!_isRunning) break;

                    _latestMessage = Encoding.ASCII.GetString(bytes, 0, length);
                }
            }
        }
        catch (System.IO.IOException)
        {
            if (_isRunning)
                Debug.LogWarning("Connection lost with the server.");
        }
        catch (Exception e)
        {
            if (_isRunning)
                Debug.LogWarning($"Socket warning: {e.Message}");
        }
    }

    /// <summary>Stops polling, signals the receive loop to stop and closes the socket so the threads end with the scene.</summary>
    private void OnDestroy()
    {
        CancelInvoke(nameof(FlushLatestMessage));
        _isRunning = false;
        _client?.Close();
        _client = null;
    }

    /// <summary>Activates one display mode at random (about 1/3 each) and deactivates the other two.</summary>
    private void SetCurrentDisplayMode()
    {  
        float randomValue = Utils.RandomValueInRange(0f, 1f);

        if (randomValue <= 0.33f)
        {
           _faceDisplay.gameObject.SetActive(false);
           _sliderDisplay.gameObject.SetActive(false);

           _currentDisplayMode = _emojiDisplay;

            return;
        }

        if (randomValue <= 0.66f)
        {     
            _emojiDisplay.gameObject.SetActive(false);
            _faceDisplay.gameObject.SetActive(false);

            _currentDisplayMode = _sliderDisplay;

            return;
        }

        _emojiDisplay.gameObject.SetActive(false);
        _sliderDisplay.gameObject.SetActive(false);

        _currentDisplayMode = _faceDisplay;
    }

    /// <summary>Switches the active display mode at runtime.</summary>
    /// <remarks>Invoked via Inspector by the <c>_changeEmotionDisplay</c> event of <see cref="MiniGameCheatCodes"/> (emotion test cheats).</remarks>
    public void UpdateDisplayMode(DISPLAYMODE newMode)
    {   
        _currentDisplayMode.gameObject.SetActive(false);

        if (newMode == DISPLAYMODE.Slider)
        {   
            _currentDisplayMode = _sliderDisplay;
            _currentDisplayMode.gameObject.SetActive(true);

            return;
        }

        if (newMode == DISPLAYMODE.Face)
        {   
            _currentDisplayMode = _faceDisplay;
            _currentDisplayMode.gameObject.SetActive(true);

            return;
        }
        
       _currentDisplayMode = _emojiDisplay;
        _currentDisplayMode.gameObject.SetActive(true);
    }
}