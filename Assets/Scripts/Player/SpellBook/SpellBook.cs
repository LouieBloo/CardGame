using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellBook : NetworkBehaviour
{
    public GameObject spellBookPrefab;
    public int startingMana = 10;

    private NetworkVariable<int> mana = new NetworkVariable<int>();
    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            mana.Value = startingMana;
        }
    }

    public void iconClicked()
    {
        Debug.Log("Spell book clicked...");
    }
}
