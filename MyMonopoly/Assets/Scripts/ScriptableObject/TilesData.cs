using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Tiles/TileData", order = 1)]
public class TilesData : ScriptableObject
{
    public string tileName;
    public int[] upgradePrice;
    public int[] sellPrice;
    public int[] rentPrice;
    public int[] buyPrice;
}
