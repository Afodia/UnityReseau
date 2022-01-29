using System;
using System.Globalization;
using System.Collections;
using Mirror;
using UnityEngine;
using TMPro;

public class MyNetworkPlayer : NetworkBehaviour
{
    [SerializeField] int playerId = 0;
    /*[SerializeField]*/ int clientId = 0;
    float money = 2000000f;
    NetworkConnection conn;
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

    }

    #endregion
    #region Client

    [ClientRpc]
    public void RpcRollDices(int resDice1, int resDice2)
    {
        StartCoroutine(DisplayDicesRolling(resDice1, resDice2));
    }

    public void OfferToUpgrade(TilesData upgradePrice, Sprite[] houses, int lvl)
    {
        //if (!hasAuthority)
        DisplayUpgradeOffer(upgradePrice, houses, lvl);
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

    [Client]
    private void DisplayUpgradeOffer(TilesData price, Sprite[] houses, int lvl)
    {
        //UIPanel.instance.ShowPanel(price, houses, lvl, money, connectionToClient.connectionId);
    }

    [TargetRpc]
    public void UpdateDisplayMoneyOfPlayer(int id, float money)
    {
        TMP_Text playerMoneyText = playersUI[id - 1].transform.Find("PlayerMoney").GetComponent<TMP_Text>();
        playerMoneyText.text = $"$ {money.ToString("N0", CultureInfo.GetCultureInfo("en-US"))}";
    }

    [ClientRpc]
    public void RpcPlayerWin(int playerId, string reason)
    {
       WinLoseMenu.SetActive(true);

       if (playerId != this.playerId) {
           WinLoseText.text = $"Player {playerId} won because {reason}, so... you lost";
           return;
       }

       WinLoseText.text = $"You won because {reason}";
    }

    [TargetRpc]
    public void TargetPlayerLose(int playerId, string reason)
    {
        if (playerId != this.playerId)
            return;

       WinLoseMenu.SetActive(true);
       WinLoseText.text = $"You lose because {reason}";
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

    [ClientRpc]
    public void RpcSetPlayerAvatarPosition(Vector3 newPosition)
    {
        PlayerAvatar.transform.position = new Vector3(newPosition.x, newPosition.y, 0);
    }

    [ClientRpc]
    public void RpcSetPlayerAvatarColor(Color color)
    {
        PlayerAvatarColor.color = color;
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
            amount -= money;
            money = 0;
            MustSell(amount);
        }

//        RpcUpdateDisplayMoneyOfPlayer(this.playerId, this.money);
    }

    #endregion
}
