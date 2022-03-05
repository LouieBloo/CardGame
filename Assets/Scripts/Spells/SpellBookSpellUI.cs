using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellBookSpellUI : MonoBehaviour
{
    public Image image;
    SpellBook.SpellBookEntry spell;

    Action<SpellBook.SpellBookEntry> clickCallback;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void clicked()
    {
        if(clickCallback != null)
        {
            clickCallback(spell);
        }
    }

    public void setup(SpellBook.SpellBookEntry spell, Action<SpellBook.SpellBookEntry> clickCallback)
    {
        this.spell = spell;
        this.image.sprite = spell.spellBookImage;
        this.clickCallback = clickCallback;
    }
}
