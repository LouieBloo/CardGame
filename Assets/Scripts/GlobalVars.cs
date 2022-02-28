using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVars : MonoBehaviour
{

    public static GlobalVariables gv;
    // Start is called before the first frame update
    void Start()
    {
        gv = GetComponent<GlobalVariables>();
    }
}
