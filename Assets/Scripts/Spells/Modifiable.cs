using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Modifiable : NetworkBehaviour
{
    NetworkList<NetworkObjectReference> attachedModifications;

    private void Awake()
    {
        attachedModifications = new NetworkList<NetworkObjectReference>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void applyModification(NetworkObjectReference modificationReference)
    {
        if (IsServer)
        {
            attachedModifications.Add(modificationReference);
        }
    }

    public void removeModification(NetworkObjectReference modificationReference)
    {
        if (IsServer)
        {
            attachedModifications.Remove(modificationReference);
        }
    }

    public int getArmorModification()
    {
        int returnTotal = 0;
        foreach(NetworkObjectReference networkRef in attachedModifications)
        {
            if (networkRef.TryGet(out NetworkObject targetObject))
            {
                foreach(CreatureModification modifier in targetObject.GetComponents<CreatureModification>())
                {
                    //CreatureModification modifier = targetObject.GetComponent<CreatureModification>();
                    returnTotal += modifier.armorDelta;
                }
            }
            
        }

        return returnTotal;
    }
}
