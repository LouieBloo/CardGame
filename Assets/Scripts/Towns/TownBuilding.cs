using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TownBuilding : NetworkBehaviour
{
    public string humanReadableName;
    // Start is called before the first frame update
    void Start()
    {
        PlayerTurnManager.NewWeekStarted += newWeekStarted;   
    }

    private void OnDestroy()
    {
        PlayerTurnManager.NewWeekStarted -= newWeekStarted;
    }

    private void newWeekStarted(object sender, EventArgs e)
    {
        if (gameObject.activeInHierarchy && IsServer)
        {
        }
    }
}
