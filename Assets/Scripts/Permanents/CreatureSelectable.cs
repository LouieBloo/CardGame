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


    public override void commandIssuedToCell(PermanentCell target,Grid grid)
    {
        if (!IsOwner) { return; }
        if (target.hasPermanent())
        {

        }
        else
        {
            movementScript.moveToPointFromPath(target);
        }
    }
}
