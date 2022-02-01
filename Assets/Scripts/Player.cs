using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public GameObject myPrefab;

    public GameObject grid;

    private int offset = 0;

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer && IsOwner)
        {
            //spawnGridServerRpc();
        }
    }


    void Update()
    {
        //Debug.Log(IsServer);
        if (IsOwner && Input.GetKeyDown(KeyCode.P))
        {
            testServerRpc();
        }
    }

    [ServerRpc]
    void testServerRpc()
    {
        Debug.Log("luke: " + GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc]
    void spawnGridServerRpc()
    {
        GameObject go = Instantiate(grid, new Vector3(0, 0, 0), grid.transform.rotation);
        go.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    void axeManServerRpc(int offset)
    {
        GameObject go = Instantiate(myPrefab, new Vector3(offset,0,0), Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
    }
}
