using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSelectable : Selectable
{
    // Start is called before the first frame update
    void Awake()
    {
        this.type = SelectableType.Pickup;
    }


}
