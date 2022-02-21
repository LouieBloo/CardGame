using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using HexMapTools;

public class Player : NetworkBehaviour
{
    private ObjectSelecting objectSelector;
    private Grid grid;

    private SpellBook spellBook;

    public GameObject uiPrefab;
    private GameObject ui;

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
            ui = Instantiate(uiPrefab, Vector3.zero, Quaternion.identity);
            ui.GetComponent<PlayerUI>().setup(this);
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
            if (Input.GetKeyDown(KeyCode.M))
            {
                createCreatureServerRpc(objectSelector.getTargetedCell().getHexCoordinates(), "SWORDSMAN");
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                createCreatureServerRpc(objectSelector.getTargetedCell().getHexCoordinates(), "ARCHER");
            }
        }
        
    }

    [ServerRpc]
    void createCreatureServerRpc(HexCoordinates cell,string creatureName)
    {
        HexCoordinates[] cellsToSpawnOn = new HexCoordinates[creatureName == "THIN_DRAGON" ? 2 : 1];
        cellsToSpawnOn[0] = cell;
        if(cellsToSpawnOn.Length > 1)
        {
            cellsToSpawnOn[1] = HexUtility.GetNeighbour(cell, HexDirection.W);
        }

        if(grid == null)
        {
            GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>().createCreatureOnCell(cellsToSpawnOn, GetComponent<NetworkObject>().OwnerClientId, creatureName);
        }
        else
        {
            grid.createCreatureOnCell(cellsToSpawnOn, GetComponent<NetworkObject>().OwnerClientId, creatureName);
        }
        
    }

    [ServerRpc]
    void testServerRpc()
    {
        Debug.Log("luke: " + GetComponent<NetworkObject>().OwnerClientId);
    }


 
}
