using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TreasureChest : PickupItem
{
    public int gold;
    public GameObject goldTextPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public override void playerPickedUp(ulong clientId)
    {
        //NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerStats>().modifyGoldServerRpc(gold);
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>().playerStats.modifyGold(gold);
        GlobalVars.gv.turnManager.playerMadeMoveServerRpc();
        spawnGoldTextClientRpc(gold);
        kill();
    }

    [ClientRpc]
    void spawnGoldTextClientRpc(int amount)
    {
        goldTextPrefab.GetComponent<DamageNumber>().Spawn(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), amount);
    }
}
