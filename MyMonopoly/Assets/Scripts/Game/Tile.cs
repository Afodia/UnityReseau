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
    [Server]
    public int GetId()
    {
        Debug.Log($"id : {_id}");
        return _id;
    }

    public abstract void Action(MyNetworkPlayer player);
    #endregion

    #region Client
    void Start() { }

    public abstract Vector3 GetPlayerPosition();

    #endregion
}
