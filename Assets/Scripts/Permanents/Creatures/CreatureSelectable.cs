using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CreatureSelectable : Selectable
{

    private CreatureMovement movementScript;
    private Attacker attacker;

    public Texture2D westMoveTexture;
    public Texture2D northWestMoveTexture;
    public Texture2D northEastMoveTexture;
    public Texture2D eastMoveTexture;
    public Texture2D southEastMoveTexture;
    public Texture2D southWestMoveTexture;
    private Dictionary<HexDirection, Texture2D> mouseTextureMoveDirectionMapping = new Dictionary<HexDirection, Texture2D>();

    public GameObject statsUIPrefab;
    private GameObject activeStatsUI;

    private Action commandFinishedCallback;

    private void Awake()
    {
        movementScript = this.GetComponent<CreatureMovement>();
        attacker = GetComponent<Attacker>();
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
    }

    public override SelectableHexArea select()
    {
        base.select();
        return movementScript.getMovementRange();
    }

    public override bool commandIssuedToCell(PermanentCell target, List<PermanentCell> extraHoveringCells, HexDirection orientation, HexDirection mouseOrientation, Action commandFinishedCallback)
    {
        //if(doIHaveTurnPriority)
        if (!IsOwner) { return false; }

        this.commandFinishedCallback = commandFinishedCallback;

        //convert permanet cell to vector 3 for network serialization
        Vector3[] extraMovePositions = new Vector3[extraHoveringCells.Count];
        for (int x = 0; x < extraHoveringCells.Count; x++)
        {
            extraMovePositions[x] = extraHoveringCells[x].transform.position;
        }

        attacker.commandIssuedToCellServerRpc(target.transform.position, extraMovePositions, orientation, mouseOrientation);
        return true;
    }

    public void commandFinished()
    {
        if (commandFinishedCallback != null)
        {
            commandFinishedCallback();
        }
    }


    //When we are selected and the mouse is hovering over another selectable
    public override OnHoverOverSelectableResponse onMouseHoverEnter(PermanentCell cell, Selectable selectableMouseIsHoveringOn, HexDirection orientation, HexDirection mouseOrientation)
    {
        //check for valid move if we are hovering over a cell
        if(selectableMouseIsHoveringOn.Type == SelectableType.HexCell && !cell.hasPermanent())
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
            if (attacker.isValidTarget(cell.getHexCoordinates(), selectableMouseIsHoveringOn.GetComponent<NetworkObject>().OwnerClientId)){
                return attacker.mouseAttackHover(mouseOrientation);
            }

            /*if(movementScript.getHexSpaceType() == CreatureStats.CreatureHexSpaces.Point)
            {
                return new OnHoverOverSelectableResponse(mouseTextureDirectionMapping[mouseOrientation], new SelectableHexArea(SelectableHexAreaType.Line, 1,CellHelper.getOppositeOfDirection(mouseOrientation)));
            }else if (movementScript.getHexSpaceType() == CreatureStats.CreatureHexSpaces.Line)
            {
                //note the +1 to distance since we start counting from the target, not the move position
                return new OnHoverOverSelectableResponse(mouseTextureDirectionMapping[mouseOrientation], new SelectableHexArea(SelectableHexAreaType.Line, movementScript.hexSpaceDistance.Value+1, CellHelper.getOppositeOfDirection(mouseOrientation)));
            }*/
        }

        return null;
    }

    public override HexDirection getOrientation()
    {
        return movementScript.getOrientation();
    }

    public override void onAltClick(Vector3 mousePosition)
    {
        activeStatsUI = Instantiate(statsUIPrefab, Vector3.zero, Quaternion.identity);
        activeStatsUI.GetComponent<CreatureStatsUI>().setup(GetComponent<Creature>().getCurrentStats());
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(activeStatsUI.GetComponent<RectTransform>(), mousePosition, null, out Vector3 worldPosition))
        {
            //we put the ui at the mouse position but we also need to check if it will go off the screen
            Rect uiRect = activeStatsUI.transform.GetChild(0).GetComponent<RectTransform>().rect;

            float xOffset = 0;
            float yOffset = (uiRect.height/2);
            if(mousePosition.x <= (uiRect.width/2))
            {
                xOffset = (uiRect.width / 2) - mousePosition.x;
            }else if(Screen.width - (uiRect.width / 2) < mousePosition.x)
            {
                xOffset = (uiRect.width / 2) - (Screen.width - mousePosition.x);
                xOffset *= -1;
            }

            if (Screen.height - uiRect.height < mousePosition.y)
            {
                yOffset = (uiRect.height / 2) - (Screen.height - mousePosition.y);
                yOffset *= -1;
            }
            activeStatsUI.transform.GetChild(0).position = new Vector3(worldPosition.x + xOffset, worldPosition.y + yOffset, worldPosition.z);
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
