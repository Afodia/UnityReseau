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
        Debug.Log("action !");
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
        currLvl = lvl;
        ownerId = pId;
        UpdateUI();
        return;
    }

    [ClientRpc]
    private void UpdateUI()
    {
        price.text = GameManager.instance.ChangePriceToText(data.rentPrice[currLvl]);
        house.sprite = houses[(ownerId - 1) + (currLvl * 4)];
        return;
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

    public override Vector3 GetPlayerPosition(int playerId)
    {
        return playerPos[playerId - 1].transform.position;
    }

    #endregion
}
