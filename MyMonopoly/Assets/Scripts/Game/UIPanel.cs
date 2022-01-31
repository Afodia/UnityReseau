using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;

public class UIPanel : NetworkBehaviour
{
    [SerializeField] Sprite[] houses = new Sprite[16];
    [SerializeField] TMP_Text upgradeCity;
    [SerializeField] TMP_Text upgradeRent;
    [SerializeField] TMP_Text upgradeButton;
    [SerializeField] GameObject upgradePanel;
    [SerializeField] GameObject[] upgradePanels;

    int upgradeLevel = 0;
    TilesData currData;
    int currLvl;

    #region UpgradePanel

    [Client]
    public void ChangeUpgradeLvl(int newLevel)
    {
        upgradeLevel = newLevel;
        UpgradeUpdate();
    }

    [Client]
    public void ShowPanel(TilesData data, int[] housesId, int lvl, float money)
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
}
