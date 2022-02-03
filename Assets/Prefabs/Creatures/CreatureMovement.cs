using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CreatureMovement : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void moveToPointFromPath(List<Vector3> path)
    {
        if (IsOwner)
        {
            moveToPointServerRpc(path.ToArray());
        }
    }

    [ServerRpc]
    void moveToPointServerRpc(Vector3[] route)
    {
        //StartCoroutine(moveToPoint(route));

        Debug.Log("DOING ROUTE: " + route.Length);
    }

    IEnumerator moveToPoint(List<Vector3> route)
    {
        yield return null;
    }
}
