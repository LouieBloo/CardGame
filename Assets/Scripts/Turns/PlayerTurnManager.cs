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
    private NetworkVariable<int> currentDay = new NetworkVariable<int>(0);
    private List<NetworkObjectReference> allObjectsTracking = new List<NetworkObjectReference>();
    private List<GameObject> activeTurnIndicators;

    public Transform spawnTurnIndicatorTransform;
    private NetworkObject activePermanentInTurnOrder;

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

    [ServerRpc]
    public void playerMadeMoveServerRpc()
    {
        objectsInTurnOrder.RemoveAt(0);
        if(objectsInTurnOrder.Count > 0)
        {
            removeHeadOfTurnIndicatorClientRpc(objectsInTurnOrder[0]);
        }
        else
        {
            Debug.Log("Need new turn now...");
        }
    }

    private ulong getActivePlayerId()
    {
        if (objectsInTurnOrder.Count > 0)
        {
            if (objectsInTurnOrder[0].TryGet(out NetworkObject targetObject))
            {
                return targetObject.OwnerClientId;
            }
        }

        return 99999;
    }

    public bool isPlayerValidToMakeMove(ulong playerId)
    {
        if (!forcingTurnOrder) { return true; }

        if(getActivePlayerId() != playerId)
        {
            return false;
        }

        return true;
    }

    public bool isObjectValidToMakeMove(NetworkObjectReference networkObject)
    {
        if (!forcingTurnOrder) { return true; }

        if (objectsInTurnOrder.Count > 0 && objectsInTurnOrder[0].NetworkObjectId == networkObject.NetworkObjectId)
        {
            return true;
        }

        return false;
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
        //Debug.Log("== Recalculating turn order ==");

       // Debug.Log("All Objects in turn: " + allObjectsTracking.Count);

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

        //Debug.Log("Objects in final order: " + sortedObjects.Count);

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
        activeTurnIndicators = new List<GameObject>();
        foreach (NetworkObjectReference n in objectsInOrder)
        {
            if (n.TryGet(out NetworkObject targetObject))
            {
                GameObject indicator = Instantiate(turnIndicatorPrefab, spawnTurnIndicatorTransform);
                indicator.GetComponent<TurnIndicatorUI>().setup(targetObject.GetComponent<Creature>());
                activeTurnIndicators.Add(indicator);
            }
            x++;
        }

        highlightActivePermanentInTurnOrderClientRpc(objectsInOrder[0]);
    }

    [ClientRpc]
    void removeHeadOfTurnIndicatorClientRpc(NetworkObjectReference newActivePermanent)
    {
        if (activeTurnIndicators != null)
        {
            Destroy(activeTurnIndicators[0]);
            activeTurnIndicators.RemoveAt(0);
            highlightActivePermanentInTurnOrderClientRpc(newActivePermanent);
        }
    }

    [ClientRpc]
    void highlightActivePermanentInTurnOrderClientRpc(NetworkObjectReference activePermanent)
    {
        if(activePermanentInTurnOrder != null)
        {
            activePermanentInTurnOrder.GetComponent<Selectable>().deHighlight();
        }

        if (activePermanent.TryGet(out NetworkObject targetObject))
        {
            activePermanentInTurnOrder = targetObject;
            activePermanentInTurnOrder.GetComponent<Selectable>().highlight();
        }
    }
}
