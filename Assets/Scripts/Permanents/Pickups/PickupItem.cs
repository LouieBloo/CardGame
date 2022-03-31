using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PickupItem : NetworkBehaviour
{
    Action killCallback;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public virtual void playerPickedUp(ulong clientId)
    {

    }

    public void registerkillCallback(Action callback)
    {
        this.killCallback = callback;
    }

    public virtual void kill()
    {
        if (killCallback != null)
        {
            killCallback();
        }
    }
}
