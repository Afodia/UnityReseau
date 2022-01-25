using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject[] Tiles;
    public static GameManager instance;
    private GameManager() { }
    public static event Action OnAllPlayersReadyEventReceived;

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

        networkPlayers = players;
        currPlayer = players[rand];

        foreach (MyNetworkPlayer p in networkPlayers)
            currPlayer.RpcSetPlayersUi(networkPlayers.Count);

        MyNetworkRoomManager.OnAllGamePlayersReady += OnAllPlayersReady;
        MyNetworkPlayer.OnPlayerReady += OnPlayerReady;

        currPhase = Phase.LaunchDice;
        PhaseChange();
    }

    void OnDestroy() {
        MyNetworkRoomManager.OnAllGamePlayersReady -= OnAllPlayersReady;
        MyNetworkPlayer.OnPlayerReady -= OnPlayerReady;

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

    [Server]
    void OnMovePhase()
    {
        Debug.Log("must move of " + diceResult);
        int newPos = currPlayer.GetTile() + diceResult;

        if (newPos > Tiles.Length - 1) {
            Tiles[0].GetComponent<Tile>().Action(currPlayer);
            newPos %= (Tiles.Length - 1);
        }
        currPlayer.SetTile(newPos); // visually move player

        currPhase = Phase.TileAction;
        PhaseChange();
    }
    
    [Server]
    void OnTileActionPhase()
    {
        // Tiles[currPlayer.GetTile()].GetComponent<Tile>().Action(currPlayer);

        currPhase = playAgain ? Phase.LaunchDice : Phase.NextTurn;
        PhaseChange();
    }

    [Server]
    void OnNextTurnPhase()
    {
        if (NetworkServer.connections.Count <= 1) {
            currPlayer.RpcPlayerWin(currPlayer.GetPlayerId(), "you are the last survivor !");
            return;
        }

        int nextPlayerId = currPlayer.GetPlayerId() + 1;
        if (nextPlayerId >= networkPlayers.Count)
            nextPlayerId = 0;
        currPlayer = networkPlayers[nextPlayerId];

        currPhase = Phase.LaunchDice;
        PhaseChange();
    }

    void OnAllPlayersReady()
    {
        Debug.Log("GameManager received event OnAllPlayersReady");
        OnAllPlayersReadyEventReceived?.Invoke();
        foreach (MyNetworkPlayer p in networkPlayers)
            currPlayer.RpcSetPlayersUi(networkPlayers.Count);
    }

    void OnPlayerReady(int playerId)
    {
        networkPlayers[playerId].RpcSetPlayersUi(networkPlayers.Count);
    }

    #endregion
    #region Client

    public Vector3 GetTilePosition(int tileId)
    {
        Debug.Log("GetTilePosition : " + tileId);
        Tiles[tileId].TryGetComponent<BuyableTile>(out BuyableTile buyableTile);
        if (buyableTile != null)
            return buyableTile.GetPlayerPosition();

        Tiles[tileId].TryGetComponent<NonBuyableTile>(out NonBuyableTile nonBuyableTile);
        if (nonBuyableTile != null)
            return nonBuyableTile.GetPlayerPosition();

        return new Vector3(0, 0, 0);
    }

    public void LaunchDice()
    {
        CmdLaunchDice();
    }

    #endregion

    //// public override void OnStartClient()
    //void Start()
    //{
    //    myNetworkRoomManagerObj = GameObject.Find("MyNetworkRoomManager");
    //    myNetworkRoomManagerScript = myNetworkRoomManagerObj.GetComponent<MyNetworkRoomManager>();

    //    // MyNetworkRoomManager.OnPlayerAdded += OnPlayerAdd;
    //    MyNetworkRoomManager.OnAllGamePlayersReady += HandleAllGamePlayersReady;


    //    UIPanel.OnPlayerBoughtUpgrade += TargetPlayerBoughtUpgrade;
    //}

    //void OnDestroy()
    //{
    //    MyNetworkRoomManager.OnAllGamePlayersReady += HandleAllGamePlayersReady;
    //    // MyNetworkRoomManager.OnPlayerAdded -= OnPlayerAdd;

    //    MyNetworkPlayer.OnMoneyChanged -= UpdateDisplayMoneyOfPlayer;

    //    UIPanel.OnPlayerBoughtUpgrade -= TargetPlayerBoughtUpgrade;
    //}

}
