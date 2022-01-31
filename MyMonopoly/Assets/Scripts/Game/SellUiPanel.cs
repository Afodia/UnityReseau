using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class SellUiPanel : NetworkBehaviour
{
    float minimumMoneyRequired;
    [SerializeField] TMP_Text sellValueText;
    [SerializeField] SellButton[] citiesButtons;

    void OnEnable()
    {
        SellButton.OnCityButtonClicked += UpdateSellValueText;
    }

    void OnDisable()
    {
        SellButton.OnCityButtonClicked -= UpdateSellValueText;
    }

    [TargetRpc]
    public void TargetShowPanel(List<BuyableTile> playerOwnedCities, float minimumMoneyRequired)
    {
        this.gameObject.SetActive(true);
        this.minimumMoneyRequired = minimumMoneyRequired;

        for (int i = 0; i < playerOwnedCities.Count; i++)
            citiesButtons[i].SetButtonsInformations(playerOwnedCities[i].GetId(), playerOwnedCities[i].GetTileName(), playerOwnedCities[i].GetSellPrice());
    }

    [Client]
    public void HidePanel()
    {
        this.gameObject.SetActive(false);

        foreach (SellButton cityButton in citiesButtons)
            cityButton.ResetUi();
    }

    [Client]
    public void UpdateSellValueText()
    {
        sellValueText.text = $"$ {GameManager.instance.ChangePriceToText(GetTotalSellValue())}";
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
            GameManager.instance.CmdSellTile(citiesIdsToSell.ToArray());
    }

}
