using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using HexMapTools;

public class Player : NetworkBehaviour
{
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

        if (objectSelector.isEmptyCellTargeted())
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                createCreatureServerRpc(objectSelector.getTargetedCell().getHexCoordinates(), "THIN_DRAGON");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                createCreatureServerRpc(objectSelector.getTargetedCell().getHexCoordinates(), "SWORDSMAN");
            }
        }
        
    }

    [ServerRpc]
    void createCreatureServerRpc(HexCoordinates cell,string creatureName)
    {
        if(grid == null)
        {
            GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>().createCreatureOnCell(cell, GetComponent<NetworkObject>().OwnerClientId, creatureName);
        }
        else
        {
            grid.createCreatureOnCell(cell, GetComponent<NetworkObject>().OwnerClientId, creatureName);
        }
        
    }

    [ServerRpc]
    void testServerRpc()
    {
        Debug.Log("luke: " + GetComponent<NetworkObject>().OwnerClientId);
    }


 
}
