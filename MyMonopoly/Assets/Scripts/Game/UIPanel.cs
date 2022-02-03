using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UIPanel : NetworkBehaviour
{
    #region LuckCard

    [Header("Luck card panel")]

    [SerializeField] GameObject luckCardPanel;
    [SerializeField] TMP_Text description;
    CardsData currCard;

    [Client]
    public void ShowLuckCard(CardsData card)
    {
        currCard = card;
        luckCardPanel.SetActive(true);
        description.text = card.description;
    }

    [Client]
    public void TakeCard()
    {
        luckCardPanel.SetActive(false);
        GameManager.instance.CmdCardTaken(currCard);
    }

    #endregion

    #region UpgradePanel

    [Header("Upgrade panel")]
    [SerializeField] Sprite[] houses = new Sprite[16];
    [SerializeField] TMP_Text upgradeCity;
    [SerializeField] TMP_Text upgradeRent;
    [SerializeField] Button upgradeBtn;
    [SerializeField] TMP_Text upgradeButton;
    [SerializeField] GameObject upgradePanel;
    [SerializeField] GameObject[] upgradePanels;


    int upgradeLevel = -1;
    TilesData currData;
    int currLvl;

    [Client]
    public void ChangeUpgradeLvl(int newLevel)
    {
        upgradeLevel = newLevel;
        UpgradeUpdate();
    }

    [Client]
    public void ShowUpgradePanel(TilesData data, int[] housesId, int lvl, float money)
    {
        float toBuy = 0;
        float alreadyBought = 0;
        for (int i = 0 ; i <= lvl ; i++)
            alreadyBought += currData.upgradePrice[i];
        Debug.Log($"Tile lvl : {lvl}, player money : {money}");
        upgradeLevel = lvl;
        upgradePanel.SetActive(true);
        for (int i = 0 ; i < upgradePanels.Length ; i++) {
            upgradePanels[i].GetComponentsInChildren<Image>()[1].sprite = houses[housesId[i]];
            upgradePanels[i].GetComponentInChildren<TMP_Text>().text = GameManager.instance.ChangePriceToText(data.upgradePrice[i]);
            toBuy += data.upgradePrice[i];
            Debug.Log($"toBuy {toBuy} for i {i}");
            Debug.Log($"already bought {alreadyBought} for i {i}");
            if (i <= lvl || (toBuy - alreadyBought) > money) {
                upgradePanels[i].GetComponent<Button>().enabled = false;
                if (lvl != -1 && i <= lvl)
                    upgradePanels[i].transform.Find("Selection").gameObject.SetActive(true); 
            }
        }
        currLvl = lvl;
        currData = data;
        upgradeCity.text = data.tileName;
        UpgradeUpdate();
    }

    [Client]
    void UpgradeUpdate()
    {
        Debug.Log("upgrade panel, new lvl :" + upgradeLevel);
        float toBuy = 0;
        for (int i = currLvl + 1 ; i <= upgradeLevel ; i++)
            toBuy += currData.upgradePrice[i];
        if (upgradeLevel != -1)
            upgradeRent.text = "Rent rate: " + GameManager.instance.ChangePriceToText(currData.rentPrice[upgradeLevel]);
        if (currLvl == upgradeLevel) {
            upgradeButton.text = "No upgrade selected";
            upgradeBtn.enabled = false;
        } else
            upgradeButton.text = "Buy For " + GameManager.instance.ChangePriceToText(toBuy);
    }

    [Client]
    private void ResetBuyUi()
    {
        upgradePanel.SetActive(false);
        foreach (GameObject up in upgradePanels) {
            up.transform.Find("Selection").gameObject.SetActive(false);
        }
    }

    [Client]
    public void ValidateUpgrade()
    {
        ResetBuyUi();
        GameManager.instance.CmdUpgradeBuilding(upgradeLevel);
    }

    [Command(requiresAuthority = false)]
    public void CancelBuy()
    {
        ResetBuyUi();
        GameManager.instance.TileActionEnded();
    }
    #endregion



    #region SellPanel

    [Header("Sell panel")]
    [SerializeField] GameObject sellPanel;
    [SerializeField] TMP_Text mustSellForValueText;
    [SerializeField] TMP_Text sellValueText;
    [SerializeField] Button confirmSellButton;
    [SerializeField] SellButton[] citiesButtons;
    float minimumMoneyRequired;


    [TargetRpc]
    public void TargetShowSellPanel(List<BuyableTile> playerOwnedCities, float minimumMoneyRequired)
    {
        sellPanel.SetActive(true);
        this.minimumMoneyRequired = minimumMoneyRequired;
        mustSellForValueText.text = $"Must sell for $ {GameManager.instance.ChangePriceToText(minimumMoneyRequired)}";

        for (int i = 0; i < playerOwnedCities.Count; i++)
            citiesButtons[i].SetButtonsInformations(this, playerOwnedCities[i].ClientGetId(), playerOwnedCities[i].ClientGetTileName(), playerOwnedCities[i].ClientGetSellPrice());
    }

    [Client]
    public void TargetHideSellPanel()
    {
        sellPanel.SetActive(false);

        confirmSellButton.interactable = false;

        sellValueText.text = "";
        sellValueText.color = Color.red;

        foreach (SellButton cityButton in citiesButtons)
            cityButton.ResetUi();
    }

    [Client]
    public void UpdateSellValueText()
    {
        sellValueText.text = $"$ {GameManager.instance.ChangePriceToText(GetTotalSellValue())}";
        if (GetTotalSellValue() >= minimumMoneyRequired) {
            sellValueText.color = Color.green;
            confirmSellButton.interactable = true;
        } else
            sellValueText.color = Color.red;

    }

    float GetTotalSellValue()
    {
        float totalSellValue = 0f;

        foreach (SellButton cityButton in citiesButtons)
            if (cityButton.isSelected())
                totalSellValue += cityButton.GetSellPrice();

        return totalSellValue;
    }

    [Client]
    public void Sell()
    {
        List<int> citiesIdsToSell = new List<int>();

        foreach (SellButton cityButton in citiesButtons)
            if (cityButton.isSelected())
                citiesIdsToSell.Add(cityButton.GetTileId());

        if (GetTotalSellValue() >= this.minimumMoneyRequired)
            GameManager.instance.CmdSellTiles(citiesIdsToSell.ToArray());
    }

    #endregion
}
