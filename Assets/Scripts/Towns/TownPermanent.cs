using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TownPermanent : NetworkBehaviour, NetworkLoadable, TurnNotifiable
{
    string townPermanentName;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setSpawnParameters(string creatureName, HexDirection orientation)
    {
        this.townPermanentName = creatureName;
    }

    public void takePriority()
    {
    }

    public void turnEnded()
    {
    }

    public void turnStarted()
    {
    }

    public bool isLoaded()
    {
        return true;
    }
}
