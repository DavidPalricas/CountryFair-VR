using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Configures the Unity Transport IP and port, then auto-connects as a client on Start.
/// Call <see cref="StartHost"/> manually to launch a host instead (used on the therapist device).
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    /// <summary>Target server IP address injected into <see cref="UnityTransport"/> at startup.</summary>
    [SerializeField]
    private string _serverIp = "172.20.10.2";
    /// <summary>Target server port injected into <see cref="UnityTransport"/> at startup.</summary>
    [SerializeField]
    private ushort _serverPort = 50050;
    /// <summary>When true, <c>Start</c> automatically calls <see cref="NetworkManager.StartClient"/>.</summary>
    [SerializeField] private bool _autoConnectClient = true;

    /// <summary>Applies the IP/port to the transport and, if <see cref="_autoConnectClient"/> is set, joins as a Netcode client (the headset's role).</summary>
    private void Start()
    {
        ConfigureTransport();

        if (_autoConnectClient)
        {
            // Debug.Log($"Connecting to {_serverIp}:{_serverPort}...");
            NetworkManager.Singleton.StartClient();
        }
    }

    /// <summary>
    /// Configures the transport with the assigned IP/port and starts the NetworkManager as host.
    /// </summary>
    /// <remarks>Invoked via Inspector by a debug button, to launch the game as host on the therapist's machine.</remarks>
    public void StartHost()
    {
        ConfigureTransport();
        NetworkManager.Singleton.StartHost();
    }

    /// <summary>Injects <see cref="_serverIp"/>/<see cref="_serverPort"/> into the NetworkManager's <see cref="UnityTransport"/>; must run before starting client or host.</summary>
    private void ConfigureTransport()
    {
        if (NetworkManager.Singleton.TryGetComponent<UnityTransport>(out var transport))
        {
            transport.SetConnectionData(_serverIp, _serverPort);
        }
        else
        {
            Debug.LogError("UnityTransport não encontrado no NetworkManager!");
        }
    }
}