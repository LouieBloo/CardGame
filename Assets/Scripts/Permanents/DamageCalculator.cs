using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculator 
{
    public static int calculateDamage(DamageDealer attacker, DamageTaker victim)
    {
        int totalDamage = 0;
        if (!attacker) { Debug.Log("no attacker"); }
        if (!victim) { Debug.Log("no victim"); }
        if (attacker.damageType == DamageDealer.DamageType.Physical && victim.armorType == DamageTaker.ArmorType.Physical)
        {
            totalDamage = attacker.getBaseDamage() - victim.getArmor();
        }

        if (totalDamage > 0)
        {
            victim.takeDamage(totalDamage);
        }

        return totalDamage;
    }
}
