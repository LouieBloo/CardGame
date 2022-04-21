using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static SpellBook;

public class SpellBookUI : MonoBehaviour
{
    public GameObject spellBookSpellUIPrefab;
    public Transform spellSpawnTransform;

    private SpellBookEntry[] spells;
    private Action<SpellBookEntry> spellCastCallback;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    //spellsInSpellbook, Spells, spellBookSpellClicked
    public void setup(NetworkList<FixedString64Bytes> spellsInSpellbook, Dictionary<string, SpellBookEntry> allGameSpells, Action<SpellBookEntry> callback)
    {
        this.spellCastCallback = callback;

        foreach(FixedString64Bytes str in spellsInSpellbook)
        {
            if (allGameSpells[str.ToString()] != null)
            {
                GameObject spellObj = Instantiate(spellBookSpellUIPrefab, spellSpawnTransform);
                spellObj.GetComponent<SpellBookSpellUI>().setup(allGameSpells[str.ToString()], spellClicked);
            }
        }
    }
    
    private void spellClicked(SpellBookEntry clickedSpell)
    {
        Debug.Log("Spell clicked: " + clickedSpell.name);
        if(spellCastCallback != null)
        {
            spellCastCallback(clickedSpell);
        }
    }
}
