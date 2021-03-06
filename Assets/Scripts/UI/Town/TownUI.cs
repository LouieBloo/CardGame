using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownUI : MonoBehaviour
{
    private Town town;
    private TownManager townManager;
    public GameObject townUIButtonPrefab;
    public Transform buttonSpawnLocation;

    private List<TownUIButton> allButtons = new List<TownUIButton>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            closeButtonPressed();
        }
    }

    public void setup(Town town,TownManager townManager)
    {
        this.town = town;
        this.townManager = townManager;
        this.town.subscribeToBuiltBuildingCallback(buildingBuilt);
        int index = 0;
        foreach (KeyValuePair<string, List<Town.TownBuildingReference>> entry in town.getFactionBuildingByUpgradeTrack())
        {
            GameObject uiButton;
            if (allButtons.Count > index)
            {
                uiButton = allButtons[index].gameObject;
            }
            else
            {
                uiButton = Instantiate(townUIButtonPrefab, buttonSpawnLocation);
                allButtons.Add(uiButton.GetComponent<TownUIButton>());
            }
            
            Town.TownBuildingReference referenceForButton = entry.Value[0];

            //check what building reference we should give to this button, if we dont have one already built use the first in the list
            foreach (Town.TownBuildingReference t in town.getBuiltBuildings())
            {
                // do something with entry.Value or entry.Key
                if (t.upgradeTrack.ToString() == entry.Key)
                {
                    //find the reference
                    referenceForButton = t.upgradedVersion;
                    break;
                }
            }

            uiButton.GetComponent<TownUIButton>().setup(referenceForButton, buildingClicked);

            index++;
        }
    }

    private void buildingBuilt()
    {
        setup(this.town,this.townManager);
    }

    private void buildingClicked(Town.TownBuildingReference obj)
    {
        town.buildBuildingServerRpc(obj.name);
    }

    public void closeButtonPressed()
    {
        townManager.townButtonPressed();
        Camera.main.GetComponent<CameraTracker>().zoomToTownToggle(null);
    }
}
