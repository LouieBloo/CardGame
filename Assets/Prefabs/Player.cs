using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public GameObject myPrefab;

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
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && IsOwner)
        {
            axeManServerRpc(offset);
            offset += 1;
        }
    }

    [ServerRpc]
    void axeManServerRpc(int offset)
    {
        GameObject go = Instantiate(myPrefab, new Vector3(offset,0,0), Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
    }
}
