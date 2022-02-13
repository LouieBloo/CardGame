using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSelectable : Selectable
{

    private CreatureMovement movementScript;

    private void Awake()
    {
        movementScript = this.GetComponent<CreatureMovement>();
    }


    public override void commandIssuedToCell(PermanentCell target, PermanentCell attackMoveCell, Grid grid, HexDirection orientation)
    {
        //if(doIHaveTurnPriority)

        if (!IsOwner) { return; }
        if (target.hasPermanent() && attackMoveCell)
        {
            movementScript.moveToCellAndAttack(target, attackMoveCell, orientation);
        }
        else
        {
            movementScript.moveToCell(target,orientation);
        }
    }
}
