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
            return new OnHoverOverSelectableResponse(spellCursorTexture, new SelectableHexArea(SelectableHexAreaType.Point, 0,HexDirection.NONE));
        }

        return null;
    }
}
