using UnityEngine;
using Mirror;

public class MyNetworkRoomPlayer : NetworkRoomPlayer
{
    #region Room UI

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
}
