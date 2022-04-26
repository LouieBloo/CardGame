using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerTurnManager : NetworkBehaviour
{
    public bool forcingTurnOrder = true;
    public int buildTurnOrderTimeLimit = 4;
    public int creatureMoveTimeLimit = 5;

    public GameObject turnIndicatorPrefab;

    private NetworkList<NetworkObjectReference> playersInTurnOrder;
    private NetworkVariable<int> currentRound = new NetworkVariable<int>(0);

    public NetworkList<NetworkObjectReference> objectsInTurnOrder;
    private NetworkVariable<int> currentDay = new NetworkVariable<int>(0);
    private List<NetworkObjectReference> allObjectsTracking = new List<NetworkObjectReference>();
    private List<GameObject> activeTurnIndicators;

    private List<Player> allPlayers;

    public Transform spawnTurnIndicatorTransform;
    private NetworkObject activePermanentInTurnOrder;

    private NetworkedTimer timer;

    private enum Round
    {
        BuildCastCardsRecruit,
        CreatureMovement
    }

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
        timer = GetComponent<NetworkedTimer>();
        if (IsServer)
        {
            objectsInTurnOrder = new NetworkList<NetworkObjectReference>();
            playersInTurnOrder = new NetworkList<NetworkObjectReference>();
        }

        timer.subscribeToChanges(GlobalVars.gv.player.getUI().timerText);
    }

    public void start(List<Player> allPlayers)
    {
        this.allPlayers = allPlayers;
        startNewDay();
    }

    private void Update()
    {
        if (IsServer)
        {
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
            }
        }
    }

    ///////////////////////////////////////
    /////////// ROUNDS //////////////////////////
    //////////////////////////////////////////
    private void startNewDay()
    {
        currentRound.Value = 0;
        currentDay.Value++;

        playersInTurnOrder.Clear();
        calculatePlayerTurnOrder();

        startBuildingRound();

        updatePlayerFaceUIClientRpc(getActivePlayer().OwnerClientId);
        updatePlayerRoundPanelClientRpc(currentRound.Value, currentDay.Value);
    }

    void calculatePlayerTurnOrder()
    {
        foreach (Player p in allPlayers)
        {
            playersInTurnOrder.Add(p.GetComponent<NetworkObject>());
        }
    }

    private void startNewRound()
    {
        currentRound.Value += 1;
        if(currentRound.Value > 2)
        {
            startNewDay();
        }
        //creature movement
        else if(currentRound.Value == 1)
        {
            calculatePlayerTurnOrder();
            alertCreaturesOfNewTurn();
            startCreatureMovementRound();
        }
        //final build/recruit/cast
        else if (currentRound.Value == 2)
        {
            clearCreatureTurnIndicatorClientRpc();
            calculatePlayerTurnOrder();
            startBuildingRound();
        }

        
    }

    public void playerPassedRoundPriority()
    {
        playersInTurnOrder.RemoveAt(0);
        if(playersInTurnOrder.Count > 0)
        {
            //creature movement
            if (currentRound.Value == 1)
            {
                startCreatureMovementRound();
            }
            else if (currentRound.Value == 0 || currentRound.Value == 2 )
            {
                startBuildingRound();
            }
        }
        else
        {
            startNewRound();
        }
    }

    void startCreatureMovementRound()
    {
        Debug.Log("Starting Creature Round...");
        //recalculateTurnOrder(networkListToList(objectsInTurnOrder));
        recalculateTurnOrder(allObjectsTracking);
        updatePlayerFaceUIClientRpc(getActivePlayer().OwnerClientId);
        updatePlayerRoundPanelClientRpc(currentRound.Value, currentDay.Value);
        resetTimer(creatureMoveTimeLimit);
    }

    void startBuildingRound()
    {
        Debug.Log("Starting Building Round...");
        updatePlayerFaceUIClientRpc(getActivePlayer().OwnerClientId);
        updatePlayerRoundPanelClientRpc(currentRound.Value, currentDay.Value);
        resetTimer(buildTurnOrderTimeLimit);
    }

    NetworkObject getActivePlayer()
    {
        if (playersInTurnOrder.Count > 0)
        {
            if (playersInTurnOrder[0].TryGet(out NetworkObject targetObject))
            {
                return targetObject;
            }
        }

        return null;
    }

    public bool isPlayerAbleToBuildAndCast(ulong playerId)
    {
        if (!IsServer) { return false; }

        if(getActivePlayer().OwnerClientId == playerId) { return true; }
        return false;
    }

    void resetTimer(float time)
    {
        if(time > 0f)
        {
            timer.start(time,timerPopped);
        }
        else
        {
            timer.start(time, null);
        }
        
        startTimerClientRpc(time);
    }


    void timerPopped()
    {
        if (IsServer)
        {
            playerPassedRoundPriority();
        }
    }

    [ClientRpc]
    private void startTimerClientRpc(float time)
    {
        if (!IsServer)
        {
            timer.start(time,null);
        }
    }

    [ClientRpc]
    private void updatePlayerFaceUIClientRpc(ulong activePlayerId)
    {
        GlobalVars.gv.player.getUI().selectActivePlayerFace(activePlayerId);
    }

    [ClientRpc]
    private void updatePlayerRoundPanelClientRpc(int roundNumber,int dayNumber)
    {
        GlobalVars.gv.player.getUI().changeRoundUIUpdate(roundNumber,dayNumber);
    }
    ///////////////////////////////////////
    /////////// END ROUNDS /////////////
    //////////////////////////////////////////

    /// <summary>
    /// Returns the active creature in the move phase
    /// </summary>
    /// <returns></returns>
    public NetworkObject getActiveObject()
    {
        if (objectsInTurnOrder != null && objectsInTurnOrder.Count > 0 && objectsInTurnOrder[0].TryGet(out NetworkObject targetObject))
        {
            return targetObject;
        }

        return null;
    }

    /// <summary>
    /// When a player does an action on a creature such as move, defend, etc
    /// </summary>
    public void playerMadeCreatureMove(ulong creatureNetworkId)
    {
        if(currentRound.Value != 1 || getActiveObject().NetworkObjectId != creatureNetworkId) { return; }

        objectsInTurnOrder.RemoveAt(0);

        if (objectsInTurnOrder.Count > 0)
        {
            //tell first in line they have priority
            getActiveObject().GetComponent<Creature>().takePriority();
            //tell players to update their ui
            removeHeadOfTurnIndicatorClientRpc(objectsInTurnOrder[0]);
        }
        else
        {
            playerPassedRoundPriority();
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

    public bool isPlayerValidToMakeCreatureMove(ulong playerId)
    {
        if (!forcingTurnOrder) { return true; }

        if(getActivePlayerId() != playerId || currentRound.Value != 1)
        {
            return false;
        }

        return true;
    }

    public bool isObjectValidToMakeMove(NetworkObjectReference networkObject)
    {
        if (!forcingTurnOrder) { return true; }

        if (currentRound.Value == 1 && objectsInTurnOrder.Count > 0 && objectsInTurnOrder[0].NetworkObjectId == networkObject.NetworkObjectId)
        {
            return true;
        }

        return false;
    }

    public void addObjectToTurnOrder(NetworkObjectReference objectNetworkReference)
    {
        allObjectsTracking.Add(objectNetworkReference);
        objectsInTurnOrder.Add(objectNetworkReference);
        //recalculateTurnOrder(networkListToList(objectsInTurnOrder));
    }

    public void removeObjectFromTurnOrder(NetworkObjectReference objectNetworkReference)
    {
        /*if(objectsInTurnOrder[0].NetworkObjectId == objectNetworkReference.NetworkObjectId)
        {
            return;
        }*/
        allObjectsTracking.Remove(objectNetworkReference);
        objectsInTurnOrder.Remove(objectNetworkReference);
        recalculateTurnOrder(networkListToList(objectsInTurnOrder));
    }

    /// <summary>
    /// Calls the turn started function on all creatures
    /// </summary>
    private void alertCreaturesOfNewTurn()
    {
        foreach (NetworkObjectReference n in allObjectsTracking)
        {
            if (n.TryGet(out NetworkObject targetObject))
            {
                targetObject.GetComponent<Creature>().turnStarted();
            }
        }
        //recalculateTurnOrder(allObjectsTracking);
    }

    private List<NetworkObjectReference> networkListToList(NetworkList<NetworkObjectReference> sourceList)
    {
        List<NetworkObjectReference> finalList = new List<NetworkObjectReference>();
        foreach(NetworkObjectReference n in sourceList)
        {
            finalList.Add(n);
        }

        return finalList;
    }

    public void recalculateTurnOrder(List<NetworkObjectReference> objectsInTurn)
    {
        if (!IsServer) { return; }
        //Debug.Log("== Recalculating turn order ==");
        // Debug.Log("All Objects in turn: " + allObjectsTracking.Count);

        ulong activePlayerId = getActivePlayer().OwnerClientId;

        //get the creature stats and put them in an object easier to sort
        List<SortableObject> sortedObjects = new List<SortableObject>();
        foreach(NetworkObjectReference objectRef in objectsInTurn)
        {
            if (objectRef.TryGet(out NetworkObject targetObject))
            {
                //dont add creatures whos turn it is now
                if(targetObject.OwnerClientId != activePlayerId) { continue; }

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
        objectsInTurnOrder.Clear();
        NetworkObjectReference[] objectsInOrder = new NetworkObjectReference[sortedObjects.Count];
        for(int x = 0; x < sortedObjects.Count; x++) 
        {
            //Debug.Log(sortedObjects[x].stats.name + ": " + sortedObjects[x].stats.currentSpeed);
            objectsInOrder[x] = sortedObjects[x].networkObject;
            objectsInTurnOrder.Add(sortedObjects[x].networkObject);
        }

        //tell the first object in line that they have priority
        if(objectsInTurn.Count > 0)
        {
            getActiveObject().GetComponent<Creature>().takePriority();
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

        if(objectsInOrder != null && objectsInOrder.Length > 0)
        {
            highlightActivePermanentInTurnOrderClientRpc(objectsInOrder[0]);
        }
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
    void clearCreatureTurnIndicatorClientRpc()
    {
        if (activeTurnIndicators != null)
        {
            for(int x = 0; x < activeTurnIndicators.Count; x++)
            {
                Destroy(activeTurnIndicators[x]);
            }

            activeTurnIndicators.Clear();
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

            //tell the camera to track this target
            Camera.main.GetComponent<CameraTracker>().trackTarget(targetObject.transform,false);
        }
    }
}
