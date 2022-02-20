using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSelectable : Selectable
{

    private CreatureMovement movementScript;

    public Texture2D westMoveTexture;
    public Texture2D northWestMoveTexture;
    public Texture2D northEastMoveTexture;
    public Texture2D eastMoveTexture;
    public Texture2D southEastMoveTexture;
    public Texture2D southWestMoveTexture;
    private Dictionary<HexDirection, Texture2D> mouseTextureMoveDirectionMapping = new Dictionary<HexDirection, Texture2D>();

    public Texture2D westAttackTexture;
    public Texture2D northWestAttackTexture;
    public Texture2D northEastAttackTexture;
    public Texture2D eastAttackTexture;
    public Texture2D southEastAttackTexture;
    public Texture2D southWestAttackTexture;
    private Dictionary<HexDirection, Texture2D> mouseTextureDirectionMapping = new Dictionary<HexDirection, Texture2D>();

    public GameObject statsUIPrefab;
    private GameObject activeStatsUI;

    private void Awake()
    {
        movementScript = this.GetComponent<CreatureMovement>();
        type = SelectableType.Creature;
    }

    private void Start()
    {
        mouseTextureMoveDirectionMapping.Add(HexDirection.W, westMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.NW, northWestMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.NE, northEastMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.E, eastMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.SE, southEastMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.SW, southWestMoveTexture);

        mouseTextureDirectionMapping.Add(HexDirection.W, westAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.NW, northWestAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.NE, northEastAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.E, eastAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.SE, southEastAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.SW, southWestAttackTexture);
    }

    public override bool commandIssuedToCell(PermanentCell target, List<PermanentCell> extraHoveringCells, HexDirection orientation, HexDirection mouseOrientation)
    {
        //if(doIHaveTurnPriority)

        if (!IsOwner) { return false; }
        if (target.hasPermanent() && extraHoveringCells.Count > 0)
        {
            Debug.Log("SHOULD ATTACK");
            movementScript.moveToCellAndAttack(target, extraHoveringCells, orientation, mouseOrientation);
        }
        else
        {
            movementScript.moveToCell(target, extraHoveringCells, orientation);
        }

        return false;
    }


    //When we are selected and the mouse is hovering over another selectable
    public override OnHoverOverSelectableResponse onMouseHoverEnter(Selectable selectableMouseIsHoveringOn, HexDirection orientation, HexDirection mouseOrientation)
    {
        //check for valid move if we are hovering over a cell
        if(selectableMouseIsHoveringOn.Type == SelectableType.HexCell && !selectableMouseIsHoveringOn.GetComponent<PermanentCell>().hasPermanent())
        {
            if (movementScript.getHexSpaceType() == CreatureStats.CreatureHexSpaces.Point)
            {
                return new OnHoverOverSelectableResponse(mouseTextureMoveDirectionMapping[orientation], new SelectableHexArea(SelectableHexAreaType.Point, 0,orientation));
            }
            else if (movementScript.getHexSpaceType() == CreatureStats.CreatureHexSpaces.Line)
            {
                return new OnHoverOverSelectableResponse(mouseTextureMoveDirectionMapping[orientation], new SelectableHexArea(SelectableHexAreaType.Line, movementScript.hexSpaceDistance.Value,orientation));
            }
        }
        else if (selectableMouseIsHoveringOn.Type == SelectableType.Creature)
        {
            if(movementScript.getHexSpaceType() == CreatureStats.CreatureHexSpaces.Point)
            {
                return new OnHoverOverSelectableResponse(mouseTextureDirectionMapping[mouseOrientation], new SelectableHexArea(SelectableHexAreaType.Line, 1,CellHelper.getOppositeOfDirection(mouseOrientation)));
            }else if (movementScript.getHexSpaceType() == CreatureStats.CreatureHexSpaces.Line)
            {
                //note the +1 to distance since we start counting from the target, not the move position
                return new OnHoverOverSelectableResponse(mouseTextureDirectionMapping[mouseOrientation], new SelectableHexArea(SelectableHexAreaType.Line, movementScript.hexSpaceDistance.Value+1, CellHelper.getOppositeOfDirection(mouseOrientation)));
            }
        }

        return null;
    }

    public override HexDirection getOrientation()
    {
        return movementScript.getOrientation();
    }

    public override void onAltClick(Vector3 mousePosition)
    {
        CreatureStats statsForUI = new CreatureStats();
        activeStatsUI = Instantiate(statsUIPrefab, Vector3.zero, Quaternion.identity);
        activeStatsUI.GetComponent<CreatureStatsUI>().setup(GetComponent<Creature>().getCurrentStats());
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(activeStatsUI.GetComponent<RectTransform>(), mousePosition, null, out Vector3 worldPosition))
        {
            activeStatsUI.transform.GetChild(0).position = new Vector3(worldPosition.x + 200, worldPosition.y + 112, worldPosition.z);
        }
    }

    public override void onAltClickRelease()
    {
        if(activeStatsUI != null)
        {
            Destroy(activeStatsUI);
            activeStatsUI = null;
        }
    }
}
