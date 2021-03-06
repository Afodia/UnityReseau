using System;
using System.Globalization;
using System.Collections;
using Mirror;
using UnityEngine;
using TMPro;

public class MyNetworkPlayer : NetworkBehaviour
{
    int playerId = 0;
    int clientId = 0;
    NetworkConnection conn;
    float money = 2000000f;
    bool isFirstBoardTurn = true;
    int nbMonopolies = 0;
    int currTile = 0;
    int nbJailTurn = 0;
    bool inJail = false;

    [SerializeField] GameObject[] playersUI = new GameObject[4];
    [Header("Dices")]
    [SerializeField] GameObject Dices;
    [SerializeField] GameObject DicesButton;

    [Header("Menus")]
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject WinLoseMenu;
    [SerializeField] TMP_Text WinLoseText;

    [Header("UI")]
    [SerializeField] GameObject PlayerAvatar;
    [SerializeField] SpriteRenderer PlayerAvatarColor;

    public bool isReady = false;
    public bool launchingDice = true;

    public bool mustSell = false;

    #region Server

    void Start()
    {
        isReady = true;
    }

    void Update()
    {
       if (isLocalPlayer && Input.GetKeyDown(KeyCode.Escape))
           PauseMenu.SetActive(!PauseMenu.activeSelf);
    }

    [Server]
    public void MustSell(float needToBePaid)
    {
        float ownedTilesSellValue = GameManager.instance.GetTotalSellValueOfPlayerOwnedTiles(this.playerId);

        if (this.money + ownedTilesSellValue < needToBePaid) {
            GameManager.instance.SellAllOwnedTilesOfPlayer(this);
            TargetPlayerLose(this.playerId,
            "don't have enought money to pay what you owe\n" +
            $"You had $ {this.money} on your pocket\n" +
            $"Your cities sell value was $ {ownedTilesSellValue}\n" +
            $"And you owed $ {needToBePaid}");
            GameManager.instance.OnPlayerLose();
        } else {
            this.mustSell = true;
            GetComponent<UIPanel>().TargetShowSellPanel(GameManager.instance.GetPlayerOwnedTiles(this.playerId), needToBePaid);
        }
    }

    #endregion
    #region Client

    [ClientRpc]
    public void RpcRollDices(int resDice1, int resDice2)
    {
        StartCoroutine(DisplayDicesRolling(resDice1, resDice2));
    }


    #endregion
    #region Client UI

    [TargetRpc]
    public void TargetSetPlayersUi(int nbPlayers)
    {
        if (!isLocalPlayer)
            return;

        for (int i = 0; i < nbPlayers; i++)
            playersUI[i].SetActive(true);

        playersUI[this.playerId - 1].transform.Find("PlayerName").GetComponent<TMP_Text>().text = $"Player {this.playerId} (me)";
    }

    [ClientRpc]
    public void RpcShowDices()
    {
        Dices.SetActive(true);
    }

    [ClientRpc]
    public void RpcHideDices()
    {
        Dices.SetActive(false);
    }

    [TargetRpc]
    public void TargetShowDiceButton()
    {
        DicesButton.SetActive(true);
    }

    [TargetRpc]
    public void TargetHideDiceButton()
    {
        DicesButton.SetActive(false);
    }

    [TargetRpc]
    public void SetIsFirstBoardTurn(bool value)
    {
        this.isFirstBoardTurn = value;
    }

    [Server]
    public bool GetIsFirstBoardTurn()
    {
        return this.isFirstBoardTurn;
    }

    [TargetRpc]
    public void SetNbMonopolies(int nbMonopolies)
    {
        this.nbMonopolies = nbMonopolies;
    }

    [Server]
    public int GetNbMonopolies()
    {
        return this.nbMonopolies;
    }

    [Client]
    IEnumerator DisplayDicesRolling(int resDice1, int resDice2)
    {
        launchingDice = true;
        Dices.GetComponentsInChildren<RollTheDice>()[0].Roll(resDice1);
        Dices.GetComponentsInChildren<RollTheDice>()[1].Roll(resDice2);
        yield return new WaitUntil(() => !Dices.GetComponentsInChildren<RollTheDice>()[0].isRolling);
        yield return new WaitUntil(() => !Dices.GetComponentsInChildren<RollTheDice>()[1].isRolling);
        yield return new WaitForSeconds(1f);
        launchingDice = false;
    }

