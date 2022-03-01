using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageTaker : NetworkBehaviour
{
    public enum ArmorType
    {
        Physical,
        Magical,
    }

    public ArmorType armorType = ArmorType.Physical;

    [SerializeField] private int baseHealth = 0;
    [SerializeField] private NetworkVariable<int> health = new NetworkVariable<int>();

    [SerializeField] private NetworkVariable<int> baseArmor = new NetworkVariable<int>();

    [SerializeField] private NetworkVariable<int> amount = new NetworkVariable<int>();

    private Permanent permanent;
    private Modifiable modifiable;

    public GameObject damageFloatingTextPrefab;

    private void Start()
    {
        permanent = GetComponent<Permanent>();
        modifiable = GetComponent<Modifiable>();
    }

    public void setup(CreatureStats creatureStats)
    {
        if (IsServer)
        {
            health.Value = creatureStats.baseHealth;
            baseHealth = creatureStats.baseHealth;

            armorType = creatureStats.armorType;
            baseArmor.Value = creatureStats.baseArmor;

            amount.Value = 3;
        }
    }

    public int getArmor()
    {
        if(permanent.type == Permanent.Type.Creature)
        {
            //return baseArmor + permanent.GetComponent<Creature>().defensePoints.Value;
            return baseArmor.Value + modifiable.getArmorModification();
        }

        return 0;
    }

    public void takeDamage(int damage)
    {
        if (IsServer)
        {
            if(damage > 0)
            {
                int healthAfterDamage = health.Value - damage;
                if (healthAfterDamage > 0)
                {
                    health.Value -= damage;
                }
                else
                {
                    float totalHealth = health.Value + (baseHealth * (amount.Value - 1));
                    float remainingHealth = totalHealth - damage;
                    amount.Value = Mathf.CeilToInt(remainingHealth / baseHealth);
                    health.Value = (int)(remainingHealth % baseHealth);
                }
            }
            else
            {
                //so the user doesnt see negative numbers
                damage = 0;
            }
            
            spawnDamageFloatingTextClientRpc(damage);

            if(amount.Value <= 0)
            {
                die();
            }
        }
    }

    [ClientRpc]
    void spawnDamageFloatingTextClientRpc(int damage)
    {
        damageFloatingTextPrefab.GetComponent<DamageNumber>().Spawn(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), damage);
    }

    private void die()
    {
        if(IsServer && amount.Value <= 0)
        {
            GetComponent<Creature>().killed();
        }
    }

    public void subscribeToHealthChanges(NetworkVariable<int>.OnValueChangedDelegate callback)
    {
        health.OnValueChanged += callback;
    }

    public void subscribeToAmountChanges(NetworkVariable<int>.OnValueChangedDelegate callback)
    {
        amount.OnValueChanged += callback;
    }

    public int getHealth()
    {
        return health.Value;
    }

    public int getAmount()
    {
        return amount.Value;
    }
}
