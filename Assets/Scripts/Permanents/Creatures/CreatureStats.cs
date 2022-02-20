using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureStats : MonoBehaviour
{
    public DamageDealer.DamageType damageType;
    public DamageDealer.PhysicalType physicalDamageType;
    public int baseDamage;
    public int currentDamage;

    public int baseHealth;
    public int currentHealth;
    public DamageTaker.ArmorType armorType;
    public int baseArmor;
    public int currentArmor;

    public int baseSpeed;
    public int currentSpeed;

    public string name;

    public enum CreatureHexSpaces
    {
        Point,
        Line
    }

    public CreatureHexSpaces hexSpaces = CreatureHexSpaces.Point;
    public int hexSpaceDistance = 0;
}
