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

        if (currPlayer.isInJail() && currPlayer.GetNbTurnInJail() >= 3) {
            currPlayer.SetInJail(false);
            currPlayer.ResetNbTurnInJail();
        }

        int resDice1 = Random.Range(1, 6);
        int resDice2 = Random.Range(1, 6);

        currPlayer.RpcRollDices(resDice1, resDice2);

        diceResult = resDice1 + resDice2;
        playAgain = false;
        if (resDice1 == resDice2) {
            if (currPlayer.isInJail()) {
                currPlayer.SetInJail(false);
                currPlayer.ResetNbTurnInJail();
            }
            nbDouble += 1;
            playAgain = true;
        }
        if (nbDouble >= 3) {
            Tiles[24].GetComponent<Tile>().Action(currPlayer);
            playAgain = false;
            currPhase = Phase.NextTurn;
            PhaseChange();
        }

        if (currPlayer.isInJail())
            currPlayer.IncreaseNbTurnInJail();

        currPhase = Phase.Move;
        PhaseChange();
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
        currPlayer.SetTile(newPos);
        currPlayer.RpcSetPlayerAvatarPosition(this.GetTilePosition(newPos));

        currPhase = Phase.TileAction;
        PhaseChange();
    }
    
    [Server]
    void OnTileActionPhase()
    {
        // Tiles[currPlayer.GetTile()].GetComponent<Tile>().Action(currPlayer);

        Debug.Log($"1 playAgain : {playAgain}, Phase : {currPhase}");
        currPhase = playAgain ? Phase.LaunchDice : Phase.NextTurn;
        Debug.Log($"2 playAgain : {playAgain}, Phase : {currPhase}");
        PhaseChange();
    }

    [Server]
    void OnNextTurnPhase()
    {
        if (NetworkServer.connections.Count <= 1) {
            currPlayer.RpcPlayerWin(currPlayer.GetPlayerId(), "you are the last survivor !");
            return;
        }

        Debug.Log("current player turn : " + currPlayer.GetPlayerId());
        int nextPlayerId = currPlayer.GetPlayerId() + 1;
        if (nextPlayerId > networkPlayers.Count)
            nextPlayerId = 1;
        currPlayer = networkPlayers[nextPlayerId - 1];
        Debug.Log("next player turn : " + currPlayer.GetPlayerId());

        currPhase = Phase.LaunchDice;
        PhaseChange();
    }

    void OnAllPlayersReady()
    {
        Debug.Log("GameManager received event OnAllPlayersReady");
        OnAllPlayersReadyEventReceived?.Invoke();

        Color greenColor;
        ColorUtility.TryParseHtmlString("#1AB600", out greenColor);
        Color blueColor;
        ColorUtility.TryParseHtmlString("#3600FF", out blueColor);
        Color redColor;
        ColorUtility.TryParseHtmlString("#FF0000", out redColor);
        Color purpleColor;
        ColorUtility.TryParseHtmlString("#A100D0", out purpleColor);

        foreach (MyNetworkPlayer p in networkPlayers) {
            p.TargetSetPlayersUi(networkPlayers.Count);
            p.RpcSetPlayerAvatarPosition(this.GetTilePosition(0));
            int playerId = p.GetPlayerId();
            p.RpcSetPlayerAvatarColor(playerId == 1 ? greenColor : playerId == 2 ? blueColor : playerId == 3 ? redColor : playerId == 4 ? purpleColor : Color.white);
        }
    }

    void OnPlayerReady(int playerId)
    {
        networkPlayers[playerId].TargetSetPlayersUi(networkPlayers.Count);
    }

    #endregion
    #region Client

    public Vector3 GetTilePosition(int tileId)
    {
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

}
