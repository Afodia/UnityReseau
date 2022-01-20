using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : MonoBehaviour
{
    int id;
    bool isFirstBoardTurn = true;
    float money = 2000000f;
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
    public float GetMoney()
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
    public void OfferToUpgrade(TilesData upgradePrice, Sprite[] houses, int lvl)
    {
        //if (!hasAuthority)
        DisplayUpgradeOffer(upgradePrice, houses, lvl);
    }

    // [Client]
    private void DisplayUpgradeOffer(TilesData price, Sprite[] houses, int lvl)
    {
        UIPanel.instance.ShowPanel(price, houses, lvl);
    }

    #endregion
}
