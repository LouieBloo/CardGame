using System.Collections;
using System.Collections.Generic;
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

    public GameObject spawnCreaturePrefab;
    public GameObject creatureObjectReference;

    private DamageTaker damageTaker;
    private DamageDealer damageDealer;

    public struct CreatureInfo
    {
        public int health;
        public int amount;
    }

    private void Start()
    {
        damageTaker = GetComponent<DamageTaker>();
        damageTaker.subscribeToAmountChanges(uiNeedsUpdating);
        damageTaker.subscribeToHealthChanges(uiNeedsUpdating);

        damageDealer = GetComponent<DamageDealer>();

        if (IsOwner)
        {
            spawnCreatureObjectServerRpc();
        }

        updateUI();
    }

    private void Update()
    {
        //updateUI();
    }

    [ServerRpc]
    void spawnCreatureObjectServerRpc()
    {
        GameObject newObj = Instantiate(spawnCreaturePrefab, transform.position,Quaternion.identity);
        newObj.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        newObj.transform.SetParent(transform);
        newObj.transform.localRotation = spawnCreaturePrefab.transform.rotation;
        creatureObjectReference = newObj;
        
        damageTaker.setup(creatureObjectReference.GetComponent<CreatureStats>());
        damageDealer.setup(creatureObjectReference.GetComponent<CreatureStats>());
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
