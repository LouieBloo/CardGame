using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{

    public List<GameObject> playerSpawns;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += playerConnected;
    }

    void playerConnected(ulong playerId)
    {
        if (IsServer)
        {
            int spawnIndex = NetworkManager.Singleton.ConnectedClientsIds.Count-1;
            NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.transform.position = playerSpawns[spawnIndex].transform.position;
            playerSpawns[spawnIndex].gameObject.SetActive(false);
        }
        else
        {
            foreach(GameObject g in playerSpawns)
            {
                g.SetActive(false);
            }
        }
    }
}
