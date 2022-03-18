using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{

    public NetworkVariable<int> gold = new NetworkVariable<int>();
    public NetworkVariable<int> mana = new NetworkVariable<int>();

    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            gold.Value = 200;
        }
    }

    [ServerRpc]
    public void modifyGoldServerRpc(int amount)
    {
        gold.Value += amount;
    }
}
