using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerColorSwab : MonoBehaviour
{
    Action<Color> callback;

    public void setup(Color myColor, Action<Color> callback)
    {
        GetComponent<Image>().color = myColor;
        this.callback = callback;
    }

    // Update is called once per frame
    public void buttonClick()
    {
        callback(GetComponent<Image>().color);
    }
}
