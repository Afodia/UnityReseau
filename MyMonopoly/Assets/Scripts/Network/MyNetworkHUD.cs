using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

namespace Mirror
{
    [RequireComponent(typeof(MyNetworkRoomManager))]
    public class MyNetworkHUD : MonoBehaviour
    {
        [Scene] [SerializeField] string mainMenuSceneName = "MainMenu";
        [SerializeField] TMP_Text infosText = null;

        [Header("Main menu join game")]
        [SerializeField] TMP_InputField ipAddress = null;
        [SerializeField] Button connectGameButton = null;
        [SerializeField] Button joinGameButton = null;

        public void StartHostingGame()
        {
            NetworkRoomManager.singleton.StartHost();
        }

        public void StopHostingGame()
        {
            NetworkRoomManager.singleton.StopHost();
            ReloadMainMenuScene();
        }

        public void ConnectGame()
        {
            NetworkManager.singleton.networkAddress = ipAddress.text;
            connectGameButton.interactable = false;
            infosText.text = $"Connecting to {NetworkManager.singleton.networkAddress}...";
            NetworkRoomManager.singleton.StartClient();
        }

        public void ReadyToJoinGame()
        {
            NetworkClient.Ready();
            if (NetworkClient.localPlayer == null)
                NetworkClient.AddPlayer();
        }

        public void LeaveGame()
        {
            NetworkRoomManager.singleton.StopClient();
            ReloadMainMenuScene();
        }

        public void ReloadMainMenuScene()
        {
            Destroy(this.gameObject);
            NetworkManager.singleton.ServerChangeScene(mainMenuSceneName);
        }

        void Update()
        {
            UpdateStatusLabels();

            if (NetworkClient.isConnected && !NetworkClient.ready && joinGameButton && !joinGameButton.IsActive())
                joinGameButton.enabled = true;
        }

        void UpdateStatusLabels()
        {
            if (NetworkServer.active && NetworkClient.active) // host mode
                infosText.text = $"<b>Host</b>: running via {Transport.activeTransport}";
            else if (NetworkClient.isConnected) // client only
                infosText.text = $"<b>Client</b>: connected to {NetworkManager.singleton.networkAddress} via {Transport.activeTransport}";
            else if (!NetworkServer.active && !NetworkClient.active && infosText.text != "") // offline
                infosText.text = "";
        }
    }
}