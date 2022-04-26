using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Level : NetworkBehaviour
{
    public PlayerStart[] playerStarts;
    private List<Player> allPlayers = new List<Player>();

    public GameObject playerTurnManagerPrefab;

    public bool waitForAllPlayers = false;

    public GameObject waitInputOption;

    public GameObject gridToSpawn;

    int readiedPlayerCount = 0;

    [System.Serializable]
    public class PlayerStart
    {
        public HexDirection facingDirection;
        public GameObject spawnLocation;
        public GameObject[] baseCells;
        public GameObject[] spawnCells;
        public GameObject[] gateCells;
    }
   

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += playerConnected;
    }

    void playerConnected(ulong playerId)
    {
        if (IsServer)
        {
            int spawnIndex = NetworkManager.Singleton.ConnectedClientsIds.Count - 1;
            
            NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.transform.position = playerStarts[spawnIndex].spawnLocation.transform.position;
            NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.transform.rotation = playerStarts[spawnIndex].spawnLocation.transform.rotation;
            NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<Player>().facingOrientation.Value = playerStarts[spawnIndex].facingDirection.ToString();
            NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<Player>().setReadyCallback(playerReady);
            playerStarts[spawnIndex].spawnLocation.SetActive(false);

            allPlayers.Add(NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<Player>());
        }
        else
        {
            foreach (PlayerStart p in playerStarts)
            {
                p.spawnLocation.SetActive(false);
            }
        }
    }

    void playerReady()
    {
        readiedPlayerCount++;
        if (!waitForAllPlayers || readiedPlayerCount >= NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            for (int x = 0; x < playerStarts.Length; x++)
            {
                //GameObject grid = Instantiate(gridToSpawn, Vector3.zero, Quaternion.identity);
                //grid.GetComponent<NetworkObject>().Spawn();

                foreach (GameObject g in playerStarts[x].baseCells)
                {
                    if (allPlayers.Count - 1 >= x)
                    {
                        GlobalVars.gv.grid.createTownPermanentOnCell(new HexCoordinates[] { GlobalVars.gv.grid.getHexCoordinatesFromPosition(g.transform.position) }, allPlayers[x].OwnerClientId, "CastleTownPermanent");
                    }
                }

                if (allPlayers.Count - 1 >= x)
                {
                    NetworkObjectReference[] allPlayersReferences = new NetworkObjectReference[allPlayers.Count];
                    for(int y = 0; y < allPlayers.Count; y++)
                    {
                        allPlayersReferences[y] = allPlayers[y].GetComponent<NetworkObject>();
                    }

                    allPlayers[x].spawnSetupUIClientRpc(allPlayersReferences);


                    //tell town to spawn starting creatures
                    allPlayers[x].townManager.getTown().startGame(playerStarts[x]);
                }
            }

            GameObject turnManager = Instantiate(playerTurnManagerPrefab, Vector3.zero, Quaternion.identity);
            turnManager.GetComponent<NetworkObject>().Spawn();
            turnManager.GetComponent<PlayerTurnManager>().start(allPlayers);

            Destroy(waitInputOption);
        }
    }


    public void waitForTurnCheckbox(bool value)
    {
        this.waitForAllPlayers = value;
    }
}
