using System;
using Mirror;

public class MyNetworkPlayer : NetworkBehaviour
{
    public static event Action<int, float> OnMoneyChanged;
    public static event Action OnPlayerAdded;

    int id;
    bool isFirstBoardTurn = true;
    float money = 2000000f;
    int nbMonopole = 0;
    int currTile = 0;
    int nbJailTurn = 0;
    int nbConsecutiveDouble = 0;

    void Awake()
    {
        id = (int)GetComponent<NetworkIdentity>().netId;
        OnPlayerAdded?.Invoke();
    }

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
        OnMoneyChanged?.Invoke(id, money);
    }

    [Server]
    public void MustSell(float needToBePaid)
    { }

    #endregion

    #region Client
    public void OfferToUpgrade(float[] upgradePrice)
    {
        //if (!hasAuthority)
        DisplayUpgradeOffer(upgradePrice);
    }

    [Client]
    private void DisplayUpgradeOffer(float[] price)
    {

    }

    #endregion
}
