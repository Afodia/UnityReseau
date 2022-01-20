using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    GameObject myNetworkRoomManager = null;
    [SerializeField] GameObject[] playersUI = new GameObject[4];
    List<MyNetworkPlayer> networkPlayers = new List<MyNetworkPlayer>();
    public static event Action<int> OnPlayerTurnEnds;
    [SerializeField] GameObject dicesContainer = null;
    [SerializeField] RollTheDice diceRoller1 = null;
    [SerializeField] RollTheDice diceRoller2 = null;


    [Header("Menus")]
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject WinLoseMenu;
    [SerializeField] TMP_Text WinLoseText;


    void Start()
    {
        myNetworkRoomManager = GameObject.Find("MyNetworkRoomManager");
        MyNetworkRoomManager.OnPlayerWin += PlayerWin;
        MyNetworkRoomManager.OnPlayerLose += PlayerLose;
        MyNetworkRoomManager.OnPlayerNewTurnStarts += PlayerNewTurnStarts;

        OnPlayerAdd();
        MyNetworkPlayer.OnPlayerAdded += OnPlayerAdd;
    }

    void OnDestroy()
    {
        MyNetworkRoomManager.OnPlayerWin += PlayerWin;
        MyNetworkRoomManager.OnPlayerLose += PlayerLose;

        MyNetworkRoomManager.OnPlayerNewTurnStarts -= PlayerNewTurnStarts;
        MyNetworkPlayer.OnMoneyChanged -= UpdateDisplayMoneyOfPlayer;
        MyNetworkPlayer.OnPlayerAdded -= OnPlayerAdd;
    }

    void OnPlayerAdd()
    {
        GameObject[] players = SortPlayerReferencesById(GameObject.FindGameObjectsWithTag("Player"));

        for (int i = 0; i < players.Length; i++) {
            networkPlayers.Add(players[i].GetComponent<MyNetworkPlayer>());
            playersUI[i].SetActive(true);
        }

        MyNetworkPlayer.OnMoneyChanged += UpdateDisplayMoneyOfPlayer;
    }

    GameObject[] SortPlayerReferencesById(GameObject[] players)
    {
        for (int i = 0; i < players.Length; i++) {
            for (int j = i + 1; j < players.Length; j++) {
                if (players[i].GetComponent<MyNetworkPlayer>().GetId() > players[j].GetComponent<MyNetworkPlayer>().GetId()) {
                    GameObject tmp = players[i];
                    players[i] = players[j];
                    players[j] = tmp;
                }
            }
        }
        return players;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseMenu.SetActive(!PauseMenu.activeSelf);
    }

    #region Game loop
    void PlayerNewTurnStarts(int id)
    {
        if (myNetworkRoomManager.GetComponent<MyNetworkRoomManager>().clientIndex != id)
            return;

        dicesContainer.SetActive(true);
    }

    void EndTurn()
    {
        OnPlayerTurnEnds?.Invoke(myNetworkRoomManager.GetComponent<MyNetworkRoomManager>().clientIndex);
    }

    public void RollDices()
    {
        StartCoroutine(IERollDices());
    }

    public IEnumerator IERollDices()
    {
        yield return diceRoller1.Roll();
        yield return diceRoller2.Roll();
        yield return new WaitForSeconds(2.5f);
        int resDice1 = diceRoller1.GetLastRollResult();
        int resDice2 = diceRoller2.GetLastRollResult();
        Debug.Log($"Dice 1 : {resDice1}, Dice 2 : {resDice2}");
        dicesContainer.SetActive(false);
        EndTurn();
    }

    void UpdateDisplayMoneyOfPlayer(int id, float money)
    {
        TMP_Text playerMoneyText = playersUI[id].transform.Find("PlayerMoney").GetComponent<TMP_Text>();
        playerMoneyText.text = $"$ {SplitNumberString3By3(money.ToString())}";
    }

    string SplitNumberString3By3(string numberString)
    {
        string result = "";
        int i = numberString.Length - 1;

        while (i >= 0) {
            result = numberString[i] + result;
            i -= 3;
            if (i >= 0)
                result = " " + result;
        }
        return result;
    }
    #endregion

    #region Player Win/Lose
    void PlayerWin(int playerId, string reason)
    {
        if (playerId != myNetworkRoomManager.GetComponent<MyNetworkRoomManager>().clientIndex)
            return;

        WinLoseMenu.SetActive(true);
        WinLoseText.text = $"You won because {reason}";
    }

    void PlayerLose(int playerId, string reason)
    {
        if (playerId != myNetworkRoomManager.GetComponent<MyNetworkRoomManager>().clientIndex)
            return;

        WinLoseMenu.SetActive(true);
        WinLoseText.text = $"You lose because {reason}";
    }
    #endregion
}
