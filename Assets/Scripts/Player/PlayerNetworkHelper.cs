using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class PlayerNetworkHelper
{
    public static ulong getPlayerIdFromNetworkReference(NetworkObjectReference networkObject)
    {
        if (networkObject.TryGet(out NetworkObject targetObject))
        {
            return targetObject.OwnerClientId;
        }

        Debug.Log("Couldnt find player id from entwork reference");
        return 999999;
    }
}
