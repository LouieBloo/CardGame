using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI dayText;

    public GameObject playerFaceUIPrefab;
    public Transform playerFaceSpawnTransform;

    private Player myPlayer;
    private List<PlayerFaceUI> playerFaces = new List<PlayerFaceUI>();

    public Image[] roundImageBorders;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setup(Player player)
    {
        //wont work as the onvalue changed wont fire and the gold.value isnt there yet
        myPlayer = player;
        myPlayer.GetComponent<PlayerStats>().gold.OnValueChanged += goldUpdated;
        myPlayer.GetComponent<PlayerStats>().mana.OnValueChanged += manaUpdated;
    }

    private void goldUpdated(int previousValue, int newValue)
    {
        goldText.text = newValue + "";
    }

    private void manaUpdated(int previousValue, int newValue)
    {
        manaText.text = newValue + "";
    }

    public void spellBookClicked()
    {
        myPlayer.GetComponent<SpellBook>().iconClicked();
    }

    public void createPlayerFaces(List<Player> players)
    {
        foreach(Player p in players)
        {
            GameObject g = Instantiate(playerFaceUIPrefab, playerFaceSpawnTransform);
            g.GetComponent<PlayerFaceUI>().setup(p);
            playerFaces.Add(g.GetComponent<PlayerFaceUI>());
        }
        
    }

    public void selectActivePlayerFace(ulong playerId)
    {
        foreach(PlayerFaceUI face in playerFaces)
        {
            if(face.player.OwnerClientId == playerId)
            {
                face.setActiveTurn(true);
            }
            else
            {
                face.setActiveTurn(false);
            }
        }
    }

    public void changeRoundUIUpdate(int roundNumber,int dayNumber)
    {
        dayText.text = "Day " + dayNumber;
        for(int x = 0; x < roundImageBorders.Length; x++)
        {
            if(x == roundNumber)
            {
                roundImageBorders[x].enabled = true;
            }
            else
            {
                roundImageBorders[x].enabled = false;
            }
        }
    }
}
