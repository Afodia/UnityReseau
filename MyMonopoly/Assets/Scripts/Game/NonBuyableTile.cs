using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NonBuyableTile : Tile
{
    #region Server
    [Server]
    public override void Action(Player player)
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
    private void GoToJailAction(Player player)
    { }

    [Server]
    private void JailAction(Player player)
    { }

    [Server]
    private void LuckAction(Player player)
    { }

    [Server]
    private void SquareAction(Player player)
    { }

    [Server]
    private void StartAction(Player player)
    {
        player.ChangeMoney(300000);
    }

    #endregion

}
