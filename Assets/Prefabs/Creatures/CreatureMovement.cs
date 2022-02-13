using HexMapTools;
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

    public void setup(HexDirection startingOrientation)
    {
        Debug.Log(startingOrientation);
        facingOrientation.Value = startingOrientation.ToString();
        rotateToOrientation(facingOrientation.Value.ToString());
    }

    private void rotateToOrientation(string orientation)
    {
        switch (orientation)
        {
            case "W":
                transform.Rotate(0, -90, 0);
                break;
            case "NW":
                transform.Rotate(0, -45, 0);
                break;
            case "NE":
                transform.Rotate(0, 45, 0);
                break;
            case "E":
                transform.Rotate(0, 90, 0);
                break;
            case "SE":
                transform.Rotate(0, 135, 0);
                break;
            case "SW":
                transform.Rotate(0, 225, 0);
                break;
        }
    }

    public void moveToCell(PermanentCell target)
    {
        if (IsOwner)
        {
            moveAndExecuteActionServerRpc(target.transform.position,Vector3.zero,Creature.CreatureActions.Move);
        }
    }

    public void moveToCellAndAttack(PermanentCell attackTarget, PermanentCell targetCell)
    {
        if (IsOwner)
        {
            moveAndExecuteActionServerRpc(targetCell.transform.position, attackTarget.transform.position, Creature.CreatureActions.Attack);
        }
    }


    [ServerRpc]
    void moveAndExecuteActionServerRpc(Vector3 targetMovePosition, Vector3 targetActionPosition, Creature.CreatureActions action)
    {
        if (doingCommand != null) { Debug.Log("Already doing a command...");return; }

        if (!grid)
        {
            grid = GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>();
        }

        //grab target move cell
        PermanentCell targetMoveCell = grid.cells[grid.getHexCoordinatesFromPosition(targetMovePosition)];
        //make sure there is nothing in this cell
        if (!targetMoveCell || (targetMoveCell.hasPermanent() && targetMoveCell.getAttachedPermanent().OwnerClientId != OwnerClientId)) { Debug.Log("Invalid attack, either no target or target has a permanent"); return; }
        //if we are attacking, make sure its a valid attack
        PermanentCell targetActionCell = grid.cells[grid.getHexCoordinatesFromPosition(targetActionPosition)];
        if (action == Creature.CreatureActions.Attack)
        {
            if (!targetActionCell) {  Debug.Log("Invalid attack, cant find cell"); return; }
            if (!targetActionCell.hasPermanent()) { Debug.Log("Invalid attack, target doesnt have permanent"); return; }
            if(targetActionCell.getAttachedPermanent().GetComponent<NetworkObject>().OwnerClientId == OwnerClientId) { Debug.Log("Invalid attack, target is same team as us!"); return; }
        }

        List<Vector3> path = grid.findPathVector3(grid.getHexCoordinatesFromPosition(transform.position), targetMoveCell.getHexCoordinates());
        if (path != null)
        {
            doingCommand = moveToPointThenExecuteAction(path.ToArray(), targetActionCell, action);
            StartCoroutine(doingCommand);
        }
    }

    IEnumerator moveToPointThenExecuteAction(Vector3[] route, PermanentCell targetActionCell, Creature.CreatureActions action)
    {
        Animator animator = GetComponent<Creature>().creatureObjectReference.GetComponent<Animator>();
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

            grid.permanentMovedToNewCell(startingPos, route[route.Length - 1], GetComponent<NetworkObject>());
        }

        animator.SetBool("Running", false);

        //if we need to rotate towards our action target
        if (action == Creature.CreatureActions.Attack)
        {
            //rotate towards action target
            yield return StartCoroutine(rotateTowardsPoint(targetActionCell.transform.position));
        }

        handleAction(targetActionCell, action);

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
