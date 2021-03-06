using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Monopoly
{
    public List<BuyableTile> tiles;

    public bool IsMonopoly(bool checkMonopolyLine = false)
    {
        if ((checkMonopolyLine && this.tiles.Count == 0) || (!checkMonopolyLine && this.tiles.Count <= 1))
            return false;

        int ownerId = tiles[0].GetOwnerId();
        if (ownerId == 0)
            return false;

        foreach (BuyableTile tile in this.tiles)
            if (tile.GetOwnerId() != ownerId)
                return false;

        return true;
    }

    public int GetMonopolyOwnerId()
    {
        if (this.tiles.Count == 0)
            return 0;

        return this.tiles[0].GetOwnerId();
    }
}