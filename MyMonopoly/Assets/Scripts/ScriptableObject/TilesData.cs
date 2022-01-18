using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Tiles/TileData", order = 1)]
public class TilesData : ScriptableObject
{
    public string tileName;
    public float[] upgradePrice = new float[5];
    public float[] sellPrice = new float[5];
    public float[] rentPrice = new float[5];
    public float[] buyPrice = new float[5];
}
