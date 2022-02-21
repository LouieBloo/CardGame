using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    public bool respondingToInput = true;

    private SpellBook spellBook;
    // Start is called before the first frame update
    void Start()
    {
        spellBook = GetComponent<SpellBook>();
        if (IsOwner)
        {
            StartCoroutine(pollForInput());
        }
    }

    // Update is called once per frame
    IEnumerator pollForInput()
    {
        while (respondingToInput)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                spellBook.iconClicked();
            }
            yield return null;
        }
    }
}
