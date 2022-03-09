using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerDefaults : MonoBehaviour
{
    public string name = "Lukey";

    Action<Color> colorCallback;

    public void setup(Action<Color> colorCallback)
    {
        this.colorCallback = colorCallback;
    }

    public string getName()
    {
        return name;
    }

    public void setColor(Color c)
    {
        colorCallback(c);
        //Destroy(this.gameObject);
    }
}
