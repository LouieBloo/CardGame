using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Town : NetworkBehaviour
{
    public string factionName;
    private string townName;

    public TownBuildingReference[] factionBuildings;
    private Dictionary<string, TownBuildingReference> factionBuildingMap = new Dictionary<string, TownBuildingReference>();

    private List<TownBuildingReference> builtBuildings = new List<TownBuildingReference>();

    public enum TownBuildingUpgradeTrack
    {
        Base,
        Creature1,
        Creature2,
        Creature3,
        MageGuild,
        Market
    }

    [System.Serializable]
    public class TownBuildingReference
    {
        public string name;
        public TownBuilding building;
        public TownBuildingReference upgradedVersion;
        public TownBuildingUpgradeTrack upgradeTrack;
        public int spotInUpgradeTrack;
        public int cost;
    }

    // Start is called before the first frame update
    void Awake()
    {
        //we sort these references and then assign upgradedVersion to each one so its easy to upgrade
        Dictionary<string, List<TownBuildingReference>> tempDict = new Dictionary<string, List<TownBuildingReference>>();
        foreach(TownBuildingReference tb in factionBuildings)
        {
            if (tempDict.ContainsKey(tb.upgradeTrack.ToString()))
            {
                bool didInsert = false;
                for(int x = 0; x < tempDict[tb.upgradeTrack.ToString()].Count; x++)
                {
                    if(tempDict[tb.upgradeTrack.ToString()][x].spotInUpgradeTrack > tb.spotInUpgradeTrack)
                    {
                        tempDict[tb.upgradeTrack.ToString()].Insert(x, tb);
                        didInsert = true;
                        break;
                    }
                }

                if (!didInsert)
                {
                    tempDict[tb.upgradeTrack.ToString()].Add(tb);
                }
            }
            else
            {
                tempDict[tb.upgradeTrack.ToString()] = new List<TownBuildingReference>();
                tempDict[tb.upgradeTrack.ToString()].Add(tb);
            }

            factionBuildingMap.Add(tb.name, tb);
        }

        //since they are sorted we make the x-1 townReference have a ref to x town ref
        foreach (KeyValuePair<string, List<TownBuildingReference>> entry in tempDict)
        {
            // do something with entry.Value or entry.Key
            for(int x = 1; x < entry.Value.Count; x++)
            {
                entry.Value[x - 1].upgradedVersion = entry.Value[x];
            }
        }
    }

    [ServerRpc]
    public void buildBuildingServerRpc(string buildingName)
    {
        if(builtBuildings.Find(i => i.name == buildingName) != null){
            Debug.Log("Already have that building built..");
            return;
        }
        
        //GameObject newBuilding = Instantiate(factionBuildingMap[buildingName].prefab, transform);

        //builtBuildings.Add(newBuilding);

        //buildBuildingClientRpc(buildingName);
        foreach (TownBuildingReference t in builtBuildings)
        {
            if (t.upgradeTrack == factionBuildingMap[buildingName].upgradeTrack)
            {
                t.building.gameObject.SetActive(false);
                break;
            }
        }
        factionBuildingMap[buildingName].building.gameObject.SetActive(true);
        builtBuildings.Add(factionBuildingMap[buildingName]);
    }

    [ClientRpc]
    void buildBuildingClientRpc(string buildingName)
    {
        foreach(TownBuildingReference t in builtBuildings)
        {
            if(t.upgradeTrack == factionBuildingMap[buildingName].upgradeTrack)
            {
                t.building.gameObject.SetActive(false);
                break;
            }
        }
        factionBuildingMap[buildingName].building.gameObject.SetActive(true);
        builtBuildings.Add(factionBuildingMap[buildingName]);
    }
}
