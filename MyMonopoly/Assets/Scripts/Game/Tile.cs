using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Tile : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer over;
    [Header("To change")]
    [SerializeField] protected int _id;
    [SerializeField] protected Type type;
    [SerializeField] protected Sprite overlay = null;

    public enum Type
    {
        Start,
        Jail,
        GoToJail,
        Luck,
        Train,
        Square,
        House,
        Taxes
    };

    #region Server
    public abstract void Action(Player player);
    #endregion

    #region Client
    private void Start()
    {
        Debug.Log(over);
        Debug.Log(overlay);
    }

    #endregion
}
