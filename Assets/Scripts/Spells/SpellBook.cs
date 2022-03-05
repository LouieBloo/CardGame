using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpellBook : NetworkBehaviour
{
    public int startingMana = 10;

    public SpellBookEntry[] Spells;
    private Dictionary<string, SpellBookEntry> allGameSpells = new Dictionary<string, SpellBookEntry>();

    public GameObject spellBookUIPrefab;
    private GameObject activeSpellBook;

    private NetworkList<FixedString64Bytes> spellsInSpellbook;
    private NetworkVariable<int> mana = new NetworkVariable<int>();
    private ObjectSelecting objectSelector;

    private string currentlyCastingSpellId;
    private Grid grid;

    [System.Serializable]
    public class SpellBookEntry
    {
        public GameObject spellPrefab;
        public string name;
        public int mana;
        public Sprite spellBookImage;
    }

    private void Awake()
    {
        if (IsServer)
        {
            spellsInSpellbook = new NetworkList<FixedString64Bytes>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach(SpellBookEntry s in Spells)
        {
            allGameSpells.Add(s.name, s);
        }

        if (IsServer)
        {
            mana.Value = startingMana;

            spellsInSpellbook.Add("Frost Shield");
            spellsInSpellbook.Add("Lightning Bolt");
        }

        objectSelector = GameObject.FindGameObjectsWithTag("Game")[0].GetComponent<ObjectSelecting>();
        grid = GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>();
    }

    private void spellActivated(SpellBookEntry spell)
    {
        currentlyCastingSpellId = spell.name;
        objectSelector.findTarget(targetFound, allGameSpells[spell.name].spellPrefab.GetComponent<Selectable>());
        toggleSpellBook();//close the book
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
    public void castSpellServerRpc(string spellId,Vector3 target)
    {
        if (spellsInSpellbook.Contains(spellId) && mana.Value >= allGameSpells[spellId].mana)
        {
            GameObject spellGameobject = Instantiate(allGameSpells[spellId].spellPrefab, target, Quaternion.identity);
            if(spellGameobject.GetComponent<NetworkObject>() != null)
            {
                spellGameobject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
            }
            else
            {
                spawnSpellClientRpc(spellId, target);
            }
            
            spellGameobject.transform.SetParent(transform);
            spellGameobject.GetComponent<SpellGameObject>().setup(grid.getPermanentCellAtPosition(target), null);

            wipeObjectSelectorClientRpc();
        }
        else
        {
            GlobalVars.gv.gameUI.alertMessage("You dont have this spell");
        }
    }


    //some spells we need to network track such as buffs, others we dont such as damage dealers. The client just needs to render the animation + sound
    [ClientRpc]
    void spawnSpellClientRpc(string spellId, Vector3 target)
    {
        if (IsServer) { return; }

        GameObject spellGameobject = Instantiate(allGameSpells[spellId].spellPrefab, target, Quaternion.identity);
        spellGameobject.transform.SetParent(transform);
        spellGameobject.GetComponent<SpellGameObject>().setup(grid.getPermanentCellAtPosition(target), null);
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

    private void toggleSpellBook()
    {
        if(activeSpellBook == null)
        {
            objectSelector.stopPolling();
            activeSpellBook = Instantiate(spellBookUIPrefab, Vector3.zero, Quaternion.identity);
            activeSpellBook.GetComponent<SpellBookUI>().setup(spellsInSpellbook, allGameSpells, spellActivated);
        }
        else
        {
            objectSelector.startPolling();
            Destroy(activeSpellBook);
            activeSpellBook = null;
        }
    }

    public void iconClicked()
    {
        toggleSpellBook();
    }
}
