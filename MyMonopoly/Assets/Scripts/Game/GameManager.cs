using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject[] Tiles;
    public static GameManager instance;
    private GameManager() { }

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);

        instance = this;
    }

    enum Phase
    {
        WaitingStart,
        LaunchDice,
        Move,
        TileAction,
        NextTurn
    }

    Phase currPhase = Phase.WaitingStart;
    List<MyNetworkPlayer> networkPlayers;// = new List<MyNetworkPlayer>();
    MyNetworkPlayer currPlayer;
    int nbDouble = 0;
    bool playAgain = false;
    int diceResult = 0;

    #region Server

    [Server]
    public void StartGame(List<MyNetworkPlayer> players)
    {
        int rand = Random.Range(0, players.Count - 1);
        int i = 1;

        Debug.Log("in game manager startGame");
        foreach (MyNetworkPlayer p in players) {
            Debug.Log(p);
            Debug.Log(p.GetId());
            //p.SetId(i);
            //i++;
        }

        networkPlayers = players;
        currPlayer = players[rand];

        currPhase = Phase.LaunchDice;
        PhaseChange();
    }

    [Server]
    void PhaseChange()
    {
        switch (currPhase) {
            case Phase.LaunchDice:
                OnLaunchDicePhase();
                break;
            case Phase.Move:
                OnMovePhase();
                break;
            case Phase.TileAction:
                OnTileActionPhase();
                break;
            case Phase.NextTurn:
                OnNextTurnPhase();
                break;
        }
    }

    [Command(requiresAuthority = false)]
    void CmdLaunchDice()
    {
        RollDices();
    }

    [Server]
    void OnLaunchDicePhase()
    {
        currPlayer.ShowDiceButton();
    }

    [Server]
    void OnMovePhase()
    {
        Debug.Log("must move of ");
        Debug.Log(diceResult);
        int newPos = currPlayer.GetTile() + diceResult;

        if (newPos > Tiles.Length) {
            Tiles[0].GetComponent<Tile>().Action(currPlayer);
            newPos %= Tiles.Length;
        }
        currPlayer.SetTile(newPos);
        // visually move player
        currPhase = Phase.TileAction;
        PhaseChange();
    }
    
    [Server]
    void OnTileActionPhase()
    {
        Tiles[currPlayer.GetTile()].GetComponent<Tile>().Action(currPlayer);
        currPhase = playAgain ? Phase.LaunchDice : Phase.NextTurn;
        PhaseChange();
    }

    [Server]
    void OnNextTurnPhase()
    {

    }


    [Server]
    void RollDices()
    {
        Debug.Log("rollDice");
        currPlayer.HideDiceButton();
        currPlayer.RpcShowDices();

        int resDice1 = Random.Range(1, 6);
        int resDice2 = Random.Range(1, 6);

        currPlayer.RpcRollDices(resDice1, resDice2);

        diceResult = resDice1 + resDice2;
        playAgain = false;
        if (resDice1 == resDice2) {
            nbDouble += 1;
            playAgain = true;
        }
        if (nbDouble >= 3)
            Tiles[24].GetComponent<Tile>().Action(currPlayer);
        currPhase = Phase.Move;
        PhaseChange();
        //currentPlayer.ResNewDiceRoll(resDice1, resDice2);
        //if (currentPlayer.GetNbConsecutiveDouble() == 3) {
        // Go to jail
        // then connectionToClient.identity.GetComponent<MyNetworkPlayer>().ResetNbConsecutiveDouble();
    }

    #endregion

    #region Client

    public void LaunchDice()
    {
        CmdLaunchDice();
    }

    #endregion

    //#region Client

    //[Header("Server Game Settings")]
    //    GameObject myNetworkRoomManagerObj = null;
    //    MyNetworkRoomManager myNetworkRoomManagerScript = null;
    //    List<MyNetworkPlayer> networkPlayers = new List<MyNetworkPlayer>();
    //    int currentPlayerIndex;
    //    MyNetworkPlayer currentPlayer = null;
    //    int currentPlayerTurn = 0;
    //    bool gameHasStarted = false;
    //    [SerializeField] RollTheDice diceRoller1 = null;
    //    [SerializeField] RollTheDice diceRoller2 = null;

    //// void Awake()
    //// {
    ////     // Get connectionToClient.identity.GetComponent<MyNetworkPlayer>() as local object ?
    ////     myNetworkRoomManagerObj = GameObject.Find("MyNetworkRoomManager");
    ////     Debug.Log("GameManager Awake: myNetworkRoomManagerObj = " + (myNetworkRoomManagerObj == null ? "null" : "not null"));
    ////     MyNetworkRoomManager.OnAllGamePlayersReady += HandleAllGamePlayersReady;

    ////     MyNetworkPlayer.OnPlayerAdded += OnPlayerAdd;
    ////     OnPlayerAdd();
    //// }

    //// public override void OnStartClient()
    //void Start()
    //{
    //    myNetworkRoomManagerObj = GameObject.Find("MyNetworkRoomManager");
    //    myNetworkRoomManagerScript = myNetworkRoomManagerObj.GetComponent<MyNetworkRoomManager>();

    //    // MyNetworkRoomManager.OnPlayerAdded += OnPlayerAdd;
    //    MyNetworkRoomManager.OnAllGamePlayersReady += HandleAllGamePlayersReady;

    //    MyNetworkPlayer.OnMoneyChanged += UpdateDisplayMoneyOfPlayer;

    //    UIPanel.OnPlayerBoughtUpgrade += TargetPlayerBoughtUpgrade;

    //    currentPlayerIndex = myNetworkRoomManagerScript.clientIndex;
    //    this.CmdGetPlayersFromServer();
    //    foreach (MyNetworkPlayer player in networkPlayers) {
    //        // Debug.Log($"playerid = {player.GetId()}, currentplayer index = {currentPlayerIndex}");
    //        if (player.hasAuthority) {
    //            currentPlayer = player;
    //            break;
    //        }
    //    }
    //}

    //void OnDestroy()
    //{
    //    MyNetworkRoomManager.OnAllGamePlayersReady += HandleAllGamePlayersReady;
    //    // MyNetworkRoomManager.OnPlayerAdded -= OnPlayerAdd;

    //    MyNetworkPlayer.OnMoneyChanged -= UpdateDisplayMoneyOfPlayer;

    //    UIPanel.OnPlayerBoughtUpgrade -= TargetPlayerBoughtUpgrade;
    //}

    //[Header("Client Game Settings")]
    //    [SerializeField] GameObject[] playersUI = new GameObject[4];
    //    [SerializeField] GameObject dicesContainer = null;


    //[Header("Client menus")]
    //    [SerializeField] GameObject PauseMenu;
    //    [SerializeField] GameObject WinLoseMenu;
    //    [SerializeField] TMP_Text WinLoseText;

    //void OnPlayerAdd()
    //{
    //    GameObject[] players = SortPlayerReferencesById(GameObject.FindGameObjectsWithTag("Player"));
    //    networkPlayers.Clear();

    //    for (int i = 0; i < players.Length; i++) {
    //        networkPlayers.Add(players[i].GetComponent<MyNetworkPlayer>());
    //        playersUI[i].SetActive(true);
    //    }
    //}

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //        PauseMenu.SetActive(!PauseMenu.activeSelf);

    //    // if (CountHowManyPlayersUiAreActive() != NetworkServer.connections.Count && !gameHasStarted)
    //    //     OnPlayerAdd();

    //    // if (CountHowManyPlayersUiAreActive() == NetworkServer.connections.Count && !gameHasStarted) {
    //    //     gameHasStarted = true;
    //    //     NextTurn(this.actualPlayerTurn);
    //    // }
    //}

    //[ClientRpc]
    //public void RpcPlayerNewTurnStarts(int id)
    //{
    //    currentPlayerIndex = id;

    //    if (currentPlayerIndex != id)
    //        return;

    //    dicesContainer.SetActive(true);
    //}


    //[Command]
    //public void CmdRollDices()
    //{
    //    if (currentPlayerIndex != currentPlayerTurn)
    //        return;

    //    myNetworkRoomManagerScript.RollDices();
    //}

    //[TargetRpc]
    //void TargetPlayerBoughtUpgrade(int playerId, int upgradeLvl)
    //{
    //    Debug.Log("TargetPlayerBoughtUpgrade");
    //}

    //[Command]
    //void CmdEndTurn()
    //{
    //    if (currentPlayerIndex != currentPlayerTurn)
    //        return;

    //    myNetworkRoomManagerScript.EndTurn();
    //}

    //[ClientRpc]
    //public void RpcRollDices(int resDice1, int resDice2)
    //{
    //    if (!dicesContainer.activeSelf)
    //        return;

    //    StartCoroutine(DisplayDicesRolling(resDice1, resDice2));
    //    this.CmdEndTurn();
    //}

    //[ClientRpc]
    //public void RpcPlayerWin(int playerId, string reason)
    //{
    //    WinLoseMenu.SetActive(true);

    //    if (playerId != this.currentPlayerIndex) {
    //        WinLoseText.text = $"Player {playerId} won because {reason}, so... you lost";
    //        return;
    //    }

    //    WinLoseText.text = $"You won because {reason}";
    //}

    //[TargetRpc]
    //public void TargetPlayerLose(int playerId, string reason)
    //{
    //    if (playerId != this.currentPlayerIndex)
    //        return;

    //    WinLoseMenu.SetActive(true);
    //    WinLoseText.text = $"You lose because {reason}";
    //}

    //#endregion
    //#region Client Utils

    //[Server]
    //void HandleAllGamePlayersReady()
    //{
    //    Debug.Log("HandleAllGamePlayersReady");
    //    for (int i = 0; i < networkPlayers.Count; i++)
    //        playersUI[i].SetActive(true);
    //}

    //[Command]
    //void CmdGetPlayersFromServer()
    //{
    //    networkPlayers = MyNetworkRoomManager.instance.GetPlayers();
    //    // networkPlayers = myNetworkRoomManagerScript.GetPlayers();
    //    Debug.Log("networkPlayers is null ? " + (networkPlayers == null ? "null" : "not null") + " networkPlayers.Count = " + networkPlayers.Count);
    //}

    //GameObject[] SortPlayerReferencesById(GameObject[] players)
    //{
    //    for (int i = 0; i < players.Length; i++) {
    //        for (int j = i + 1; j < players.Length; j++) {
    //            if (players[i].GetComponent<MyNetworkPlayer>().GetId() > players[j].GetComponent<MyNetworkPlayer>().GetId()) {
    //                GameObject tmp = players[i];
    //                players[i] = players[j];
    //                players[j] = tmp;
    //            }
    //        }
    //    }

    //    return players;
    //}

    //void UpdateDisplayMoneyOfPlayer(int id, float money)
    //{
    //    TMP_Text playerMoneyText = playersUI[id].transform.Find("PlayerMoney").GetComponent<TMP_Text>();
    //    playerMoneyText.text = $"$ {SplitNumberString3By3(money.ToString())}";
    //}

    //string SplitNumberString3By3(string numberString)
    //{
    //    string result = "";
    //    int i = numberString.Length - 1;

    //    while (i >= 0) {
    //        result = numberString[i] + result;
    //        i -= 3;
    //        if (i >= 0)
    //            result = " " + result;
    //    }
    //    return result;
    //}

    //[Client]
    //IEnumerator DisplayDicesRolling(int resDice1, int resDice2)
    //{
    //    yield return diceRoller1.Roll(resDice1);
    //    yield return diceRoller2.Roll(resDice2);
    //    yield return new WaitForSeconds(2.5f);
    //    dicesContainer.SetActive(false);
    //}

    //[Client]
    //int CountHowManyPlayersUiAreActive()
    //{
    //    int count = 0;
    //    for (int i = 0; i < playersUI.Length; i++) {
    //        if (playersUI[i].activeSelf)
    //            count++;
    //    }
    //    return count;
    //}

    //#endregion
}
