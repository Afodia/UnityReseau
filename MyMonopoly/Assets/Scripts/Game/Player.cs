using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : MonoBehaviour
{
    int id;
    bool isFirstBoardTurn = true;
    uint money = 2000000;
    int nbMonopole = 0;
    int currTile = 0;
    int nbJailTurn = 0;
    int nbConsecutiveDouble = 0;

    #region Server
    [Server]
    public int GetId()
    {
        return id;
    }

    [Server]
    public uint GetMoney()
    {
        return money;
    }

    [Server]
    public void ChangeMoney(int amount)
    {
        if (amount > 0)
            money += (uint)amount;
        else if (money + amount >= 0)
            money -= (uint)amount;
        else {
            amount *= -1;
            amount -= (int)money;
            money = 0;
            MustSell(amount);
        }
    }

    [Server]
    public void MustSell(int needToBePaid)
    { }

    #endregion

    #region Client
    public void OfferToUpgrade(int[] upgradePrice)
    {
        //if (!hasAuthority)
        DisplayUpgradeOffer(upgradePrice);
    }

    [Client]
    private void DisplayUpgradeOffer(int[] price)
    {

    }

    #endregion
}
