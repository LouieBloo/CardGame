using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Permanent : NetworkBehaviour
{
    public enum Type
    {
        Creature,
        Pickup,
        Wall,
        Town
    }

    public Type type = Type.Creature;

    private NetworkList<Vector3> cellsOccupied;

    private void Awake()
    {
        cellsOccupied = new NetworkList<Vector3>();
    }

    public void permanentAttacked(NetworkObjectReference attackingPermanent, Type attackingPermanentType, Action retaliationCallback)
    {
        if (!IsServer){Debug.Log("Permanent not server"); return;}

        if (type == Type.Creature && attackingPermanentType == Type.Creature)
        {
            if (attackingPermanent.TryGet(out NetworkObject targetObject))
            {
                GetComponent<Creature>().attacked(targetObject.GetComponent<Creature>().GetComponent<DamageDealer>(), retaliationCallback);
            }
        }
        else if(type == Type.Town)
        {
            if (attackingPermanent.TryGet(out NetworkObject targetObject))
            {
                DamageCalculator.calculateDamage(targetObject.GetComponent<Creature>().GetComponent<DamageDealer>(), GetComponent<DamageTaker>());
                retaliationCallback();
            }
        }
    }

    [ServerRpc (RequireOwnership =false)]
    public void setOccupiedCellsServerRpc(Vector3[] positions)
    {
        cellsOccupied.Clear();
        foreach(Vector3 p in positions)
        {
            cellsOccupied.Add(p);
        }
    }

    public NetworkList<Vector3> getCellsOccupied()
    {
        return cellsOccupied;
    }
}
