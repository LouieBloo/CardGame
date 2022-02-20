using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreatureStatsUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI speedText;

    public void setup(CreatureStats stats)
    {
        nameText.text = stats.name;
        damageText.text = stats.currentDamage + "";
        armorText.text = stats.currentArmor + "";
        healthText.text = stats.baseHealth + "";
        speedText.text = stats.currentSpeed + "";
    }
}
