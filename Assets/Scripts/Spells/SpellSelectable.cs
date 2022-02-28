using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellSelectable : Selectable
{
    public Texture2D spellCursorTexture;
    public Texture2D noTargetCursorTexture;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public override SelectableHexArea select()
    {
        return null;
    }

    public override OnHoverOverSelectableResponse onMouseHoverEnter(PermanentCell cell, Selectable selectableMouseIsHoveringOn, HexDirection orientation, HexDirection mouseOrientation)
    {
        //check for valid move if we are hovering over a cell
        if (selectableMouseIsHoveringOn.Type == SelectableType.HexCell && !cell.hasPermanent())
        {
            return new OnHoverOverSelectableResponse(noTargetCursorTexture, new SelectableHexArea(SelectableHexAreaType.Point, 0, HexDirection.NONE));
        }
        else if (selectableMouseIsHoveringOn.Type == SelectableType.Creature)
        {
            /*if (attacker.isValidTarget(cell.getHexCoordinates(), selectableMouseIsHoveringOn.GetComponent<NetworkObject>().OwnerClientId))
            {
                return attacker.mouseAttackHover(mouseOrientation);
            }*/

            return new OnHoverOverSelectableResponse(spellCursorTexture, new SelectableHexArea(SelectableHexAreaType.Point, 0,HexDirection.NONE));

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
}
