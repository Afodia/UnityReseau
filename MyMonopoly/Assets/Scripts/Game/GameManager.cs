using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject[] playersUI = new GameObject[4];
    List<MyNetworkPlayer> networkPlayers = new List<MyNetworkPlayer>();

    void Start()
    {
        OnPlayerAdd();
        MyNetworkPlayer.OnPlayerAdded += OnPlayerAdd;

    }

    void OnServerDisconnected()
    {
        Debug.Log("Server disconnected");
        PauseMenu.SetActive(true);
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

    void OnDestroy()
    {
        MyNetworkPlayer.OnMoneyChanged -= UpdateDisplayMoneyOfPlayer;
        MyNetworkPlayer.OnPlayerAdded -= OnPlayerAdd;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu.SetActive(!PauseMenu.activeSelf);
        }
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
}
