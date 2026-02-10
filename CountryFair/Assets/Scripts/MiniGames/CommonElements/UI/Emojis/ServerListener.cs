using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ServerListener : MonoBehaviour
{
    // REFERÊNCIA AO SCRIPT DO EMOJI (Arrasta no Inspector)
    [SerializeField] private ActivateEmoji _emojiScript; 
    
    // Configurações da conexão crua
    private TcpClient _client;
    private Thread _receiveThread;
    private const string IP = "172.20.10.2"; // IP do teu servidor externo
    private const int PORT = 50050;         // Porta do servidor externo

    private void Start()
    {
        ConnectToTcpServer();
    }

    private void ConnectToTcpServer()
    {
        try 
        {
            _client = new TcpClient(IP, PORT);
            _receiveThread = new Thread(ListenForData)
            {
                IsBackground = true
            };
            _receiveThread.Start();
            Debug.Log("Conectado ao servidor de emocoes externo.");
        }
        catch (System.Exception e) 
        {
            Debug.LogError($"Erro ao conectar: {e.Message}");
        }
    }

    // Este método corre numa thread separada (CUIDADO!)
    private void ListenForData()
    {
        try 
        {
            byte[] bytes = new byte[1024];
            while (_client != null) 
            {
                using (NetworkStream stream = _client.GetStream()) 
                {
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) 
                    {
                        var incomingData = new byte[length];
                        System.Array.Copy(bytes, 0, incomingData, 0, length);
                        
                        // 1. Receber string crua
                        string serverMessage = Encoding.ASCII.GetString(incomingData);
                        Debug.Log($"Recebido: {serverMessage}");

                        // 2. DISPARAR PARA A MAIN THREAD
                        // Unity não permite mexer em GameObjects a partir de outra Thread.
                        // Precisamos de uma fila ou de uma ação agendada.
                        // Forma simplificada (mas "suja") para entenderes o conceito:
                        MainThreadDispatcher.Enqueue(() => 
                        {
                            // AQUI É A CHAMADA QUE PERGUNTASTE:
                            if (_emojiScript != null)
                            {
                                _emojiScript.ProcessServerString(serverMessage);
                            }
                        });
                    }
                }
            }
        }
        catch (System.Exception e) 
        {
            Debug.Log($"Socket exception: {e.Message}");
        }
    }
}