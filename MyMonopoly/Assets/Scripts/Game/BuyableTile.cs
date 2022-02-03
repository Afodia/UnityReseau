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

    private bool isMonopoly = false;
    private int ownerId = 0;
    private int currLvl = -1;

    #region Server
    
    [Server]

    public override void Action(MyNetworkPlayer player, int tileId)
    {
        if (type == Type.Train)
            TrainTile();
        else {
            if (ownerId == 0 || ownerId == player.GetPlayerId())
                UpgradeTile(player);
            else if (player.GetPlayerId() != ownerId)
                PayRent(player);
        }
    }

    [Server]
    private void TrainTile()
    {
        Debug.Log("train tile");
        GameManager.instance.TileActionEnded();
    }

    [Server]
    private void BuyTile(MyNetworkPlayer player) // Pupose is to buy tile of another player
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

        if (player.GetMoney() >= data.upgradePrice[currLvl + 1])
            player.RpcDisplayUpgradeOffer(data, toSend, currLvl);
    }

    [Server]
    public void UpdateTile(int pId, int lvl)
    {
        this.currLvl = lvl;
        this.ownerId = pId;
        UpdateUI(pId, lvl);
        return;
    }

    [ClientRpc]
    private void UpdateUI(int pId, int lvl)
    {
        Debug.Log($"Updating UI for {data.tileName}, price: {data.upgradePrice[lvl]}, owner: {pId}, lvl: {lvl}, isMonopoly: {isMonopoly}");
        if (pId == 0) {
            price.text = "";
            house.sprite = null;
        } else {
            price.text = GameManager.instance.ChangePriceToText(data.rentPrice[lvl] * (System.Convert.ToSingle(isMonopoly) + 1f));
            house.sprite = houses[(pId - 1) + (lvl * 4)];
        }
        Debug.Log($"Updated UI for {data.tileName}, price: {data.upgradePrice[lvl]}, owner: {pId}, lvl: {lvl}, isMonopoly: {isMonopoly}");
    }

    [Server]
    private void PayRent(MyNetworkPlayer player)
    {
        float rent = GetRent();

        player.ChangeMoney(-rent);
        GameManager.instance.GetPlayer(ownerId).ChangeMoney(rent);

        GameManager.instance.ChangeMoneyDisplayed();
        GameManager.instance.TileActionEnded();
    }

    [Server]
    public TilesData GetData()
    {
        return this.data;
    }

    [ClientRpc]
    public void RpcSetData(TilesData data)
    {
        this.data = data;
    }

    [Server]
    private float GetRent()
    {
        return data.rentPrice[currLvl] * (System.Convert.ToSingle(isMonopoly) + 1f);
    }

    [Server]
    public float GetUpgrade(int lvl)
    {
        float toBuy = 0;
        for (int i = currLvl == -1 ? 0 : currLvl ; i <= lvl ; i++)
            toBuy += data.upgradePrice[i];
        return toBuy;
    }

    [Server]
    public int GetOwnerId()
    {
        return this.ownerId;
    }

    [Server]
    public int GetTileLevel()
    {
        return this.currLvl;
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
        UpdateTile(0, -1);
    }

    [Server]
    public void SetMonopoly(bool state)
    {
        this.isMonopoly = state;
        RpcSetMonopoly(state);
        UpdateTile(this.ownerId, this.currLvl);
    }

    [ClientRpc]
    public void RpcSetMonopoly(bool state)
    {
        this.isMonopoly = state;
    }

    [Server]
    public bool IsMonopoly()
    {
        return this.isMonopoly;
    }

    #endregion

    #region Client

    [Client]
    public string ClientGetTileName()
    {
        return this.data.tileName;
    }

    [Client]
    public float ClientGetSellPrice()
    {
        return this.data.sellPrice[currLvl];
    }


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
