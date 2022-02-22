using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAnimatorHelper : MonoBehaviour
{
    public Transform projectileSpawnPosition;
    private Action rangePrepCallback;
    public void subscribe(Action rangePrepCallback)
    {
        this.rangePrepCallback = rangePrepCallback;
    }

    public void rangePrepFinished()
    {
        if (rangePrepCallback != null)
        {
            rangePrepCallback();
        }
    }
}
