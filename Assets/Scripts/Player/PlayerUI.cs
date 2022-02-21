using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    private Player myPlayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setup(Player player)
    {
        myPlayer = player;
    }

    public void spellBookClicked()
    {
        myPlayer.GetComponent<SpellBook>().iconClicked();
    }
}
