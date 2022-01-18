using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPanel : MonoBehaviour
{
    public static UIPanel instance;
    private UIPanel() { }
    [SerializeField] TMP_Text upgradeCity;
    [SerializeField] TMP_Text upgradeRent;
    [SerializeField] TMP_Text upgradeButton;
    [SerializeField] GameObject upgradePanel;
    [SerializeField] GameObject[] upgradePanels;
    
    int upgradeLevel = 0;
    TilesData currData;
    int currLvl;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);

        instance = this;
    }

    public void ChangeUpgradeLvl(int newLevel)
    {
        upgradeLevel = newLevel;
        UpgradeUpdate();
    }

    public string ChangePriceToText(float price)
    {
        string toReturn = "";
        if (price < 1000000)
            toReturn = (price / 1000).ToString()+ "K";
        else
            toReturn = (price / 1000000).ToString() + "M";
        Debug.Log(toReturn);
        return toReturn;
    }

    public void ShowPanel(TilesData data, Sprite[] houses, int lvl)
    {
        upgradePanel.SetActive(true);
        for (int i = 0 ; i < upgradePanels.Length ; i++) {
            upgradePanels[i].GetComponentsInChildren<RawImage>()[0].texture = houses[i].texture;
            upgradePanels[i].GetComponentInChildren<TMP_Text>().text = ChangePriceToText(data.upgradePrice[i]);
            if (i < lvl)
                upgradePanels[i].GetComponent<Button>().enabled = false;
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
        upgradeRent.text = "Rent rate: " + currData.rentPrice[upgradeLevel] + "K";
        upgradeButton.text = "Buy For " + ChangePriceToText(toBuy);
    }
}
