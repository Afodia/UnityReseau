using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

namespace Mirror
{
    [RequireComponent(typeof(NetworkManager))]
    public class MyNetworkHUD : MonoBehaviour
    {
        NetworkManager manager;
        [SerializeField] string mainMenuSceneName = "MainMenu";
        [SerializeField] TMP_Text infosText;

        [Header("Join game")]
        [SerializeField] TMP_InputField ipAddress;
        [SerializeField] Button connectGameButton;
        [SerializeField] Button joinGameButton;

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }

        public void StartHostingGame()
        {
            NetworkManager.singleton.StartHost(); // starts both host and client
        }

        public void StopHostingGame()
        {
            NetworkManager.singleton.StopHost(); // stop both host and client
            ReloadMainMenuScene();
        }

        public void ConnectGame()
        {
            NetworkManager.singleton.networkAddress = ipAddress.text;
            connectGameButton.interactable = false;
            infosText.text = $"Connecting to {NetworkManager.singleton.networkAddress}...";
            NetworkManager.singleton.StartClient();
        }

        public void JoinGame()
        {
            NetworkClient.Ready();
            if (NetworkClient.localPlayer == null)
                NetworkClient.AddPlayer();
        }

        public void LeaveGame()
        {
            NetworkManager.singleton.StopClient();
            ReloadMainMenuScene();
        }

        void ReloadMainMenuScene()
        {
            if (manager)
                Destroy(manager.transform.gameObject);
            SceneManager.LoadScene(mainMenuSceneName);
        }

        void Update()
        {
            UpdateStatusLabels();

            if (NetworkClient.isConnected && !NetworkClient.ready && !joinGameButton.IsActive())
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