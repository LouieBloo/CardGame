using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TownManager : NetworkBehaviour
{
    public TownPrefab[] towns;
    private Town activeTown;
    private string townName;

    [System.Serializable]
    public class TownPrefab
    {
        public string name;
        public GameObject prefab;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner)
        {
            testServerRpc();
            //townObject.GetComponent<NetworkObject>().Spawn();
        }
    }

    [ServerRpc]
    void testServerRpc()
    {
        TownPrefab newTown = null;
        foreach (TownPrefab t in towns)
        {
            if (t.name == townName)
            {
                newTown = t;
                break;
            }
        }
        //f
        GameObject townObject = Instantiate(newTown.prefab, transform);
        activeTown = townObject.GetComponent<Town>();
    }

    public void setup(string townName)
    {
        this.townName = townName;
    }
}
