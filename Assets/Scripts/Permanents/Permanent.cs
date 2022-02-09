using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Permanent : NetworkBehaviour
{
    public enum Type
    {
        Creature,
        Chest,
        Wall
    }

    public Type type = Type.Creature;



    public void permanentAttacked(NetworkObjectReference attackingPermanent, Type attackingPermanentType)
    {
        if (!IsServer){Debug.Log("Permanent not server"); return;}

        if (type == Type.Creature && attackingPermanentType == Type.Creature)
        {
            if (attackingPermanent.TryGet(out NetworkObject targetObject))
            {
                GetComponent<Creature>().attacked(targetObject.GetComponent<Creature>().creatureObjectReference.GetComponent<DamageDealer>());
            }
        }
    }
}
