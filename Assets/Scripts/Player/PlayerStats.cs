using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public int startingGold;
    public int startingMana;
    public NetworkVariable<int> gold = new NetworkVariable<int>();
    public NetworkVariable<int> mana = new NetworkVariable<int>();

    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            gold.Value = startingGold;
            mana.Value = startingMana;
        }
    }

    public void modifyGold(int amount)
    {
        if (!IsServer) { return; }
        gold.Value += amount;
        if(gold.Value < 0)
        {
            gold.Value = 0;
        }
    }

    [ServerRpc]
    public void modifyManaServerRpc(int amount)
    {
        mana.Value += amount;
        if (mana.Value < 0)
        {
            mana.Value = 0;
        }
    }
}
