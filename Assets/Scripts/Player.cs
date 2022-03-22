using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using HexMapTools;

public class Player : PlayerOwnedNetworkObject
{
    private ObjectSelecting objectSelector;
    private Grid grid;

    public GameObject uiPrefab;
    private GameObject ui;

    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>();

    public GameObject playerDefaultsPrefab;
    private GameObject playerDefaultsGameObject;

    public GameObject playerTurnManagerPrefab;

    private PlayerStats playerStats;

    public GameObject townPrefab;
    private Town town;

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
            Debug.Log("Spawning object");
            GameObject turnManager = Instantiate(playerTurnManagerPrefab, Vector3.zero, Quaternion.identity);
            turnManager.GetComponent<NetworkObject>().Spawn();
        }

        if (IsOwner)
        {
            objectSelector = GameObject.FindGameObjectsWithTag("Game")[0].GetComponent<ObjectSelecting>();
            grid = GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>();
            ui = Instantiate(uiPrefab, Vector3.zero, Quaternion.identity);
            ui.GetComponent<PlayerUI>().setup(this);

            //playerDefaultsGameObject = Instantiate(playerDefaultsPrefab, Vector3.zero, Quaternion.identity);
            //playerDefaultsGameObject.GetComponent<PlayerDefaults>().setup(setColorServerRpc);
            setColorServerRpc(OwnerClientId == 0 ? Color.green : Color.blue);

            GlobalVars.gv.player = this;

            
        }

    }

    private void Start()
    {
        if (IsServer)
        {
            GameObject townObject = Instantiate(townPrefab, transform.position,Quaternion.identity);
            townObject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
            townObject.GetComponent<TownManager>().setup("CASTLE");
        }
    }

    [ServerRpc]
    void setColorServerRpc(Color c) {
        foreach(NetworkClient n in NetworkManager.Singleton.ConnectedClientsList)
        {
            if(n.PlayerObject.GetComponent<Player>().playerColor.Value == c)
            {
                sendPlayerErrorClientRpc("That color has already been taken!");
                return;
            }
        }

        Debug.Log("Server got color: " + c);
        playerColor.Value = c;
        destroyDefaultsClientRpc();
    }

    [ClientRpc]
    void destroyDefaultsClientRpc()
    {
        if (IsOwner)
        {
            Destroy(playerDefaultsGameObject);
        }
    }

    void Update()
    {
        if (!IsOwner) { return; }

        if (objectSelector.isEmptyCellTargeted())
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                createCreatureServerRpc(objectSelector.getTargetedCell().getHexCoordinates(), "THIN_DRAGON");
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                createCreatureServerRpc(objectSelector.getTargetedCell().getHexCoordinates(), "SWORDSMAN");
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                createCreatureServerRpc(objectSelector.getTargetedCell().getHexCoordinates(), "ARCHER");
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                createCreatureServerRpc(objectSelector.getTargetedCell().getHexCoordinates(), "SPIDER");
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                createCreatureServerRpc(objectSelector.getTargetedCell().getHexCoordinates(), "SKELETON");
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                town.buildBuildingServerRpc("BARRACKS");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                town.buildBuildingServerRpc("TOWN_BASE_UPGRADE");
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

}
