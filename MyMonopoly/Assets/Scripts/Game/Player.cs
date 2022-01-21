using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : MonoBehaviour
{
    int id;
    bool isFirstBoardTurn = true;
    public float money = 2000000f;
    int nbMonopole = 0;
    int currTile = 0;
    int nbJailTurn {get; set;} = 0;
    int nbConsecutiveDouble = 0;

    #region Server
    //[Server]

    //[Server]
    public int GetId()
    {
        return id;
    }

    //[Server]
    public float GetMoney()
    {
        return money;
    }

    //[Server]:
    public void ChangeMoney(int amount)
    {
        if (amount > 0)
            money += amount;
        else if (money + amount >= 0)
            money -= amount;
        else {
            amount *= -1;
            amount -= (int)money;
            money = 0;
            MustSell(amount);
        }
    }

    [Server]
    public void MustSell(int needToBePaid)
    {
        Debug.Log(needToBePaid);
    }

    #endregion

    #region Client
    public void OfferToUpgrade(TilesData upgradePrice, Sprite[] houses, int lvl)
    {
        //if (!hasAuthority)
        DisplayUpgradeOffer(upgradePrice, houses, lvl);
    }

    //[Client]
    private void DisplayUpgradeOffer(TilesData price, Sprite[] houses, int lvl)
    {
        UIPanel.instance.ShowPanel(price, houses, lvl, money, id);
    }

    #endregion
}
