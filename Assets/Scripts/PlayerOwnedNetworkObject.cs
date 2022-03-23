using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerOwnedNetworkObject : NetworkBehaviour
{

    [ClientRpc]
    protected virtual void sendPlayerErrorClientRpc(string error)
    {
        if (IsOwner)
        {
            GlobalVars.gv.gameUI.alertMessage(error);
        }
    }

    public Player getPlayer()
    {
        return NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<Player>();
    }
}
