using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIPanel : MonoBehaviour
{
    public static UIPanel instance;
    private UIPanel() { }
    [SerializeField] TMP_Text upgradeCity;
    [SerializeField] TMP_Text upgradeRent;
    [SerializeField] TMP_Text upgradeButton;
    [SerializeField] GameObject upgradePanel;
    [SerializeField] GameObject[] upgradePanels;

    public static event Action<int, int> OnPlayerBoughtUpgrade;

    int upgradeLevel = 0;
    TilesData currData;
    int currLvl;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);

        instance = this;
    }
    public string ChangePriceToText(float price)
    { // 1000 -> 1K : 100000 -> 100K : 1000000 -> 1M
        string toReturn;

        if (price < 1000000)
            toReturn = (price / 1000).ToString()+ "K";
        else
            toReturn = (price / 1000000).ToString() + "M";
        return toReturn;
    }

    #region UpgradePanel
    public void ChangeUpgradeLvl(int newLevel)
    {
        upgradeLevel = newLevel;
        UpgradeUpdate();
    }


    public void ShowPanel(TilesData data, Sprite[] houses, int lvl, float money)
    {
        float toBuy = 0;

        upgradePanel.SetActive(true);
        for (int i = 0 ; i < upgradePanels.Length ; i++) {
            upgradePanels[i].GetComponentsInChildren<Image>()[1].sprite = houses[i];
            upgradePanels[i].GetComponentInChildren<TMP_Text>().text = ChangePriceToText(data.upgradePrice[i]);
            toBuy += data.upgradePrice[i];
            if (i < lvl || toBuy > money) {
                upgradePanels[i].GetComponent<Button>().enabled = false;
            }
        }
        currLvl = lvl;
        currData = data;
        upgradeCity.text = data.tileName;
    }

    void UpgradeUpdate()
    {
        float toBuy = 0;
        for (int i = currLvl ; i <= upgradeLevel ; i++)
            toBuy += currData.upgradePrice[i];
        upgradeRent.text = "Rent rate: " + ChangePriceToText(currData.rentPrice[upgradeLevel]);
        upgradeButton.text = "Buy For " + ChangePriceToText(toBuy);
    }

    public void ValidateUpgrade()
    {

        //OnPlayerBoughtUpgrade?.Invoke(currPlayerId, upgradeLevel);
    }
    #endregion
}
