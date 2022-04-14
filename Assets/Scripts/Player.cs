using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Networking;
using UnityEngine;
using HexMapTools;
using Unity.Collections;
using System;

public class Player : PlayerOwnedNetworkObject, NetworkLoadable
{
    private ObjectSelecting objectSelector;
    private Grid grid;

    public GameObject uiPrefab;
    private GameObject ui;

    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>();
    public NetworkVariable<FixedString64Bytes> facingOrientation = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<FixedString64Bytes> name = new NetworkVariable<FixedString64Bytes>();

    public GameObject playerDefaultsPrefab;
    private GameObject playerDefaultsGameObject;

    public GameObject playerTurnManagerPrefab;

    public PlayerStats playerStats;

    public GameObject townPrefab;
    public TownManager townManager;

    public PlayerInput playerInput;

    private Action readyCallback;
    //only public for debugging

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

            townManager.setup("CASTLE");

            playerDefaultsGameObject = Instantiate(playerDefaultsPrefab, Vector3.zero, Quaternion.identity);
            playerDefaultsGameObject.GetComponent<PlayerDefaults>().setup(playerInfoGatheredServerRpc);
            //setColorServerRpc(OwnerClientId == 0 ? Color.green : Color.blue);

            GlobalVars.gv.player = this;
        }

        if (IsServer)
        {
            
        }
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public void setReadyCallback(Action readyCallback)
    {
        this.readyCallback = readyCallback;
    }

    [ServerRpc]
    void playerInfoGatheredServerRpc(PlayerDefaults.PlayerDefaultInfo info) {
        foreach(NetworkClient n in NetworkManager.Singleton.ConnectedClientsList)
        {
            if(n.PlayerObject.GetComponent<Player>().playerColor.Value == info.color)
            {
                sendPlayerErrorClientRpc("That color has already been taken!");
                return;
            }
        }

        playerColor.Value = info.color;
        name.Value = info.name;

        destroyDefaultsClientRpc();

        readyCallback();
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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            townManager.getTown().buildBuildingServerRpc("BARRACKS");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            townManager.getTown().buildBuildingServerRpc("TOWN_BASE_UPGRADE");
        }

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

    public string getName()
    {
        return name.Value.ToString();
    }

    [ClientRpc]
    public void spawnSetupUIClientRpc(NetworkObjectReference[] allPlayers)
    {
        if (IsOwner)
        {
            Debug.Log(allPlayers.Length);
            List<Player> players = new List<Player>();
            foreach(NetworkObjectReference r in allPlayers)
            {
                if (r.TryGet(out NetworkObject targetObject))
                {
                    players.Add(targetObject.GetComponent<Player>());
                }
            }
            ui.GetComponent<PlayerUI>().createPlayerFaces(players);
        }
    }

    public bool isLoaded()
    {
        try
        {
            if (name.Value.ToString() != "")
            {
                return true;
            }
        }
        catch(Exception e)
        {
        }
        

        return false;
    }
}
