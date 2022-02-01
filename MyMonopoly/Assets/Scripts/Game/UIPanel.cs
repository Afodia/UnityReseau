using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UIPanel : NetworkBehaviour
{
    #region UpgradePanel

    [Header("Upgrade panel")]
    [SerializeField] Sprite[] houses = new Sprite[16];
    [SerializeField] TMP_Text upgradeCity;
    [SerializeField] TMP_Text upgradeRent;
    [SerializeField] TMP_Text upgradeButton;
    [SerializeField] GameObject upgradePanel;
    [SerializeField] GameObject[] upgradePanels;


    int upgradeLevel = 0;
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
        Debug.Log("show panel");

        float toBuy = 0;
        upgradeLevel = lvl;
        upgradePanel.SetActive(true);
        for (int i = 0 ; i < upgradePanels.Length ; i++) {
            upgradePanels[i].GetComponentsInChildren<Image>()[1].sprite = houses[housesId[i]];
            upgradePanels[i].GetComponentInChildren<TMP_Text>().text = GameManager.instance.ChangePriceToText(data.upgradePrice[i]);
            toBuy += data.upgradePrice[i];
            if (i < lvl || toBuy > money) {
                upgradePanels[i].transform.Find("Selection").gameObject.SetActive(true);
                upgradePanels[i].GetComponent<Button>().enabled = false;
            }
        }
        currLvl = lvl;
        currData = data;
        upgradeCity.text = data.tileName;
    }

    [Client]
    void UpgradeUpdate()
    {
        float toBuy = 0;
        for (int i = currLvl ; i <= upgradeLevel ; i++)
            toBuy += currData.upgradePrice[i];
        upgradeRent.text = "Rent rate: " + GameManager.instance.ChangePriceToText(currData.rentPrice[upgradeLevel]);
        if (toBuy == 0)
            upgradeButton.text = "No upgrade selected";
        else
            upgradeButton.text = "Buy For " + GameManager.instance.ChangePriceToText(toBuy);
    }

    private void ResetUi()
    {
        foreach (GameObject up in upgradePanels) {
            up.transform.Find("Selection").gameObject.SetActive(false);
        }
    }

    [Client]
    public void ValidateUpgrade()
    {
        ResetUi();
        GameManager.instance.CmdUpgradeBuilding(upgradeLevel);
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

    [TargetRpc]
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
