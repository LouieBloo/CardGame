using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TurnIndicatorUI : MonoBehaviour
{
    public Image colorImage;
    public Image objectImage;

    public void setup(Creature creature)
    {
        StartCoroutine(waitForObjectToLoad(creature));
    }

    IEnumerator waitForObjectToLoad(Creature creature)
    {
        while (true)
        {
            if(creature.isLoaded())
            {
                break;
            }
            yield return null;
        }

        colorImage.color = creature.getCurrentStats().color;
        objectImage.sprite = creature.getCurrentStats().uiImage;
    }

}
