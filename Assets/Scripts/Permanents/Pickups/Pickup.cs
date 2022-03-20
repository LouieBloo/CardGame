using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    private string pickupName;

    public PickupPrefab[] pickupPrefabs;

    private GameObject itemObject;
    private PickupItem item;

    [System.Serializable]
    public class PickupPrefab
    {
        public string name;
        public GameObject prefab;
    }

    private void Start()
    {
        if (IsServer)
        {
            spawnCreatureObjectServerRpc();
        }
    }

    public void setSpawnParameters(string pickupName, HexDirection orientation)
    {
        this.pickupName = pickupName;
        //GetComponent<CreatureMovement>().setup(orientation);
    }

    [ServerRpc]
    void spawnCreatureObjectServerRpc()
    {
        GameObject pickupToSpawn = null;
        foreach(PickupPrefab p in pickupPrefabs)
        {
            if(p.name == this.pickupName)
            {
                pickupToSpawn = p.prefab;
            }
        }

        //Debug.Log(creatureName);    
        itemObject = Instantiate(pickupToSpawn, transform.position, Quaternion.identity);
        itemObject.GetComponent<NetworkObject>().Spawn();
        itemObject.transform.SetParent(transform);
        itemObject.transform.localRotation = pickupToSpawn.transform.rotation;
        itemObject.transform.localPosition = new Vector3(itemObject.transform.localPosition.x, pickupToSpawn.transform.position.y, itemObject.transform.localPosition.z);
        item = itemObject.GetComponent<PickupItem>();
        item.registerkillCallback(kill);

        //register this object as something that can take a turn
        //GlobalVars.gv.turnManager.addObjectToTurnOrder(GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void playerPickedUpServerRpc(ulong clientId)
    {
        item.playerPickedUp(clientId);
    }

    public void kill()
    {
        itemObject.GetComponent<NetworkObject>().Despawn(true);
        GetComponent<NetworkObject>().Despawn(true);
    }
}
