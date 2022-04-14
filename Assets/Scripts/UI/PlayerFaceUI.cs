using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerFaceUI : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;

    private Player player;

    void Start()
    {
        
    }

    public void setup(Player player)
    {
        this.player = player;
        player.townManager.getTown().health.OnValueChanged += townHealthChanged;

        StartCoroutine(waitUntilPlayerLoads());
    }

    IEnumerator waitUntilPlayerLoads()
    {
        while (!player.isLoaded())
        {
            yield return null;
        }

        townHealthChanged(0, player.townManager.getTown().health.Value);
        nameText.text = player.getName();
    }

    private void townHealthChanged(int previousValue, int newValue)
    {
        healthText.text = newValue + "";
    }
}
