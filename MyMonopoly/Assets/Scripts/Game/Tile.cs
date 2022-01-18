using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Tile : MonoBehaviour
{
    [SerializeField] SpriteRenderer over;
    [Header("To change")]
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
    public abstract void Action(Player player);
    #endregion

    #region Client
    [Client]
    private void Start()
    {
        if (overlay && over)
            over.sprite = overlay;
    }

    #endregion
}
