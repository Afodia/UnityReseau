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
            Debug.Log("Start hosting game");
            NetworkRoomManager.singleton.StartHost();
        }

        public void StopHostingGame()
        {
            Debug.Log("Stop hosting game");
            NetworkRoomManager.singleton.StopHost();
            ReloadMainMenuScene();
        }

        public void ConnectGame()
        {
            Debug.Log("Connecting to game...");
            NetworkManager.singleton.networkAddress = ipAddress.text;
            connectGameButton.interactable = false;
            infosText.text = $"Connecting to {NetworkManager.singleton.networkAddress}...";
            NetworkRoomManager.singleton.StartClient();
        }

        public void ReadyToJoinGame()
        {
            Debug.Log("Joining game");
            NetworkClient.Ready();
            if (NetworkClient.localPlayer == null)
                NetworkClient.AddPlayer();
        }

        public void LeaveGame()
        {
            Debug.Log("Leaving game");
            NetworkRoomManager.singleton.StopClient();
            ReloadMainMenuScene();
        }

        void ReloadMainMenuScene()
        {
            Debug.Log("Reloading main menu scene");
            Destroy(this.gameObject);
            SceneManager.LoadScene(mainMenuSceneName);
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