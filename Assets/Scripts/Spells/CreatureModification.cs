using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CreatureModification : MonoBehaviour
{
    public string name;

    public int damageDelta;
    public int armorDelta;
    public int speedDelta;

    public struct Modification
    {
        public string name;
        public int bonusArmorDelta;
        public string uuid;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public Modification getModification()
    {
        Modification returnValue = new Modification();
        returnValue.name = this.name;
        returnValue.uuid = System.Guid.NewGuid().ToString();

        return returnValue;
    }
}
