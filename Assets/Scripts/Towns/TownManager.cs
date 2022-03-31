using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TownManager : NetworkBehaviour
{
    public GameObject townUIPrefab;
    public TownPrefab[] towns;

    private string townName;
    private TownUI activeTownUI;
    NetworkVariable<NetworkObjectReference> townNetworkReference = new NetworkVariable<NetworkObjectReference>();

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
            spawnTownServerRpc(townName);
        }
    }

    [ServerRpc]
    void spawnTownServerRpc(string townNameParam)
    {
        TownPrefab newTown = null;
        foreach (TownPrefab t in towns)
        {
            if (t.name == townNameParam)
            {
                newTown = t;
                break;
            }
        }
        //f
        GameObject townObject = Instantiate(newTown.prefab, transform.position, Quaternion.identity);
        townObject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        townObject.transform.eulerAngles = new Vector3(townObject.transform.eulerAngles.x, transform.eulerAngles.y, townObject.transform.rotation.eulerAngles.z);
        townObject.transform.SetParent(transform);
        
        townNetworkReference.Value = townObject.GetComponent<NetworkObject>();
    }

    public Town getTown()
    {
        if (townNetworkReference.Value.TryGet(out NetworkObject targetObject))
        {
            return targetObject.GetComponent<Town>();
        }

        return null;
    }

    public void setup(string townName)
    {
        this.townName = townName;
    }

    public void townButtonPressed()
    {
        if(activeTownUI != null)
        {
            Destroy(activeTownUI.gameObject);
            activeTownUI = null;
            GetComponent<Player>().playerInput.startRespondingToInput();
        }
        else
        {
            activeTownUI = Instantiate(townUIPrefab, Vector3.zero, Quaternion.identity).GetComponent<TownUI>();
            activeTownUI.setup(getTown(),this);
            GetComponent<Player>().playerInput.stopRespondingToInput();
        }
    }
}
