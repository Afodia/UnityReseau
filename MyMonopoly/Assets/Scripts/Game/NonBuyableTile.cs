using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NonBuyableTile : Tile
{
    [SerializeField] GameObject[] playerPos;

    #region Server

    [Server]
    public override void Action(MyNetworkPlayer player, int tileId)
    {
        switch (type) {
            case Type.GoToJail:
                GoToJailAction(player, tileId);
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
    private void GoToJailAction(MyNetworkPlayer player, int tileId)
    {
        player.SetInJail(true);
        player.SetTile(tileId);
        player.RpcSetPlayerAvatarPosition(GameManager.instance.GetTilePosition(tileId, player.GetPlayerId()));
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
        if (player.GetIsFirstBoardTurn())
            player.SetIsFirstBoardTurn(false);

        player.ChangeMoney(300000);
        // player.RpcUpdateDisplayMoneyOfPlayer(player.GetPlayerId(), player.GetMoney());
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
