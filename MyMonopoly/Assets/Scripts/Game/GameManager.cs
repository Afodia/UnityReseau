using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
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
        WaitRolling,
        Move,
        TileAction,
        NextTurn
    }

    Phase currPhase = Phase.WaitingStart;
    List<MyNetworkPlayer> networkPlayers = new List<MyNetworkPlayer>();
    MyNetworkPlayer currPlayer;
    int nbDouble = 0;
    bool playAgain = false;
    int diceResult = 0;

    #region Server

    [Server]
    public void StartGame(List<MyNetworkPlayer> players)
    {
        StartCoroutine(WaitingForPlayer(players));
    }

    [Server]
    IEnumerator WaitingForPlayer(List<MyNetworkPlayer> players)
    {
        int rand = Random.Range(0, players.Count - 1);
        for (int i = 0 ; i < players.Count ; i++) {
            networkPlayers.Add(players[i]);
            yield return new WaitUntil(() => networkPlayers[i].isReady == true);
            networkPlayers[i].SetClientId(players[i].GetClientId());
            networkPlayers[i].SetPlayerId(i + 1);
            networkPlayers[i].TargetSetPlayerId(i + 1);
        }
        currPlayer = networkPlayers[rand];
        SetPlayersUI();
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
            case Phase.WaitRolling:
                StartCoroutine(OnWaitRollingPhase());
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
        currPlayer.TargetShowDiceButton();
    }

    [Server]
    void RollDices()
    {
        playAgain = false;
        currPlayer.TargetHideDiceButton();
        currPlayer.RpcShowDices();

        if (currPlayer.isInJail() && currPlayer.GetNbTurnInJail() >= 3) {
            currPlayer.SetInJail(false);
            currPlayer.ResetNbTurnInJail();
        }

        int resDice1 = Random.Range(1, 6);
        int resDice2 = Random.Range(1, 6);
        diceResult = resDice1 + resDice2;

        currPlayer.RpcRollDices(resDice1, resDice2);
        currPlayer.ResetLaunchingDice();

        if (resDice1 == resDice2) {
            if (currPlayer.isInJail()) {
                currPlayer.SetInJail(false);
                currPlayer.ResetNbTurnInJail();
            }
            nbDouble += 1;
            playAgain = true;
        }

        if (currPlayer.isInJail()) {
            currPlayer.IncreaseNbTurnInJail();
        }

        currPhase = Phase.WaitRolling;
        PhaseChange();
    }

    [Server]
    IEnumerator OnWaitRollingPhase()
    {
        yield return new WaitUntil(() => !currPlayer.launchingDice);
        currPlayer.RpcHideDices();

        if (nbDouble >= 3 || currPlayer.isInJail()) {
            Tiles[24].GetComponent<Tile>().Action(currPlayer, 8);
            playAgain = false;
            currPhase = Phase.NextTurn;
        } else
            currPhase = Phase.Move;

        PhaseChange();
    }

    [Server]
    void OnMovePhase()
    {
        int newPos = currPlayer.GetTile() + diceResult;

        if (newPos > Tiles.Length - 1) {
            Tiles[0].GetComponent<Tile>().Action(currPlayer);
            ChangeMoneyDisplayed();
            newPos %= (Tiles.Length - 1);
        }
        currPlayer.SetTile(newPos);
        currPlayer.RpcSetPlayerAvatarPosition(this.GetTilePosition(newPos, currPlayer.GetPlayerId()));

        currPhase = Phase.TileAction;
        PhaseChange();
    }

    [Server]
    private void ChangeMoneyDisplayed()
    {
        foreach (MyNetworkPlayer p in networkPlayers) {
            p.UpdateDisplayMoneyOfPlayer(currPlayer.GetPlayerId(), currPlayer.GetMoney());
        }
        return;
    }
    
    [Server]
    void OnTileActionPhase()
    {
        Tiles[currPlayer.GetTile()].GetComponent<Tile>().Action(currPlayer, 8);

        currPhase = playAgain ? Phase.LaunchDice : Phase.NextTurn;
        PhaseChange();
    }

    [Server]
    void OnNextTurnPhase()
    {
        if (NetworkServer.connections.Count <= 1) {
           currPlayer.RpcPlayerWin(currPlayer.GetPlayerId(), "you are the last player connected !");
           return;
        }

        int nextPlayerId = currPlayer.GetPlayerId() + 1;
        if (nextPlayerId > networkPlayers.Count)
            nextPlayerId = 1;
        currPlayer = networkPlayers[nextPlayerId - 1];
        nbDouble = 0;
        currPhase = Phase.LaunchDice;
        PhaseChange();
    }

    [Server]
    void SetPlayersUI()
    {
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
            p.RpcSetPlayerAvatarPosition(this.GetTilePosition(0, p.GetPlayerId()));
            int playerId = p.GetPlayerId();
            p.RpcSetPlayerAvatarColor(playerId == 1 ? greenColor : playerId == 2 ? blueColor : playerId == 3 ? redColor : playerId == 4 ? purpleColor : Color.white);
        }
    }

    #endregion
    #region Client

    public Vector3 GetTilePosition(int tileId, int playerId)
    {
        Tiles[tileId].TryGetComponent<BuyableTile>(out BuyableTile buyableTile);
        if (buyableTile != null)
            return buyableTile.GetPlayerPosition(playerId);

        Tiles[tileId].TryGetComponent<NonBuyableTile>(out NonBuyableTile nonBuyableTile);
        if (nonBuyableTile != null)
            return nonBuyableTile.GetPlayerPosition(playerId);

        return new Vector3(0, 0, 0);
    }

    public void LaunchDice()
    {
        CmdLaunchDice();
    }

    #endregion

}