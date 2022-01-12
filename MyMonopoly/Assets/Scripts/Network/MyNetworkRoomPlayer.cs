using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class MyNetworkRoomPlayer : NetworkRoomPlayer
{
    #region Optional UI

    public override void OnGUI()
    {
        if (!showRoomGUI)
            return;

        NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
        if (room)
        {
            if (!room.showRoomGUI)
                return;

            if (!NetworkManager.IsSceneActive(room.RoomScene))
                return;

            DrawPlayerReadyState();
            DrawPlayerReadyButton();
        }
    }

    void DrawPlayerReadyState()
    {
        float[] posArray = new float[4] {(Screen.width - 100f) / 10 * 2, (Screen.width - 100f) / 10 * 4, (Screen.width - 100f) / 10 * 6, (Screen.width - 100f) / 10 * 8};
        GUILayout.BeginArea(new Rect(posArray[index], Screen.height / 2, 100f, 130f));
     // GUILayout.BeginArea(new Rect(20f + (index * 100), 200f, 90f, 130f));

        GUILayout.Label($"Player {index + 1}");

        if (readyToBegin)
            GUILayout.Label("Ready");
        else
            GUILayout.Label("Not Ready");

        if (((isServer && index > 0) || isServerOnly) && GUILayout.Button("REMOVE"))
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();

        GUILayout.EndArea();
    }

    void DrawPlayerReadyButton()
    {
        if (NetworkClient.active && isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 60f, Screen.height / 2f + 100f, 120f, 20f));

            if (readyToBegin) {
                if (GUILayout.Button("Cancel"))
                    CmdChangeReadyState(false);
            } else {
                if (GUILayout.Button("Ready"))
                    CmdChangeReadyState(true);
            }

            GUILayout.EndArea();
        }
    }

    #endregion
/*
    #region Optional UI

    GameObject canvas = null;
    [SerializeField] GameObject roomPlayerCardPrefab = null;
    GameObject roomPlayerCardInstance = null;
    TMP_Text playerNameText = null;
    TMP_Text playerStatusText = null;
    Button playerReadyButton = null;
    TMP_Text playerReadyButtonText = null;
    bool isReady = false;
    Button hostExcludeButton = null;

    void Awake()
    {
        NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
        if (room) {
            if (!NetworkManager.IsSceneActive(room.RoomScene))
                return;

            canvas = GameObject.Find("Canvas");
            if (!canvas)
                Debug.LogError("Canvas not found in scene");

            Vector3 canvaPosition = canvas.transform.position;
            Debug.Log($"{index}, {new Vector3(Mathf.Abs(canvaPosition.x) -345f + (index * 100), 284f, 0f)}");
            roomPlayerCardInstance = Instantiate(roomPlayerCardPrefab, new Vector3(Mathf.Abs(canvaPosition.x) -345f + (index * 100), 284f, 0f), Quaternion.identity, canvas.transform);
            playerNameText = roomPlayerCardInstance.transform.Find("PlayerName").GetComponent<TMP_Text>();
            playerStatusText = roomPlayerCardInstance.transform.Find("PlayerStatus").GetComponent<TMP_Text>();
            playerReadyButton = roomPlayerCardInstance.transform.Find("PlayerReadyButton").GetComponent<Button>();
            playerReadyButtonText = playerReadyButton.GetComponentInChildren<TMP_Text>();
            hostExcludeButton = roomPlayerCardInstance.transform.Find("HostExcludeButton").GetComponent<Button>();

            playerNameText.text = $"Player {index + 1}";
            playerReadyButton.onClick.AddListener(ChangeStatus);
            hostExcludeButton.onClick.AddListener(ExcludePlayer);

            if (isServer && index > 0)
                hostExcludeButton.gameObject.SetActive(true);
        }
    }
    void ChangeStatus()
    {
        isReady = !isReady;
        if (isReady) {
            CmdChangeReadyState(true);
            playerStatusText.text = "Player ready";
            playerReadyButtonText.text = "Cancel";
            playerStatusText.color = Color.green;
        } else {
            CmdChangeReadyState(false);
            playerStatusText.text = "Player not ready";
            playerReadyButtonText.text = "Ready !";
            playerStatusText.color = Color.red;
        }
    }

    void ExcludePlayer()
    {
        if (isServer && index > 0)
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
    }

    #endregion
*/
}
