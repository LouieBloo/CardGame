using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDontRotate : MonoBehaviour
{
    public Transform rotateAgainst;
    public Transform myTransform;

    // Update is called once per frame
    void Update()
    {
        myTransform.eulerAngles = new Vector3(myTransform.rotation.eulerAngles.x, 0, myTransform.rotation.eulerAngles.z);
    }
}
