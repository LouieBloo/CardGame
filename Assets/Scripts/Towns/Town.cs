using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Town : PlayerOwnedNetworkObject
{
    public string factionName;
    private string townName;

    public TownBuildingReference[] factionBuildings;
    private Dictionary<string, TownBuildingReference> factionBuildingMap = new Dictionary<string, TownBuildingReference>();
    private Dictionary<string, List<TownBuildingReference>> factionBuildingByUpgradeTrack;

    private List<TownBuildingReference> builtBuildings = new List<TownBuildingReference>();

    private Action buildingBuiltCallback;

    public TownCreature[] townCreatures;

    private Level.PlayerStart playerStart;

    [SerializeField] public NetworkVariable<int> health = new NetworkVariable<int>();

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
        public Sprite uiImage;
        public int spotInUpgradeTrack;
        public int cost;
    }

    [System.Serializable]
    public class TownCreature
    {
        public string name;
        public TownBuildingUpgradeTrack upgradeTrack;
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (IsServer)
        {
            health.Value = 100;
        }

        //we sort these references and then assign upgradedVersion to each one so its easy to upgrade
        factionBuildingByUpgradeTrack = new Dictionary<string, List<TownBuildingReference>>();
        foreach(TownBuildingReference tb in factionBuildings)
        {
            if (factionBuildingByUpgradeTrack.ContainsKey(tb.upgradeTrack.ToString()))
            {
                bool didInsert = false;
                for(int x = 0; x < factionBuildingByUpgradeTrack[tb.upgradeTrack.ToString()].Count; x++)
                {
                    if(factionBuildingByUpgradeTrack[tb.upgradeTrack.ToString()][x].spotInUpgradeTrack > tb.spotInUpgradeTrack)
                    {
                        factionBuildingByUpgradeTrack[tb.upgradeTrack.ToString()].Insert(x, tb);
                        didInsert = true;
                        break;
                    }
                }

                if (!didInsert)
                {
                    factionBuildingByUpgradeTrack[tb.upgradeTrack.ToString()].Add(tb);
                }
            }
            else
            {
                factionBuildingByUpgradeTrack[tb.upgradeTrack.ToString()] = new List<TownBuildingReference>();
                factionBuildingByUpgradeTrack[tb.upgradeTrack.ToString()].Add(tb);
            }

            factionBuildingMap.Add(tb.name, tb);
        }

        //since they are sorted we make the x-1 townReference have a ref to x town ref
        foreach (KeyValuePair<string, List<TownBuildingReference>> entry in factionBuildingByUpgradeTrack)
        {
            // do something with entry.Value or entry.Key
            for(int x = 1; x < entry.Value.Count; x++)
            {
                entry.Value[x - 1].upgradedVersion = entry.Value[x];
            }
        }


        builtBuildings.Add(factionBuildingMap["TOWN_BASE"]);
    }

    public void takeDamage(int damage)
    {
        this.health.Value -= damage;
    }

    public void subscribeToBuiltBuildingCallback(Action builtBuildingCallback)
    {
        this.buildingBuiltCallback = builtBuildingCallback;
    }

    public void startGame(Level.PlayerStart playerStart)
    {
        this.playerStart = playerStart;

        GlobalVars.gv.grid.createCreatureOnCell(new HexCoordinates[] { GlobalVars.gv.grid.getHexCoordinatesFromPosition(playerStart.spawnCells[0].transform.position) }, OwnerClientId, townCreatures[0].name);
        GlobalVars.gv.grid.createCreatureOnCell(new HexCoordinates[] { GlobalVars.gv.grid.getHexCoordinatesFromPosition(playerStart.spawnCells[1].transform.position) }, OwnerClientId, townCreatures[1].name);
    }

    [ServerRpc]
    public void buildBuildingServerRpc(string buildingName)
    {
        if(builtBuildings.Find(i => i.name == buildingName) != null){
            sendPlayerErrorClientRpc("Already have that building built..");
            return;
        }
        if (factionBuildingMap[buildingName].cost > getPlayer().playerStats.gold.Value)
        {
            sendPlayerErrorClientRpc("Not enough gold!");
            return;
        }

        getPlayer().playerStats.modifyGold(-factionBuildingMap[buildingName].cost);

        buildBuildingClientRpc(buildingName);
    }

    [ClientRpc]
    void buildBuildingClientRpc(string buildingName)
    {
        foreach(TownBuildingReference t in builtBuildings)
        {
            if(t.upgradeTrack == factionBuildingMap[buildingName].upgradeTrack)
            {
                t.building.gameObject.SetActive(false);
                builtBuildings.Remove(t);
                break;
            }
        }
        factionBuildingMap[buildingName].building.gameObject.SetActive(true);
        
        builtBuildings.Add(factionBuildingMap[buildingName]);

        //if we are showing the town building ui we need to update it
        if(this.buildingBuiltCallback != null)
        {
            this.buildingBuiltCallback();
        }
    }

    public List<TownBuildingReference> getBuiltBuildings()
    {
        return this.builtBuildings;
    }

    public Dictionary<string, List<TownBuildingReference>> getFactionBuildingByUpgradeTrack()
    {
        return this.factionBuildingByUpgradeTrack;
    }
}
