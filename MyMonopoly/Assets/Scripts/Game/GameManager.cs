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
    [SerializeField] List<MonopoliesLine> MonopoliesLines;
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
    public MyNetworkPlayer GetPlayer(int playerId)
    {
        foreach (MyNetworkPlayer player in networkPlayers)
            if (player.GetPlayerId() == playerId)
                return player;

        return null;
    }

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

        ClientTilesStartGameSetup();
        SetPlayersUI();

        currPhase = Phase.LaunchDice;
        PhaseChange();
    }

    [Server]
    void ClientTilesStartGameSetup()
    {
        foreach (GameObject tile in Tiles) {
            tile.GetComponent<Tile>().RpcSetId(tile.GetComponent<Tile>().GetId());
            if (tile.TryGetComponent<BuyableTile>(out BuyableTile buyableTile)) {
                buyableTile.RpcSetData(buyableTile.GetData());
            }
        }
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
        } else {
            if (currPlayer.isInJail())
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

        if (nbDouble >= 3) {
            Tiles[24].GetComponent<Tile>().Action(currPlayer, 8);
            playAgain = false;
            currPhase = Phase.NextTurn;
        } else if (currPlayer.isInJail())
            currPhase = Phase.NextTurn;
        else
            currPhase = Phase.Move;

        PhaseChange();
    }

    [Server]
    void OnMovePhase()
    {
        int newPos = currPlayer.GetTile() + diceResult;
        if (newPos >= Tiles.Length) {
            Tiles[0].GetComponent<Tile>().Action(currPlayer, 0);
            newPos %= (Tiles.Length - 1);
            newPos -= 1;
        }
        ChangeMoneyDisplayed();
        currPlayer.SetTile(newPos);
        currPlayer.RpcSetPlayerAvatarPosition(this.GetTilePosition(newPos, currPlayer.GetPlayerId()));

        currPhase = Phase.TileAction;
        PhaseChange();
    }

    [Server]
    void OnTileActionPhase()
    {
        Tiles[currPlayer.GetTile()].GetComponent<Tile>().Action(currPlayer, 8);
    }

    [Server]
    void DisplayVictoryReason(int id, string reason)
    {
        foreach (MyNetworkPlayer player in networkPlayers)
            player.TargetPlayerWin(id, reason);
    }

    [Server]
    public void TileActionEnded()
    {
        ChangeMoneyDisplayed();
        CheckAndUpdateBeachesTiles();
        CheckAndUpdateMonopoliesStates();

        if (NetworkServer.connections.Count <= 1) {
          DisplayVictoryReason(currPlayer.GetPlayerId(), "are the last player connected !");
          return;
        }

        if (MonopoliesLines[0].monopolies[0].IsMonopoly() && MonopoliesLines[0].monopolies[0].GetMonopolyOwnerId() == currPlayer.GetPlayerId()) {
            DisplayVictoryReason(currPlayer.GetPlayerId(), "own all the beaches !");
            return;
        } else if (GetPlayerNbMonopolies(currPlayer.GetPlayerId()) >= 3) {
            DisplayVictoryReason(currPlayer.GetPlayerId(), "have 3 monopolies !");
            return;
        } else if (PlayerHasMonopolyLine(currPlayer.GetPlayerId())) {
            DisplayVictoryReason(currPlayer.GetPlayerId(), "have a monopolies line !");
            return;
        }

        if (currPlayer.isInJail())
            currPhase = Phase.NextTurn;
        else
            currPhase = playAgain ? Phase.LaunchDice : Phase.NextTurn;
        PhaseChange();
    }

    [Server]
    void OnNextTurnPhase()
    {
        MyNetworkPlayer p = currPlayer;
        bool isCurrentPlayer = false;
        foreach (MyNetworkPlayer player in networkPlayers) {
            if (isCurrentPlayer) {
                currPlayer = player;
                break;
            }
            if (player.GetPlayerId() == currPlayer.GetPlayerId()) {
                isCurrentPlayer = true;
            }
        }
        if (p.GetPlayerId() == currPlayer.GetPlayerId())
            currPlayer = networkPlayers[0];

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
        return;
    }

    [Server]
    private void CheckUpgrade(int upgradeLvl)
    {
        if (Tiles[currPlayer.GetTile()].TryGetComponent<BuyableTile>(out BuyableTile tile)) {
            float money = tile.GetUpgrade(upgradeLvl);
            if (currPlayer.GetMoney() >= money) {
                currPlayer.ChangeMoney(-money);
                tile.UpdateTile(currPlayer.GetPlayerId(), upgradeLvl);
            }
        }

        ChangeMoneyDisplayed();
        TileActionEnded();
    }

    [Server]
    private void CardTaken(CardsData card)
    {
        if (card.type == CardsData.Type.Add) {
            currPlayer.ChangeMoney(card.value);
            ChangeMoneyDisplayed();
            TileActionEnded();
        } else if (card.type == CardsData.Type.Remove) {
            currPlayer.ChangeMoney(-card.value);
            ChangeMoneyDisplayed();
            TileActionEnded();
        } else {
            if (currPlayer.GetTile() > card.value && card.value != 24) {
                currPlayer.ChangeMoney(300000);
                ChangeMoneyDisplayed();
            }
            currPlayer.SetTile((int)card.value);
            currPlayer.RpcSetPlayerAvatarPosition(this.GetTilePosition((int)card.value, currPlayer.GetPlayerId()));
            Tiles[currPlayer.GetTile()].GetComponent<Tile>().Action(currPlayer, 8);
        }
    }

    [Server]
    public List<BuyableTile> GetPlayerOwnedTiles(int playerId)
    {
        List<BuyableTile> ownedTiles = new List<BuyableTile>();

        for (int i = 0; i < Tiles.Length; i++)
            if (Tiles[i].TryGetComponent<BuyableTile>(out BuyableTile tile) && tile.GetOwnerId() == playerId)
                ownedTiles.Add(tile);

        return ownedTiles;
    }

    [Server]
    public float GetTotalSellValueOfPlayerOwnedTiles(int playerId)
    {
        float totalSellValue = 0;

        for (int i = 0; i < Tiles.Length; i++)
            if (Tiles[i].TryGetComponent<BuyableTile>(out BuyableTile tile) && tile.GetOwnerId() == playerId)
                totalSellValue += tile.GetSellPrice();

        return totalSellValue;
    }

    [Server]
    public void SellAllOwnedTilesOfPlayer(MyNetworkPlayer player)
    {
        List<BuyableTile> ownedTiles = GetPlayerOwnedTiles(player.GetPlayerId());

        foreach (BuyableTile tile in ownedTiles)
            tile.SellTile(player);

        player.SetMoney(0f);
        ChangeMoneyDisplayed();
    }

    [Server]
    public void OnPlayerLose()
    {
        currPlayer.RpcDisablePlayerAvatar();
        networkPlayers.Remove(currPlayer);
        playAgain = false;
        // TileActionEnded();
    }

    [Server]
    public void OnPlayerDisconnected(NetworkConnection conn)
    {
        MyNetworkPlayer disconnectedPlayer = null;

        foreach (MyNetworkPlayer p in networkPlayers)
            if (p.GetConn() == conn) {
                disconnectedPlayer = p;
                break;
            }

        SellAllOwnedTilesOfPlayer(disconnectedPlayer);
        networkPlayers.Remove(disconnectedPlayer);

        if (NetworkServer.connections.Count == 1 && networkPlayers.Count == 1)
            networkPlayers[0].TargetPlayerWin(networkPlayers[0].GetPlayerId(), "are the last player connected !");
    }

    [Server]
    public void CheckAndUpdateMonopoliesStates()
    {
        for (int i = 1; i < MonopoliesLines.Count; i++)
            foreach (Monopoly monopoly in MonopoliesLines[i].monopolies)
                SetTilesMonopolyState(monopoly, monopoly.IsMonopoly());
    }

    [Server]
    void SetTilesMonopolyState(Monopoly monopoly, bool state)
    {
        foreach (BuyableTile tile in monopoly.tiles)
            if (tile.IsMonopoly() != state)
                tile.SetMonopoly(state);
    }

    [Server]
    int GetPlayerNbMonopolies(int playerId)
    {
        int nbMonopolies = 0;

        for (int i = 1; i < MonopoliesLines.Count; i++)
            foreach (Monopoly monopoly in MonopoliesLines[i].monopolies)
                if (monopoly.IsMonopoly() && monopoly.GetMonopolyOwnerId() == playerId)
                    nbMonopolies++;

        return nbMonopolies;
    }

    [Server]
    bool PlayerHasMonopolyLine(int playerId)
    {
        for (int i = 1; i < MonopoliesLines.Count; i++)
            if (MonopoliesLines[i].IsLinearMonopoly() && MonopoliesLines[i].GetMonopoliesLineOwnerId() == playerId)
                return true;

        return false;
    }

    [Server]
    void CheckAndUpdateBeachesTiles()
    {
        foreach (MyNetworkPlayer player in networkPlayers) {
            List<int> playerOwnedBeachesIds = GetBeachTilesIdsOfPlayer(player.GetPlayerId());

            foreach (int beachId in playerOwnedBeachesIds)
                Tiles[beachId].GetComponent<BuyableTile>().UpdateTile(player.GetPlayerId(), playerOwnedBeachesIds.Count - 1);
        }
    }

    [Server]
    List<int> GetBeachTilesIdsOfPlayer(int playerId)
    {
        List<int> beachTilesIds = new List<int>();

        foreach (BuyableTile tile in MonopoliesLines[0].monopolies[0].tiles)
            if (tile.GetOwnerId() == playerId)
                beachTilesIds.Add(tile.GetId());

        return beachTilesIds;
    }

    #endregion



    #region Client

    [Client]
    public void LaunchDice()
    {
        CmdLaunchDice();
    }

    [Command(requiresAuthority = false)]
    void CmdLaunchDice()
    {
        RollDices();
    }

    [Command(requiresAuthority = false)]
    public void CmdUpgradeBuilding(int upgradeLvl)
    {
        CheckUpgrade(upgradeLvl);
    }

    [Command(requiresAuthority = false)]
    public void CmdCardTaken(CardsData card)
    {
        CardTaken(card);
    }

    [Command(requiresAuthority = false)]
    public void CmdSellTiles(int[] tilesIDsToSell)
    {
        foreach (int tileID in tilesIDsToSell) {
            if (Tiles[tileID].TryGetComponent<BuyableTile>(out BuyableTile tile)) {
                currPlayer.ChangeMoney(tile.GetSellPrice());
                tile.SellTile(currPlayer);
            }
        }

        currPlayer.mustSell = false;
        ChangeMoneyDisplayed();
        TileActionEnded();
    }

    #endregion



    #region Utils

    [Server]
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

    [Client]
    public string ChangePriceToText(float price)
    {
        string toReturn;

        if (price < 1000000)
            toReturn = (price / 1000).ToString() + "K";
        else
            toReturn = (price / 1000000).ToString() + "M";
        return toReturn;
    }

    [Server]
    public void ChangeMoneyDisplayed()
    {
        foreach (MyNetworkPlayer p in networkPlayers)
            p.TargetUpdateDisplayMoneyOfPlayer(currPlayer.GetPlayerId(), currPlayer.GetMoney());
    }

    [Server]
    public void ChangeMoneyDisplayedOfPlayer(MyNetworkPlayer player)
    {
        foreach (MyNetworkPlayer p in networkPlayers)
            p.TargetUpdateDisplayMoneyOfPlayer(player.GetPlayerId(), player.GetMoney());
    }
    #endregion
}
