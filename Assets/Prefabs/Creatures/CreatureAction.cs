using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CreatureAction : NetworkBehaviour
{
    
    public void handleAction(PermanentCell targetActionCell, Creature.CreatureActions action)
    {
        if(action == Creature.CreatureActions.Attack)
        {
            //if this function is called we know its safe to attack
            targetActionCell.getAttachedPermanent().permanentAttacked(GetComponent<NetworkObject>(), Permanent.Type.Creature);
        }
    }
}
