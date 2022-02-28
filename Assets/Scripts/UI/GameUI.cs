using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public GameObject alertTextPrefab;
    private DamageNumber alertTextDN;

    public RectTransform parentRect;

    private void Start()
    {
        alertTextDN = alertTextPrefab.GetComponent<DamageNumber>();
    }

    public void alertMessage(string message)
    {
        //Spawn new popup with a random number between 0 and 100.
        DamageNumber damageNumber = alertTextDN.Spawn(Vector3.zero, message);

        //Set the rect parent and anchored position.
        damageNumber.SetAnchoredPosition(parentRect, new Vector2(0, -125));
    }
}
