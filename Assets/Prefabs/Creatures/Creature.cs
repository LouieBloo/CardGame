using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Creature : NetworkBehaviour, NetworkLoadable
{
    public enum CreatureActions
    {
        Attack, Defend, Move, Interact
    }

    public CreatureStaticUI ui;

    //public GameObject creatureObjectReference;
    NetworkVariable<NetworkObjectReference> creatureObjectReference = new NetworkVariable<NetworkObjectReference>();
    //List<CreatureModification> modifications = new List<CreatureModification>();
    

    private CreatureStats creatureStats;
    private DamageTaker damageTaker;
    private DamageDealer damageDealer;
    private CreatureMovement creatureMovement;
    private Attacker attacker;

    private NetworkVariable<Color> color = new NetworkVariable<Color>();

    [System.Serializable]
    public class CreaturePrefab
    {
        public string name;
        public GameObject prefab;
    }

    public CreaturePrefab[] creaturePrefabs;
    private Dictionary<string, GameObject> creaturePrefabMapping = new Dictionary<string, GameObject>();

    private string creatureName;


    public struct CreatureInfo
    {
        public int health;
        public int amount;
    }

    private void Start()
    {
        creatureMovement = GetComponent<CreatureMovement>();

        if (IsServer)
        {
            //setup the creature prefab mapping for performance reasons
            foreach (CreaturePrefab p in creaturePrefabs)
            {
                creaturePrefabMapping.Add(p.name, p.prefab);
            }

            creatureObjectReference.OnValueChanged += creatureObjectReferenceChanged;
        }

        damageTaker = GetComponent<DamageTaker>();
        damageTaker.subscribeToAmountChanges(uiNeedsUpdating);
        damageTaker.subscribeToHealthChanges(uiNeedsUpdating);

        damageDealer = GetComponent<DamageDealer>();
        attacker = GetComponent<Attacker>();

        if (IsServer)
        {
            spawnCreatureObjectServerRpc();
        }

        updateUI();
    }

    private void creatureObjectReferenceChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
    {
        Debug.Log("changeeeeeeee");
    }

    public void setSpawnParameters(string creatureName, HexDirection orientation)
    {
        this.creatureName = creatureName;
        GetComponent<CreatureMovement>().setup(orientation);
    }

    //we require false here as only the server can call this function
    [ServerRpc(RequireOwnership = false)]
    void spawnCreatureObjectServerRpc()
    {
        //Debug.Log(creatureName);    
        GameObject newObj = Instantiate(creaturePrefabMapping[creatureName], transform.position, Quaternion.identity);
        newObj.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        newObj.transform.SetParent(transform);
        newObj.transform.localRotation = creaturePrefabMapping[creatureName].transform.rotation;
        newObj.transform.localPosition = new Vector3(newObj.transform.localPosition.x, creaturePrefabMapping[creatureName].transform.position.y, newObj.transform.localPosition.z);

        creatureStats = newObj.GetComponent<CreatureStats>();
        color.Value = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<Player>().playerColor.Value;
        //setup damage taking and giving
        damageTaker.setup(creatureStats);
        damageDealer.setup(creatureStats);

        attacker.setup(creatureStats,newObj.GetComponent<Animator>());

        //setup creature movement
        creatureMovement.hexSpaceType.Value = creatureStats.hexSpaces.ToString();
        creatureMovement.hexSpaceDistance.Value = creatureStats.hexSpaceDistance;
        creatureMovement.speed.Value = creatureStats.baseSpeed;
        creatureMovement.movementRange.Value = creatureStats.baseMovementRange;

        creatureObjectReference.Value = newObj;

        //register this object as something that can take a turn
        GlobalVars.gv.turnManager.addObjectToTurnOrder(GetComponent<NetworkObject>());
    }

    public NetworkObject getCreatureObject(){
        if (creatureObjectReference.Value.TryGet(out NetworkObject targetObject))
        {
            return targetObject;
        }
        return null;
    }

    public void attacked(DamageDealer damageDealer)
    {
        if (IsServer)
        {
            DamageCalculator.calculateDamage(damageDealer, this.damageTaker);
        }
        else
        {
            Debug.Log("Cant apply damange as this isnt the server");
        }
    }

    public void killed()
    {
        GlobalVars.gv.grid.creatureDiedOnCell(GetComponent<NetworkObject>());
        getCreatureObject().Despawn(true);
        GetComponent<NetworkObject>().Despawn(true);
    }

    public CreatureStats getCurrentStats()
    {
        /*if (creatureObjectReference == null)
        {
            creatureObjectReference = transform.GetChild(1).gameObject;
        }*/
        //CreatureStats stats = new CreatureStats();
        CreatureStats baseStats = getCreatureObject().GetComponent<CreatureStats>();

        baseStats.currentDamage = damageDealer.getBaseDamage();
        //baseStats.baseHealth = baseStats.baseHealth;
        baseStats.currentArmor = damageTaker.getArmor();
        baseStats.currentSpeed = creatureMovement.speed.Value;
        //baseStats.name = baseStats.name;
        //baseStats.uiImage = baseStats.uiImage;
        baseStats.color = color.Value;

        return baseStats;
    }

    public void uiNeedsUpdating(int old, int newd)
    {
        updateUI();
    }


    void updateUI()
    {
        CreatureInfo parms = new CreatureInfo();
        parms.amount = damageTaker.getAmount();
        parms.health = damageTaker.getHealth();
        ui.updateAll(parms);
    }

    //note this is for the clients reference
    public bool isLoaded()
    {
        if(getCreatureObject() == null)
        {
            return false;
        }
        if(damageDealer == null || damageTaker == null || attacker == null)
        {
            return false;
        }
        if(creatureMovement == null)
        {
            return false;
        }


        return true;
    }
}
