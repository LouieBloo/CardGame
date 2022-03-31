using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface TurnNotifiable
{
    public void turnStarted();
    public void turnEnded();

    public void takePriority();
}
