using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NonBuyableTile : Tile
{
    [SerializeField] GameObject[] playerPos;

    #region Server

    [Server]
    public override void Action(MyNetworkPlayer player)
    {
        switch (type) {
            case Type.GoToJail:
                GoToJailAction(player);
                break;
            case Type.Jail:
                JailAction(player);
                break;
            case Type.Luck:
                LuckAction(player);
                break;
            case Type.Square:
                SquareAction(player);
                break;
            case Type.Start:
                StartAction(player);
                break;
        }
    }

    [Server]
    private void GoToJailAction(MyNetworkPlayer player)
    {
        player.SetInJail(true);
    }

    [Server]
    private void JailAction(MyNetworkPlayer player)
    { }

    [Server]
    private void LuckAction(MyNetworkPlayer player)
    { }

    [Server]
    private void SquareAction(MyNetworkPlayer player)
    { }

    [Server]
    private void StartAction(MyNetworkPlayer player)
    {
        player.RpcChangeMoney(300000);
    }

    #endregion


    #region Client
    private void Start()
    {
        if (overlay && over)
            over.sprite = overlay;
    }

    public override Vector3 GetPlayerPosition(int playerId)
    {
        return playerPos[playerId - 1].transform.position;
    }

    #endregion
}
