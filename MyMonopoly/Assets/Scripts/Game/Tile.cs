using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public abstract class Tile : MonoBehaviour
{
    [SerializeField] protected int _id;
    [SerializeField] protected Type type;
    [SerializeField] private Sprite overlay = null;
    public enum Type
    {
        Start,
        Jail,
        GoToJail,
        Luck,
        Train,
        Square,
        House
    };

    #region Server
    [Server]
    public abstract void Action(Player player);
    #endregion

    #region Client
    [Client]
    private void Start()
    {
        if (overlay && TryGetComponent(out Image img))
            img.sprite = overlay;
    }

    #endregion
}
