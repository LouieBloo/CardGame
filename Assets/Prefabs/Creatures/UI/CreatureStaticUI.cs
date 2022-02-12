using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureStaticUI : MonoBehaviour
{

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI amountText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void updateAll(Creature.CreatureInfo newStats)
    {
        healthText.text = newStats.health + "";
        amountText.text = newStats.amount + "";
    }
}
