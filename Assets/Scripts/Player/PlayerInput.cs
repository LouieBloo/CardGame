using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    public bool respondingToInput = true;

    private CameraTracker cameraTracker;
    private SpellBook spellBook;

    // Start is called before the first frame update
    void Start()
    {
        
        spellBook = GetComponent<SpellBook>();
        if (IsOwner)
        {
            StartCoroutine(pollForInput());
            cameraTracker = Camera.main.GetComponent<CameraTracker>();
        }
    }

    // Update is called once per frame
    IEnumerator pollForInput()
    {
        while (respondingToInput)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                spellBook.iconClicked();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //spellBook.spellActivated(0);
            }

            if (Input.GetKey(KeyCode.A))
            {
                cameraTracker.leftPressed();
            }
            if (Input.GetKey(KeyCode.D))
            {
                cameraTracker.rightPressed();
            }
            if (Input.GetKey(KeyCode.W))
            {
                cameraTracker.forwardPressed();
            }
            if (Input.GetKey(KeyCode.S))
            {
                cameraTracker.backPressed();
            }

            if(Input.mouseScrollDelta.y != 0)
            {
                cameraTracker.zoom(Input.mouseScrollDelta.y);
            }

            yield return null;
        }
    }
}
