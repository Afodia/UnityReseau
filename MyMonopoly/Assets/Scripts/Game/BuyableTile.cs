using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class BuyableTile : Tile
{
    [SerializeField] TilesData data;
    [Header("Do not touch")]
    [SerializeField] TextMeshPro cityName;
    [SerializeField] TextMeshPro price;
    [SerializeField] SpriteRenderer house;
    [SerializeField] GameObject playerPos;
    [SerializeField] Sprite[] houses = new Sprite[16];

    private bool isMonopole = false;
    private int ownerId = 0;
    private int currLvl = 0;

    #region Server
    //[Server]
    public override void Action(Player player)
    {
        if (ownerId == 0)
            UpgradeTile(player);
        else if (player.GetId() != ownerId)
            PayRent(player);
    }

    [Server]
    private void BuyTile(Player player)
    {
        if (ownerId != 0)
            return;
    }
    //[Server]
    private void UpgradeTile(Player player)
    {
        if (player.GetMoney() >= data.upgradePrice[0])
            player.OfferToUpgrade(data, new Sprite[4] { 
                houses[player.GetId()],
                houses[player.GetId() + 4 ],
                houses[player.GetId() + 8 ],
                houses[player.GetId() + 12]
            }, currLvl);
    }

    [Server]
    public void UpdateTile(int pId, int lvl)
    {
        currLvl = lvl;
        ownerId = pId;
        price.text = UIPanel.instance.ChangePriceToText(data.rentPrice[lvl]);
        house.sprite = houses[pId + (lvl * 4)];
    }

    [Server]
    private void PayRent(Player player)
    {
        float rent = GetRent();
    }

    [Server]
    private float GetRent()
    {
        return data.rentPrice[currLvl] * (System.Convert.ToSingle(isMonopole) + 1f);
    }

    #endregion

    #region Client

    private void Start()
    {
        cityName.text = data.tileName;
        house.sprite = null;
        price.text = "";
        if (overlay && over)
            over.sprite = overlay;
    }

    #endregion
}
