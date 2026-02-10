using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// Connects to a TCP server to listen for emoji commands.
/// </summary>
[RequireComponent(typeof(ActivateEmoji))]
public class ServerListener : MonoBehaviour
{   
    /// <summary>
    /// The IP address of the server to connect to.
    /// </summary>
    [SerializeField]
    private string serverIP = "172.20.10.2"; 

    /// <summary>
    /// The port number of the server to connect to.
    /// </summary>
    [SerializeField]
    private int serverPort = 50050;

    /// <summary>
    /// Reference to the emoji script. 
    /// </summary>
    private ActivateEmoji _emojiScript; 
    
    /// <summary>
    /// The TCP client used for the connection.
    /// </summary>
    private TcpClient _client;

    /// <summary>
    /// The thread used to listen for incoming data.
    /// </summary>
    private Thread _receiveThread;

    /// <summary>
    /// Unity Awake method. Initializes references.
    /// </summary>
    private void Awake()
    {
        _emojiScript = GetComponent<ActivateEmoji>();
    }

    /// <summary>
    /// Unity Start method. Initiates the connection.
    /// </summary>
    private void Start()
    {
        ConnectToTcpServer();
    }

    /// <summary>
    /// Establishes a connection to the TCP server.
    /// </summary>
    private void ConnectToTcpServer()
    {
        try 
        {
            _client = new TcpClient(serverIP, serverPort);
            _receiveThread = new Thread(ListenForData)
            {
                IsBackground = true
            };
            _receiveThread.Start();
            Debug.Log("Connected to external emotion server.");
        }
        catch (System.Exception e) 
        {
            Debug.LogError($"Error connecting: {e.Message}");
        }
    }

    /// <summary>
    /// Listens for incoming data on a separate thread.
    /// </summary>
    /// <remarks>
    /// This method runs on a separate thread (BE CAREFUL!).
    /// </remarks>
    private void ListenForData()
    {
        try 
        {
            byte[] bytes = new byte[1024];
            while (_client != null) 
            {
                using NetworkStream stream = _client.GetStream();
                int length;
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    var incomingData = new byte[length];
                    System.Array.Copy(bytes, 0, incomingData, 0, length);

                    string serverMessage = Encoding.ASCII.GetString(incomingData);
                    Debug.Log($"Received: {serverMessage}");

                    // DISPATCH TO MAIN THREAD
                    // Unity does not allow modifying GameObjects from another thread.
                    // We need a queue or scheduled action.
                    // Simplified (but 'dirty') way to understand the concept:
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        // HERE IS THE CALL YOU ASKED FOR:
                        if (_emojiScript != null)
                        {
                            _emojiScript.ProcessServerString(serverMessage);
                        }
                    });
                }
            }
        }
        catch (System.Exception e) 
        {
            Debug.Log($"Socket exception: {e.Message}");
        }
    }
}