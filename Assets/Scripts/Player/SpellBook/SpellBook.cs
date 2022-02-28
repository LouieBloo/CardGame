using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellBook : NetworkBehaviour
{
    public GameObject spellBookPrefab;
    public int startingMana = 10;

    public SpellBookEntry[] Spells;


    private NetworkList<int> spellsInSpellbook;
    private NetworkVariable<int> mana = new NetworkVariable<int>();
    private ObjectSelecting objectSelector;

    private int currentlyCastingSpellId;
    private Grid grid;

    [System.Serializable]
    public class SpellBookEntry
    {
        public GameObject spellPrefab;
        public string name;
        public int mana;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            mana.Value = startingMana;

            spellsInSpellbook.Add(0);
        }

        objectSelector = GameObject.FindGameObjectsWithTag("Game")[0].GetComponent<ObjectSelecting>();
        grid = GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>();
    }

    private void Awake()
    {
        if (IsServer)
        {
            spellsInSpellbook = new NetworkList<int>();
        }
    }

    public void spellActivated(int spellId)
    {
        currentlyCastingSpellId = spellId;
        objectSelector.findTarget(targetFound, Spells[spellId].spellPrefab.GetComponent<Selectable>());
    }

    public void targetFound(PermanentCell target, List<PermanentCell> extraHoveringCells, HexDirection orientation, HexDirection mouseOrientation)
    {
        if (target.hasPermanent())
        {
            castSpellServerRpc(currentlyCastingSpellId, target.transform.position);
        }
        else
        {
            GlobalVars.gv.gameUI.alertMessage("Need to target a creature");
        }
        
    }

    [ServerRpc]
    public void castSpellServerRpc(int spellId,Vector3 target)
    {
        if (spellsInSpellbook.Contains(spellId) && mana.Value >= Spells[spellId].mana)
        {
            GameObject spellGameobject = Instantiate(Spells[spellId].spellPrefab, target, Quaternion.identity);
            spellGameobject.GetComponent<SpellGameObject>().setup(grid.getPermanentCellAtPosition(target), null);

            castSpellClientRpc(spellId, target);
        }
        else
        {
            GlobalVars.gv.gameUI.alertMessage("You dont have this spell");
        }
    }

    [ClientRpc]
    void castSpellClientRpc(int spellId, Vector3 target)
    {
        if (!IsServer)
        {
            GameObject spellGameobject = Instantiate(Spells[spellId].spellPrefab, target, Quaternion.identity);
            spellGameobject.GetComponent<SpellGameObject>().setup(grid.getPermanentCellAtPosition(target), null);
        }
    }

    public void iconClicked()
    {
        Debug.Log("Spell book clicked...");
    }
}
