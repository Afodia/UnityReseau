using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellButton : MonoBehaviour
{
    int tileId = -1;
    float sellPrice = 0f;
    [SerializeField] TMP_Text cityNameAndSellValueText;
    bool selected = false;
    [SerializeField] Color notSelectedColor;
    [SerializeField] Color selectedColor;
    UIPanel uiPanelRef;

    public void SetButtonsInformations(UIPanel uiPanel, int tileId, string cityName, float sellPrice)
    {
        this.uiPanelRef = uiPanel;
        this.tileId = tileId;
        this.sellPrice = sellPrice;
        this.gameObject.SetActive(true);
        SetCityNameAndSellValueText(cityName, sellPrice);
    }

    public void SetCityNameAndSellValueText(string cityName, float value)
    {
        cityNameAndSellValueText.text = $"{cityName} : $ {GameManager.instance.ChangePriceToText(value)}";
    }

    public int GetTileId()
    {
        return tileId;
    }

    public float GetSellPrice()
    {
        return sellPrice;
    }

    public void Select()
    {
        selected = !selected;
        GetComponent<Image>().color = this.selected ? selectedColor : notSelectedColor;
        this.uiPanelRef.UpdateSellValueText();
    }

    public bool isSelected()
    {
        return this.selected;
    }

    public void ResetUi()
    {
        this.gameObject.SetActive(false);
        this.tileId = -1;
        this.sellPrice = 0f;
        this.cityNameAndSellValueText.text = "";
        this.selected = false;
        GetComponent<Image>().color = notSelectedColor;
    }
}
