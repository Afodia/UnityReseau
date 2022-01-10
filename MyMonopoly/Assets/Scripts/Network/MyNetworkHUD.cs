using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

[RequireComponent(typeof(NetworkManager))]
public class MyNetworkHUD : NetworkBehaviour
{
    NetworkManager manager;
    [SerializeField] TMP_Text infosText;
    [SerializeField] TMP_InputField ipAddress;
    [SerializeField] Button connectGameButton;
    [SerializeField] Button joinGameButton;

    void Awake() {
        manager = GetComponent<NetworkManager>();
    }

    public void StartHostingGame() {
        manager.StartHost(); // starts both host and client
    }

    public void StopHostingGame() {
        manager.StopHost(); // stop both host and client
        infosText.text = "";
    }

    public void ConnectGame() {
        manager.networkAddress = ipAddress.text;
        connectGameButton.interactable = false;
        infosText.text = $"Connecting to {manager.networkAddress}...";
        manager.StartClient();
    }

    public void JoinGame() {
        NetworkClient.Ready();
        if (NetworkClient.localPlayer == null)
            NetworkClient.AddPlayer();
    }

    public void LeaveGame() {
        connectGameButton.interactable = true;
        manager.StopClient();
        if (joinGameButton.IsActive())
            joinGameButton.enabled = false;
        infosText.text = "";
    }

    void Update() {
        UpdateStatusLabels();

        if (NetworkClient.isConnected && !NetworkClient.ready && !joinGameButton.IsActive())
            joinGameButton.enabled = true;
    }

    void UpdateStatusLabels()
    {
        if (NetworkServer.active && NetworkClient.active) // host mode
            infosText.text = $"<b>Host</b>: running via {Transport.activeTransport}";
        else if (NetworkClient.isConnected) // client only
            infosText.text = $"<b>Client</b>: connected to {manager.networkAddress} via {Transport.activeTransport}";
        else if (!NetworkServer.active && !NetworkClient.active && infosText.text != "") // offline
            infosText.text = "";
    }
}
