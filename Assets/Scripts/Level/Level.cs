using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Level : NetworkBehaviour
{
    public PlayerStart[] playerStarts;
    public List<Player> allPlayers;

    public bool waitForAllPlayers = false;
    [System.Serializable]
    public class PlayerStart
    {
        public HexDirection facingDirection;
        public GameObject spawnLocation;
        public GameObject[] baseCells;
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
        if(!waitForAllPlayers || NetworkManager.Singleton.ConnectedClientsIds.Count >= playerStarts.Length)
        {
            for(int x = 0; x < playerStarts.Length; x++)
            {
                foreach(GameObject g in playerStarts[x].baseCells)
                {
                    if(allPlayers.Count-1 >= x)
                    {
                        GlobalVars.gv.grid.createTownPermanentOnCell(new HexCoordinates[] { GlobalVars.gv.grid.getHexCoordinatesFromPosition(g.transform.position) }, allPlayers[x].OwnerClientId, "CastleTownPermanent");
                    }
                }
            }
        }
    }
}
