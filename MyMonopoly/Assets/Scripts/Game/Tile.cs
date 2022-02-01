using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Tile : NetworkBehaviour
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
        return _id;
    }

    public abstract void Action(MyNetworkPlayer player, int tileId = 0);

    #endregion
    #region Client

    void Start() { }

    public abstract Vector3 GetPlayerPosition(int playerId);

    [Client]
    public int ClientGetId()
    {
        return _id;
    }

    [TargetRpc]
    public void RpcSetId(int id)
    {
        this._id = id;
    }

    #endregion
}
