using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class BuyableTile : Tile
{
    [SerializeField] TilesData data;
    [SerializeField] Image[] houses;

    private bool isMonopole = false;
    private int ownerId = 0;
    private int currLvl = 0;

    #region Server
    [Server]
    public override void Action(Player player)
    {
        if (ownerId == 0)
            UpgradeTile(player);
        else if (player.GetId() != ownerId)
            PayRent(player);
    }

    [Server]
    private bool BuyTile(Player player)
    {
        if (ownerId != 0)
            return false;
        return true;
    }
    [Server]
    private bool UpgradeTile(Player player)
    {
        if (player.GetMoney() >= data.upgradePrice[0])
            player.OfferToUpgrade(data.upgradePrice);
        return true;
    }
    [Server]
    private bool PayRent(Player player)
    {
        uint rent = GetRent();
        return true;
    }

    [Server]
    private uint GetRent()
    {
        return data.rentPrice[currLvl] * (System.Convert.ToInt32(isMonopole) + 1);
    }

    #endregion

    #region Client

    private void Start()
    {
        if (TryGetComponent<Text>(out Text name))
            name.text = data.tileName;
    }
    #endregion
}
