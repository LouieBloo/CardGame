using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFaceUI : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;
    public Image backgroundImage;
    public Image activeTurnImage;

    public Player player;

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
        backgroundImage.color = new Color(player.playerColor.Value.r, player.playerColor.Value.g, player.playerColor.Value.b, 200);
    }

    private void townHealthChanged(int previousValue, int newValue)
    {
        healthText.text = newValue + "";
    }

    public void setActiveTurn(bool active)
    {
        activeTurnImage.enabled = active;
    }
}
