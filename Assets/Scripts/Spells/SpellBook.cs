using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpellBook : NetworkBehaviour
{
    public int startingMana = 10;

    public SpellBookEntry[] Spells;

    public GameObject spellBookUIPrefab;
    private GameObject activeSpellBook;

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
        public Sprite spellBookImage;
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
            spellGameobject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
            spellGameobject.transform.SetParent(transform);
            spellGameobject.GetComponent<SpellGameObject>().setup(grid.getPermanentCellAtPosition(target), null);

            wipeObjectSelectorClientRpc();
        }
        else
        {
            GlobalVars.gv.gameUI.alertMessage("You dont have this spell");
        }
    }

    [ClientRpc]
    void wipeObjectSelectorClientRpc()
    {
        if (IsOwner)
        {
            objectSelector.commandSuccessFullReset();
        }
    }

    /*public CreatureModification getModification(string name)
    {
        foreach(SpellBookEntry se in Spells)
        {
            if(se.name == name)
            {
                return se.spellPrefab.GetComponent<CreatureModification>();
            }
        }
        return null;
    }*/

    private void openSpellBook()
    {
        if(activeSpellBook == null)
        {
            activeSpellBook = Instantiate(spellBookUIPrefab, Vector3.zero, Quaternion.identity);
            activeSpellBook.GetComponent<SpellBookUI>().setup(spellsInSpellbook, Spells);
        }
        else
        {
            Destroy(activeSpellBook);
            activeSpellBook = null;
        }
    }

    public void iconClicked()
    {
        openSpellBook();
    }
}
