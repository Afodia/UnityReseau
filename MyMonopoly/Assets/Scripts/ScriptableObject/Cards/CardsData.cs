using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Card/CardData", order = 1)]
public class CardsData : ScriptableObject
{
    public enum Type
    {
        Add,
        Remove,
        Move
    }

    public Type type;
    public string description;
    public float value;

}
