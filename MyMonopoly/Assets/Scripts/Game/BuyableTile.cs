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
    [SerializeField] GameObject[] playerPos;
    [SerializeField] Sprite[] houses = new Sprite[16];

    private bool isMonopole = false;
    private int ownerId = 0;
    private int currLvl = 0;

    #region Server
    
    [Server]

    public override void Action(MyNetworkPlayer player, int tileId)
    {
        player.ChangeMoney(-200000);
        if (ownerId == 0 || ownerId == player.GetPlayerId())
            UpgradeTile(player);
        else if (player.GetPlayerId() != ownerId)
            PayRent(player);
        GameManager.instance.TileActionEnded();
    }

    [Server]
    private void BuyTile(MyNetworkPlayer player)
    {
        if (ownerId != 0)
            return;
    }

    [Server]
    private void UpgradeTile(MyNetworkPlayer player)
    {
        int[] toSend = new int[4];

        for (int i = 0 ; i < 4 ; i++)
            toSend[i] = player.GetPlayerId() - 1 + (i * 4);
        if (player.GetMoney() >= data.upgradePrice[currLvl])
            player.RpcDisplayUpgradeOffer(data, toSend, currLvl);
    }

    [Server]
    public void UpdateTile(int pId, int lvl)
    {
        this.currLvl = lvl;
        this.ownerId = pId;
        // TODO set isMonopole
        UpdateUI(pId, lvl);
        return;
    }

    [ClientRpc]
    private void UpdateUI(int pId, int lvl)
    {
        if (pId == 0) {
            price.text = "";
            house.sprite = null;
        } else {
            price.text = GameManager.instance.ChangePriceToText(data.rentPrice[lvl]);
            house.sprite = houses[(pId - 1) + (lvl * 4)];
        }
    }

    [Server]
    private void PayRent(MyNetworkPlayer player)
    {
        float rent = GetRent();

        player.ChangeMoney(-rent);
        return;
    }

    [Server]
    private float GetRent()
    {
        return data.rentPrice[currLvl] * (System.Convert.ToSingle(isMonopole) + 1f);
    }

    [Server]
    public float GetUpgrade(int lvl)
    {
        float toBuy = 0;
        for (int i = currLvl ; i <= lvl ; i++)
            toBuy += data.upgradePrice[i];
        return toBuy;
    }

    [Server]
    public int GetOwnerId()
    {
        return this.ownerId;
    }

    [Server]
    public override Vector3 GetPlayerPosition(int playerId)
    {
        return playerPos[playerId - 1].transform.position;
    }

    [Server]
    public string GetTileName()
    {
        return this.data.tileName;
    }

    [Server]
    public float GetSellPrice()
    {
        return this.data.sellPrice[currLvl];
    }

    [Server]
    public void SellTile(MyNetworkPlayer currentPlayer)
    {
        currentPlayer.ChangeMoney(GetSellPrice());
        UpdateTile(0, 0);
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
