using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Monopoly
{
    public List<BuyableTile> tiles;

    public bool IsMonopoly()
    {
        if (this.tiles.Count == 0)
            return false;

        int ownerId = tiles[0].GetOwnerId();
        if (ownerId == 0)
            return false;

        foreach (BuyableTile tile in this.tiles)
            if (tile.GetOwnerId() != ownerId) {
                Debug.Log("Isn't a monopoly");
                return false;
            }

        Debug.Log("Is a monopoly");
        return true;
    }

    public int GetMonopolyOwnerId()
    {
        if (this.tiles.Count == 0)
            return 0;

        return this.tiles[0].GetOwnerId();
    }
}