    [Client]
    public void ResetLaunchingDice()
    {
        launchingDice = true;
    }

    [TargetRpc]
    public void RpcDisplayUpgradeOffer(TilesData price, int[] houses, int lvl)
    {
        GetComponent<UIPanel>().ShowUpgradePanel(price, houses, lvl, money);
    }

    [TargetRpc]
    public void RpcDisplayBuyBeachOffer(TilesData tileData)
    {
        GetComponent<UIPanel>().ShowBuyBeachPanel(tileData);
    }

    [TargetRpc]
    public void TargetDisplayLuckCards(CardsData card)
    {
        GetComponent<UIPanel>().ShowLuckCard(card);
    }

    [TargetRpc]
    public void TargetUpdateDisplayMoneyOfPlayer(int id, float money)
    {
        TMP_Text playerMoneyText = playersUI[id - 1].transform.Find("PlayerMoney").GetComponent<TMP_Text>();
        playerMoneyText.text = $"$ {money.ToString("N0", CultureInfo.GetCultureInfo("en-US")).Replace(',', ' ')}";
    }

    [TargetRpc]
    public void TargetPlayerWin(int playerId, string reason)
    {
       WinLoseMenu.SetActive(true);

       if (playerId == this.playerId)
           WinLoseText.text = $"You won because you {reason}";
       else
           WinLoseText.text = $"Player {playerId} won because \"{reason}\" So... you lost.";
    }

    [TargetRpc]
    public void TargetPlayerLose(int playerId, string reason)
    {
        if (playerId != this.playerId)
            return;

       WinLoseMenu.SetActive(true);
       WinLoseText.text = $"You lose because you {reason}";
    }

    #endregion
    #region Getters & Setters

    [Server]
    public void SetPlayerId(int id)
    {
        this.playerId = id;
    }

    [TargetRpc]
    public void TargetSetPlayerId(int id)
    {
        this.playerId = id;
    }

    [Server]
    public int GetPlayerId()
    {
        return this.playerId;
    }

    [Server]
    public void SetClientId(int id)
    {
        this.clientId = id;
    }

    [Server]
    public int GetClientId()
    {
        return this.clientId;
    }

    [Server]
    public void SetTile(int tile)
    {
        this.currTile = tile;
    }

    [Server]
    public int GetTile()
    {
        return this.currTile;
    }
    
    [Server]
    public void SetConn(NetworkConnection conn)
    {
        this.conn = conn;
    }

    [Server]
    public NetworkConnection GetConn()
    {
        return this.conn;
    }

    [Server]
    public void SetInJail(bool isInJail)
    {
        this.inJail = isInJail;
    }

    [Server]
    public bool isInJail()
    {
        return this.inJail;
    }

    [Server]
    public void IncreaseNbTurnInJail()
    {
        this.nbJailTurn++;
    }

    [Server]
    public void ResetNbTurnInJail()
    {
        this.nbJailTurn = 0;
    }

    [Server]
    public int GetNbTurnInJail()
    {
        return this.nbJailTurn;
    }

    [Server]
    public float GetMoney()
    {
        return this.money;
    }

    [Server]
    public void SetMoney(float value)
    {
        this.money = value;
        RpcSetPlayerMoney(this.money);
    }

    [ClientRpc]
    public void RpcSetPlayerAvatarPosition(Vector3 newPosition)
    {
        PlayerAvatar.transform.position = new Vector3(newPosition.x, newPosition.y, 0);
    }

    [ClientRpc]
    public void RpcDisablePlayerAvatar()
    {
        PlayerAvatar.SetActive(false);
    }

    [ClientRpc]
    public void RpcSetPlayerAvatarColor(Color color)
    {
        PlayerAvatarColor.color = color;
    }

    [ClientRpc]
    void RpcSetPlayerMoney(float money)
    {
        this.money = money;
    }

    [Server]
    public void ChangeMoney(float amount)
    {
        if (amount > 0) {
            money += amount;
            RpcSetPlayerMoney(this.money);
        } else if (money + amount >= 0) {
            money += amount;
            RpcSetPlayerMoney(this.money);
        } else {
            amount *= -1;
            amount -= money;
            money = 0;
            RpcSetPlayerMoney(this.money);
            MustSell(amount);
        }
    }

    #endregion
}
