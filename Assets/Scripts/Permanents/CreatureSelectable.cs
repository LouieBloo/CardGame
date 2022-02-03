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
        if (target.hasPermanent())
        {

        }
        else
        {
            List<Vector3> path = grid.findPathVector3(grid.getHexCoordinatesFromPosition(transform.position), target.getHexCoordinates());

            if (path != null)
            {
                movementScript.moveToPointFromPath(path);
            }
        }
    }
}
