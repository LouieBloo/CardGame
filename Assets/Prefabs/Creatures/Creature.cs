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

    public NetworkVariable<int> attackPoints = new NetworkVariable<int>();
    public NetworkVariable<int> defensePoints = new NetworkVariable<int>();

    public CreatureStaticUI ui;

    public GameObject spawnCreaturePrefab;
    public GameObject creatureObjectReference;

    private DamageTaker damageTaker;

    public struct CreatureStats
    {
        public int health;
        public int amount;
    }

    private void Start()
    {
        if (IsOwner)
        {
            spawnCreatureObjectServerRpc();
        }
    }

    private void Update()
    {
        updateUIs();
    }

    [ServerRpc]
    void spawnCreatureObjectServerRpc()
    {
        GameObject newObj = Instantiate(spawnCreaturePrefab, transform.position,Quaternion.identity);
        newObj.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        newObj.transform.SetParent(transform);
        newObj.transform.localRotation = spawnCreaturePrefab.transform.rotation;
        creatureObjectReference = newObj;

        damageTaker = creatureObjectReference.GetComponent<DamageTaker>();
        damageTaker.subscribeToAmountChanges(uiNeedsUpdating);
        updateUIs();
    }

    public void attacked(DamageDealer damageDealer)
    {
        if (IsServer)
        {
            DamageCalculator.calculateDamage(damageDealer, damageTaker);
        }
        else
        {
            Debug.Log("Cant apply damange as this isnt the server");
        }
    }

    public void uiNeedsUpdating(int old, int newd)
    {
        updateUIs();
    }

    void updateUIs()
    {
        CreatureStats parms = new CreatureStats();
        parms.amount = damageTaker.getAmount();
        parms.health = damageTaker.getHealth();
        ui.updateAll(parms);
    }
}
