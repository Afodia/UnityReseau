using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonopoliesLine
{
    public List<Monopoly> monopolies;

    public bool IsLinearMonopoly()
    {
        if (this.monopolies.Count == 0)
            return false;

        int ownerId = this.monopolies[0].tiles[0].GetOwnerId();
        foreach (Monopoly monopoly in this.monopolies)
            if (!monopoly.IsMonopoly() || monopoly.GetMonopolyOwnerId() != ownerId)
                return false;

        return true;
    }

    public int GetMonopoliesLineOwnerId()
    {
        if (this.monopolies.Count == 0)
            return 0;

        return this.monopolies[0].GetMonopolyOwnerId();
    }
}
