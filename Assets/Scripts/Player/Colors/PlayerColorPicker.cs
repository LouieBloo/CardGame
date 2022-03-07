using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorPicker : MonoBehaviour
{
    
    public GameObject colorSwabUIPrefab;
    public PlayerDefaults playerDefault;
    public Transform colorSwabSpawnTransform;
    public Color[] allColors;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Color c in allColors)
        {
            GameObject swab = Instantiate(colorSwabUIPrefab, colorSwabSpawnTransform);
            swab.GetComponent<PlayerColorSwab>().setup(c,swabClicked);
        }
    }

    private void swabClicked(Color c)
    {
        playerDefault.setColor(c);
    }
}
