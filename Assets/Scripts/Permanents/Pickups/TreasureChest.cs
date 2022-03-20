using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TreasureChest : PickupItem
{
    public int gold;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public override void playerPickedUp(ulong clientId)
    {
        //NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerStats>().modifyGoldServerRpc(gold);
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerStats>().modifyGold(gold);
        GlobalVars.gv.turnManager.playerMadeMoveServerRpc();
        kill();
    }
}
