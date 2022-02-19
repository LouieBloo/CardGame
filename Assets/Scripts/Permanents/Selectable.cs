using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Selectable : NetworkBehaviour
{

    [SerializeField] protected GameObject selectedIndicatorPrefab;
    private GameObject selectedIndicator;

    private CreatureMovement creatureMovement;

    public enum SelectableType
    {
        Creature,
        HexCell,
        Spell
    }

    public enum SelectableHexAreaType
    {
        Point,
        Line,
        None
    }

    public class SelectableHexArea
    {
        public SelectableHexArea(SelectableHexAreaType type, int distance, HexDirection orientation)
        {
            this.type = type;
            this.distance = distance;
            this.orientation = orientation;
        }
        public SelectableHexAreaType type;
        public int distance;
        public HexDirection orientation;
    }

    public class OnHoverOverSelectableResponse
    {
        public OnHoverOverSelectableResponse(Texture2D texture, SelectableHexArea selectableArea)
        {
            this.texture = texture;
            this.selectableArea = selectableArea;
            
        }
        public Texture2D texture;
        public SelectableHexArea selectableArea;
    }

    protected SelectableType type;

    public SelectableType Type
    {
        get { return this.type; }
    }

    // Start is called before the first frame update
    void Start()
    {
        creatureMovement = GetComponent<CreatureMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedIndicator)
        {
            selectedIndicator.transform.position = new Vector3(transform.position.x, selectedIndicator.transform.position.y, transform.position.z);
        }
    }

    public void select()
    {
        if (!selectedIndicator)
        {
            selectedIndicator = Instantiate(selectedIndicatorPrefab, new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z), selectedIndicatorPrefab.transform.rotation);
        }
    }

    public void deselect()
    {
        if (selectedIndicator)
        {
            Destroy(selectedIndicator);
            selectedIndicator = null;
        }
    }


    //When we are selected and the mouse is hovering over another selectable
    public virtual OnHoverOverSelectableResponse onMouseHoverEnter(Selectable selectableMouseIsHoveringOn, HexDirection targetOrientation, HexDirection mouseOrientation)
    {
        return null;
    }

    public virtual HexDirection getOrientation()
    {
        return HexDirection.NONE;
    }

   /* public CreatureMovement CreatureMovement
    {
        get { return this.creatureMovement; }
    }*/

    //if we are selected and the user left clicks on another permanent
    public virtual bool commandIssuedToCell(PermanentCell target, List<PermanentCell> extraHoveringCells, HexDirection orientation, HexDirection mouseOrientation)
    {
        Debug.Log("Issueing command at cell: " + target);
        return false;
    }
}
