using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageDealer : NetworkBehaviour
{
    public enum DamageType
    {
        Physical,
        Magical,
    }

    public enum PhysicalType
    {
        Normal,
        Piercing
    }

    public enum MagicType
    {
        Fire,
        Ice,
        Water,
        Air,
        Balistic
    }

    public DamageType damageType = DamageType.Physical;
    public PhysicalType physicalDamageType = PhysicalType.Normal;
    public MagicType magicType = MagicType.Fire;

    [SerializeField] private NetworkVariable<int> baseDamage = new NetworkVariable<int>();

    private Permanent permanent;

    private void Start()
    {
        permanent = GetComponent<Permanent>();
    }

    public void setup(CreatureStats creatureStats)
    {
        if (IsServer)
        {
            damageType = creatureStats.damageType;
            physicalDamageType = creatureStats.physicalDamageType;

            baseDamage.Value = creatureStats.baseDamage;
        }
    }

    public int getBaseDamage()
    {
        //if we are a creature we return our base damage modifed with our player stats
        if(permanent != null && permanent.type == Permanent.Type.Creature)
        {
            //return (transform.parent.GetComponent<Creature>().attackPoints.Value * 2) + baseDamage.Value;
            return baseDamage.Value;
        }
        else
        {
            return baseDamage.Value;
        }

        return 0;
    }

    public int getWeaponDamage()
    {
        return 0;
    }

    public void turnStarted()
    {
    }

    public void turnEnded()
    {
    }
}
