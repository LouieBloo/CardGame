using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Creature : NetworkBehaviour
{
    public enum CreatureActions
    {
        Attack, Defend, Move, Interact
    }

    public CreatureStaticUI ui;

    public GameObject creatureObjectReference;

    private DamageTaker damageTaker;
    private DamageDealer damageDealer;
    private CreatureMovement creatureMovement;

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
           foreach(CreaturePrefab p in creaturePrefabs)
           {
                creaturePrefabMapping.Add(p.name, p.prefab);
           } 
        }

        damageTaker = GetComponent<DamageTaker>();
        damageTaker.subscribeToAmountChanges(uiNeedsUpdating);
        damageTaker.subscribeToHealthChanges(uiNeedsUpdating);

        damageDealer = GetComponent<DamageDealer>();

        if (IsServer)
        {
            spawnCreatureObjectServerRpc();
        }

        updateUI();
    }

    private void Update()
    {
        //updateUI();
    }

    public void setSpawnParameters(string creatureName, HexDirection orientation)
    {
        this.creatureName = creatureName;
        GetComponent<CreatureMovement>().setup(orientation);
    }

    //we require false here as only the server can call this function
    [ServerRpc(RequireOwnership =false)]
    void spawnCreatureObjectServerRpc()
    {
        Debug.Log(creatureName);
        GameObject newObj = Instantiate(creaturePrefabMapping[creatureName], transform.position,Quaternion.identity);
        newObj.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        newObj.transform.SetParent(transform);
        newObj.transform.localRotation = creaturePrefabMapping[creatureName].transform.rotation;
        newObj.transform.localPosition = new Vector3(newObj.transform.localPosition.x, creaturePrefabMapping[creatureName].transform.position.y, newObj.transform.localPosition.z);
        creatureObjectReference = newObj;
        
        damageTaker.setup(creatureObjectReference.GetComponent<CreatureStats>());
        damageDealer.setup(creatureObjectReference.GetComponent<CreatureStats>());

        //setup creature movement
        creatureMovement.hexSpaceType.Value = creatureObjectReference.GetComponent<CreatureStats>().hexSpaces.ToString();
        creatureMovement.hexSpaceDistance.Value = creatureObjectReference.GetComponent<CreatureStats>().hexSpaceDistance;
        creatureMovement.speed.Value = creatureObjectReference.GetComponent<CreatureStats>().baseSpeed;
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
        creatureObjectReference.GetComponent<NetworkObject>().Despawn(true);
        GetComponent<NetworkObject>().Despawn(true);
    }

    public CreatureStats getCurrentStats()
    {
        if (creatureObjectReference == null)
        {
            creatureObjectReference = transform.GetChild(1).gameObject;
        }
        CreatureStats stats = new CreatureStats();
        CreatureStats baseStats = creatureObjectReference.GetComponent<CreatureStats>();

        stats.currentDamage = damageDealer.getBaseDamage();
        stats.baseHealth = baseStats.baseHealth;
        stats.currentArmor = damageTaker.getArmor();
        stats.currentSpeed = creatureMovement.speed.Value;
        stats.name = baseStats.name;

        return stats;
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
}
