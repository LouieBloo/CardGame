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

    [SerializeField] private int baseArmor = 0;

    [SerializeField] private NetworkVariable<int> amount = new NetworkVariable<int>();

    private Permanent permanent;


    private void Start()
    {
        permanent = GetComponent<Permanent>();
    }

    public void setup(CreatureStats creatureStats)
    {
        if (IsServer)
        {
            health.Value = creatureStats.baseHealth;
            baseHealth = creatureStats.baseHealth;

            armorType = creatureStats.armorType;
            baseArmor = creatureStats.baseArmor;

            amount.Value = 3;
        }
    }

    public int getArmor()
    {
        if(permanent.type == Permanent.Type.Creature)
        {
            //return baseArmor + permanent.GetComponent<Creature>().defensePoints.Value;
            return baseArmor;
        }

        return 0;
    }

    public void takeDamage(int damage)
    {
        if (IsServer)
        {
            int healthAfterDamage = health.Value - damage;
            if(healthAfterDamage > 0)
            {
                health.Value -= damage;
            }
            else
            {
                float totalHealth = health.Value + (baseHealth * (amount.Value-1));
                float remainingHealth = totalHealth - damage;
                amount.Value = Mathf.CeilToInt(remainingHealth / baseHealth);
                health.Value = (int)(remainingHealth % baseHealth);
            }
        }
    }

    private void die()
    {
        if(IsServer && amount.Value <= 0)
        {
            Debug.Log("DIE PERMANET!");
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
