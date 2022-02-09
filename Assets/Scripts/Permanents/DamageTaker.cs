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

    [SerializeField] private int baseHealth = 15;
    [SerializeField] private NetworkVariable<int> health = new NetworkVariable<int>();

    [SerializeField] private int baseArmor = 0;

    [SerializeField] private NetworkVariable<int> amount = new NetworkVariable<int>(1);

    private Permanent permanent;



    private void Start()
    {
        permanent = transform.parent.GetComponent<Permanent>();

        if (IsServer)
        {
            health.Value = baseHealth;
        }
    }

    public int getArmor()
    {
        if(permanent.type == Permanent.Type.Creature)
        {
            return baseArmor + permanent.GetComponent<Creature>().defensePoints.Value;
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
                //health.Value -= damage;
                amount.Value -= damage;
            }
            else
            {
                healthAfterDamage *= -1;
                Debug.Log("Amount logic here...");
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
