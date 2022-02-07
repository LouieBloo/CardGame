using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CreatureMovement : NetworkBehaviour
{
    public float movementSpeed;
    public float rotationSpeed;

    private Grid grid;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void moveToPointFromPath(PermanentCell target)
    {
        if (IsOwner)
        {
            moveToPointServerRpc(target.transform.position);
        }
    }


    [ServerRpc]
    void moveToPointServerRpc(Vector3 targetPosition)
    {
        if (!grid)
        {
            grid = GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>();
        }

        PermanentCell target = grid.cells[grid.getHexCoordinatesFromPosition(targetPosition)];
        if (!target || target.hasPermanent()) { return; }

        List<Vector3> path = grid.findPathVector3(grid.getHexCoordinatesFromPosition(transform.position), target.getHexCoordinates());

        if (path != null)
        {
            StartCoroutine(moveToPoint(path.ToArray()));
        }
    }

    IEnumerator moveToPoint(Vector3[] route)
    {
        Vector3 startingPos = transform.position;
        int targetPointIndex = 0;

        //rotate towards first target
        yield return StartCoroutine(rotateTowardsPoint(route[targetPointIndex]));

        while (targetPointIndex != route.Length)
        {
            Vector3 targetPoint = route[targetPointIndex];
            // Check if the position of the cube and sphere are approximately equal.
            if (Vector3.Distance(transform.position, targetPoint) < 0.001f)
            {
                transform.position = targetPoint;
                targetPointIndex++;
                if(targetPointIndex != route.Length)
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



        grid.permanentMovedToNewCell(startingPos,route[route.Length - 1], GetComponent<NetworkObject>());
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
