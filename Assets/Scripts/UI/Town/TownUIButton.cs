using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TownUIButton : MonoBehaviour
{
    public Image buildingImage;
    public Sprite noMoreUpgradesImage;
    public TextMeshProUGUI buildingNameText;

    private Action<Town.TownBuildingReference> callback;
    private Town.TownBuildingReference buildingRef;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setup(Town.TownBuildingReference buildingRef, Action<Town.TownBuildingReference> callback)
    {
        this.buildingRef = buildingRef;
        this.callback = callback;

        if(buildingRef != null)
        {
            buildingImage.sprite = buildingRef.uiImage;
            buildingNameText.text = buildingRef.building.humanReadableName;
        }
        else
        {
            buildingImage.sprite = noMoreUpgradesImage;
            buildingNameText.text = "Fully Upgraded";
        }
    }

    public void clicked()
    {
        if(this.callback != null)
        {
            this.callback(this.buildingRef);
        }
    }
}
