using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StopHostingGameMenuButton : MonoBehaviour
{
    GameObject myNetworkRoomManager = null;
    Mirror.MyNetworkHUD myNetworkHUD = null;
    Button thisButton = null;

    void Awake()
    {
        myNetworkRoomManager = GameObject.Find("MyNetworkRoomManager");
        if (myNetworkRoomManager == null) {
            Debug.LogError("Could not find MyNetworkRoomManager");
            return;
        }

        if (!myNetworkRoomManager.TryGetComponent<Mirror.MyNetworkHUD>(out myNetworkHUD)) {
            Debug.LogError("Could not find MyNetworkHUD");
            return;
        }

        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(myNetworkHUD.StopHostingGame);
    }
}
