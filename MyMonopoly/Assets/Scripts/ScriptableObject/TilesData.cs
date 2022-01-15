using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Tiles/TileData", order = 1)]
public class TilesData : ScriptableObject
{
    [SerializeField] string tileName;
    [SerializeField] int[] upgradePrice;
    [SerializeField] int[] sellPrice;
    [SerializeField] int[] rentPrice;
    [SerializeField] int[] buyPrice;
}
