using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CreatureMovement : NetworkBehaviour
{
    public float movementSpeed;
    public float rotationSpeed;

    private Grid grid;

    IEnumerator doingCommand;

    public NetworkVariable<FixedString64Bytes> facingOrientation = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<FixedString64Bytes> hexSpaceType = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<int> hexSpaceDistance = new NetworkVariable<int>();
    public NetworkVariable<int> speed = new NetworkVariable<int>();

    private void Start()
    {
        if (!grid)
        {
            grid = GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>();
        }
    }

    public void setup(HexDirection startingOrientation)
    {
        facingOrientation.Value = startingOrientation.ToString();
        rotateToOrientation(facingOrientation.Value.ToString());
    }

    public HexCoordinates currentCoordinates()
    {
        return grid.getHexCoordinatesFromPosition(transform.position);
    }

    private void rotateToOrientation(string orientation)
    {
        transform.eulerAngles = new Vector3(transform.rotation.x, 0, transform.rotation.z);
        transform.Rotate(0, CellHelper.rotateAmountFromDirection(orientation), 0);
    }

    public HexDirection getOrientation()
    {
        return CellHelper.getDirectionFromString(facingOrientation.Value.ToString());
    }

    public CreatureStats.CreatureHexSpaces getHexSpaceType()
    {
        CreatureStats.CreatureHexSpaces.TryParse(hexSpaceType.Value.ToString(), out CreatureStats.CreatureHexSpaces result);
        return result;
    }

    public void moveToCell(PermanentCell target, List<PermanentCell> extraHoveringCells, HexDirection orientation)
    {
        if (IsServer)
        {
            moveAndExecuteAction(target, extraHoveringCells, null,Creature.CreatureActions.Move, orientation.ToString(),null);
        }
    }

    public void moveToCellAndAttack(PermanentCell target, List<PermanentCell> extraHoveringCells, HexDirection orientation, HexDirection mouseOrientation, Action<PermanentCell> callbackWhenDone)
    {
        if (IsServer)
        {
            moveAndExecuteAction(extraHoveringCells[0], extraHoveringCells, target, Creature.CreatureActions.Attack, orientation.ToString(),callbackWhenDone);
        }
    }


    void moveAndExecuteAction(PermanentCell targetMoveCell, List<PermanentCell> extraMovePositions, PermanentCell targetActionCell, Creature.CreatureActions action, string finalOrientation, Action<PermanentCell> callbackWhenDone)
    {
        if (doingCommand != null) { Debug.Log("Already doing a command...");return; }


        //make sure there is nothing in this cell
        if (!targetMoveCell || (targetMoveCell.hasPermanent() && targetMoveCell.getAttachedPermanent() != GetComponent<Permanent>())) { Debug.Log("Invalid attack, either no target or target has a permanent"); return; }

        //same checks for extra cells
        foreach(PermanentCell cell in extraMovePositions)
        {
            if (!cell) { Debug.Log("Line cell extra not real!"); return; }
            if (cell.hasPermanent() && cell.getAttachedPermanent() != GetComponent<Permanent>()) { Debug.Log("Line cell extra has permanent!"); return; }
        }

        //find path
        List<Vector3> path = grid.findPathVector3(grid.getHexCoordinatesFromPosition(transform.position), targetMoveCell.getHexCoordinates());
        if (path != null)
        {
            doingCommand = moveToPointThenExecuteAction(path.ToArray(), targetActionCell, action,finalOrientation, extraMovePositions,callbackWhenDone);
            StartCoroutine(doingCommand);
        }
    }

    IEnumerator moveToPointThenExecuteAction(Vector3[] route, PermanentCell targetActionCell, Creature.CreatureActions action, string finalOrientation, List<PermanentCell> extraMoveCells, Action<PermanentCell> callbackWhenDone)
    {
        Animator animator = GetComponent<Creature>().getCreatureObject().GetComponent<Animator>();
        if(route.Length > 0)
        {
            Vector3 startingPos = transform.position;
            int targetPointIndex = 0;

            //rotate towards first target
            yield return StartCoroutine(rotateTowardsPoint(route[targetPointIndex]));

            while (targetPointIndex != route.Length)
            {
                animator.SetBool("Running", true);
                Vector3 targetPoint = route[targetPointIndex];
                // Check if the position of the cube and sphere are approximately equal.
                if (Vector3.Distance(transform.position, targetPoint) < 0.001f)
                {
                    transform.position = new Vector3(targetPoint.x,transform.position.y,targetPoint.z);
                    targetPointIndex++;
                    if (targetPointIndex != route.Length)
                    {
                        yield return StartCoroutine(rotateTowardsPoint(route[targetPointIndex]));
                    }
                }
                else
                {
                    float step = movementSpeed * Time.deltaTime; // calculate distance to move
                    transform.position = Vector3.MoveTowards(transform.position, targetPoint, step);
                }

                yield return null;
            }

            grid.permanentMovedToNewCell(GetComponent<NetworkObject>(), route[route.Length - 1],finalOrientation, extraMoveCells);
        }

        animator.SetBool("Running", false);

        //if we need to rotate towards our action target
        if (action == Creature.CreatureActions.Attack)
        {
            //rotate towards action target
            yield return StartCoroutine(rotateTowardsPoint(targetActionCell.transform.position));
        }
        else
        {
            //rotate towards final orientation
            HexCoordinates neighbor = HexUtility.GetNeighbour(grid.getHexCoordinatesFromPosition(transform.position), CellHelper.getDirectionFromString(finalOrientation));
            yield return StartCoroutine(rotateTowardsPoint(grid.getPositionFromHexCoordinates(neighbor)));
        }

        //set final orientation of our object 
        facingOrientation.Value = finalOrientation;

        if(callbackWhenDone != null)
        {
            callbackWhenDone(targetActionCell);
        }

        doingCommand = null;
    }

    void handleAction(PermanentCell targetActionCell, Creature.CreatureActions action)
    {
        if (!targetActionCell) { return; }

        GetComponent<CreatureAction>().handleAction(targetActionCell, action);
    }

    IEnumerator rotateTowardsPoint(Vector3 point)
    {
        Vector3 targetDirection = (point - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
    }
}
