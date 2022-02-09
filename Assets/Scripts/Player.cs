using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using HexMapTools;

public class Player : NetworkBehaviour
{
    public GameObject myPrefab;

    private int offset = 0;

    private ObjectSelecting objectSelector;
    private Grid grid;

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

        if (IsOwner)
        {
            objectSelector = GameObject.FindGameObjectsWithTag("Game")[0].GetComponent<ObjectSelecting>();
            grid = GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>();
        }
    }


    void Update()
    {
        if (!IsOwner) { return; }

        //Debug.Log(IsServer);
        if (Input.GetKeyDown(KeyCode.P))
        {
            testServerRpc();
        }
        if (Input.GetKeyDown(KeyCode.S) && objectSelector.isEmptyCellTargeted())
        {
            createPermanentServerRpc(objectSelector.getTargetedCell().getHexCoordinates());
        }
    }

    [ServerRpc]
    void createPermanentServerRpc(HexCoordinates cell)
    {
        Debug.Log("Inside RPC");
        if(grid == null)
        {
            GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>().createPermanentOnCell(cell, myPrefab,GetComponent<NetworkObject>().OwnerClientId);
        }
        else
        {
            grid.createPermanentOnCell(cell, myPrefab, GetComponent<NetworkObject>().OwnerClientId);
        }
        
    }

    [ServerRpc]
    void testServerRpc()
    {
        Debug.Log("luke: " + GetComponent<NetworkObject>().OwnerClientId);
    }


    [ServerRpc]
    void axeManServerRpc(int offset)
    {
        GameObject go = Instantiate(myPrefab, new Vector3(offset,0,0), Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
    }
}
