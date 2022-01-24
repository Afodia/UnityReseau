using System;
using Mirror;
using UnityEngine;

public class MyNetworkPlayer : NetworkBehaviour
{
    public static event Action<int, float> OnMoneyChanged;

    int id;
    [SyncVar(hook = nameof(HandleMoneyChange))]float money = 2000000f;
    bool isFirstBoardTurn = true;
    int nbMonopole = 0;
    int currTile = 0;
    int nbJailTurn = 0;
    int nbConsecutiveDouble = 0;

    public override void OnStartServer()
    {
        // Debug.Log("connectionToClient.connectionId = " + connectionToClient.connectionId + " isclient = " + this.isClient + " isServer = " + this.isServer + " isLocalPlayer = " + this.isLocalPlayer);
        // id = connectionToClient.connectionId;
        // OnPlayerAdded?.Invoke();
    }

    #region Server

    [Server]
    public void MustSell(float needToBePaid)
    { }

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
        UIPanel.instance.ShowPanel(price, houses, lvl, money, connectionToClient.connectionId);
    }

    void HandleMoneyChange(float oldTotalMoney, float newTotalMoney)
    {
        OnMoneyChanged?.Invoke(connectionToClient.connectionId, newTotalMoney);
    }

    #endregion



    #region Getters & Setters

    [Server]
    public void SetId(int id)
    {
        this.id = id;
    }

    [Server]
    public int GetId()
    {
        return this.id;
    }

    [Server]
    public float GetMoney()
    {
        return this.money;
    }

    [Server]
    public void ChangeMoney(float amount)
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
    public int GetNbConsecutiveDouble()
    {
        return nbConsecutiveDouble;
    }

    [Server]
    public void ResNewDiceRoll(int resDice1, int resDice2)
    {
        if (resDice1 == resDice2)
            nbConsecutiveDouble++;
        else
            nbConsecutiveDouble = 0;
    }

    [Server]
    public void ResetNbConsecutiveDouble()
    {
        nbConsecutiveDouble = 0;
    }

    #endregion
}
