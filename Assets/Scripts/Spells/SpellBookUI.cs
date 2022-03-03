using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static SpellBook;

public class SpellBookUI : MonoBehaviour
{
    public GameObject spellBookSpellUIPrefab;
    public Transform spellSpawnTransform;

    private SpellBookEntry[] spells;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setup(NetworkList<int> listOfSpells, SpellBookEntry[] spells)
    {
        this.spells = spells;

        foreach(SpellBookEntry s in spells)
        {
            GameObject spellObj = Instantiate(spellBookSpellUIPrefab, spellSpawnTransform);
            spellObj.GetComponent<SpellBookSpellUI>().setup(s,spellClicked);
        }
    }

    private void spellClicked(SpellBookEntry clickedSpell)
    {
        Debug.Log("Spell clicked: " + clickedSpell.name);
    }
}
