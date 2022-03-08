using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerTurnManager : NetworkBehaviour
{
    public bool forcingTurnOrder = true;

    public GameObject turnIndicatorPrefab;

    public NetworkList<NetworkObjectReference> objectsInTurnOrder;
    private List<NetworkObjectReference> allObjectsTracking = new List<NetworkObjectReference>();
    private GameObject[] activeTurnIndicators;

    public Transform spawnTurnIndicatorTransform;

    private struct SortableObject
    {
        public SortableObject(CreatureStats stats, NetworkObject networkObject)
        {
            this.stats = stats;
            this.networkObject = networkObject;
        }

        public CreatureStats stats;
        public NetworkObject networkObject;
    }

    private void Awake()
    {
        GlobalVars.gv.turnManager = this;
        if (IsServer)
        {
            objectsInTurnOrder = new NetworkList<NetworkObjectReference>();
        }
    }

    public override void OnNetworkSpawn()
    {
        

        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void addObjectToTurnOrder(NetworkObjectReference objectNetworkReference)
    {
        allObjectsTracking.Add(objectNetworkReference);
        recalculateTurnOrder();
    }

    public void removeObjectFromTurnOrder(NetworkObjectReference objectNetworkReference)
    {
        allObjectsTracking.Remove(objectNetworkReference);
        recalculateTurnOrder();
    }

    public void recalculateTurnOrder()
    {
        if (!IsServer) { return; }
        Debug.Log("== Recalculating turn order ==");

        Debug.Log("All Objects in turn: " + allObjectsTracking.Count);

        objectsInTurnOrder.Clear();

        //get the creature stats and put them in an object easier to sort
        List<SortableObject> sortedObjects = new List<SortableObject>();
        foreach(NetworkObjectReference objectRef in allObjectsTracking)
        {
            if (objectRef.TryGet(out NetworkObject targetObject))
            {
                SortableObject newCreatureObject = new SortableObject(targetObject.GetComponent<Creature>().getCurrentStats(), targetObject);

                //sorting logic
                if (sortedObjects.Count < 1)
                {
                    sortedObjects.Add(newCreatureObject);
                }
                else
                {
                    for(int compareIndex = 0; compareIndex < sortedObjects.Count; compareIndex++)
                    {
                        if(newCreatureObject.stats.currentSpeed > sortedObjects[compareIndex].stats.currentSpeed)
                        {
                            sortedObjects.Insert(compareIndex, newCreatureObject);
                            break;
                        }else if(compareIndex+1 == sortedObjects.Count)
                        {
                            //not bigger than anyone so we are the last
                            sortedObjects.Add(newCreatureObject);
                            break;
                        }
                    }
                }
            }
        }

        Debug.Log("Objects in final order: " + sortedObjects.Count);

        NetworkObjectReference[] objectsInOrder = new NetworkObjectReference[sortedObjects.Count];
        for(int x = 0; x < sortedObjects.Count; x++) 
        {
            //Debug.Log(sortedObjects[x].stats.name + ": " + sortedObjects[x].stats.currentSpeed);
            objectsInOrder[x] = sortedObjects[x].networkObject;
            objectsInTurnOrder.Add(sortedObjects[x].networkObject);
        }

        displayTurnOrderClientRpc(objectsInOrder);
    }



    [ClientRpc]
    void displayTurnOrderClientRpc(NetworkObjectReference[] objectsInOrder)
    {
        if(activeTurnIndicators != null)
        {
            foreach (GameObject g in activeTurnIndicators)
            {
                Destroy(g);
            }
        }

        int x = 0;
        activeTurnIndicators = new GameObject[objectsInOrder.Length];
        foreach (NetworkObjectReference n in objectsInOrder)
        {
            if (n.TryGet(out NetworkObject targetObject))
            {
                activeTurnIndicators[x] = Instantiate(turnIndicatorPrefab, spawnTurnIndicatorTransform);
                activeTurnIndicators[x].GetComponent<TurnIndicatorUI>().setup(targetObject.GetComponent<Creature>());
            }
            x++;
        }
    }
}
