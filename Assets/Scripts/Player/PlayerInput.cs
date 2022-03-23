using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    public bool respondingToInput = true;

    private CameraTracker cameraTracker;
    private SpellBook spellBook;
    private PlayerTurnManager turnManager;

    // Start is called before the first frame update
    void Start()
    {
        spellBook = GetComponent<SpellBook>();
        if (IsOwner)
        {
            StartCoroutine(pollForInput());
            cameraTracker = Camera.main.GetComponent<CameraTracker>();

            turnManager = GlobalVars.gv.turnManager;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            respondingToInput = !respondingToInput;
        }
    }

    // Update is called once per frame
    IEnumerator pollForInput()
    {
        while (respondingToInput)
        {
            Vector3 mousePosition = Input.mousePosition;

            if (Input.GetKeyDown(KeyCode.B))
            {
                spellBook.iconClicked();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //spellBook.spellActivated(0);
            }

            //seperating keyboard press from mouse edge
            if (Input.GetKey(KeyCode.A))
            {
                cameraTracker.leftPressed();
            }
            if (mousePosition.x <= 10)
            {
                //cameraTracker.leftPressed(true);
            }


            if (Input.GetKey(KeyCode.D))
            {
                cameraTracker.rightPressed();
            }
            if (mousePosition.x >= Screen.width - 10)
            {
                //cameraTracker.rightPressed(true);
            }


            if (Input.GetKey(KeyCode.W))
            {
                cameraTracker.forwardPressed();
            }
            if (mousePosition.y >= Screen.height - 10)
            {
                //cameraTracker.forwardPressed(true);
            }


            if (Input.GetKey(KeyCode.S))
            {
                cameraTracker.backPressed();
            }
            if (mousePosition.y <= 10)
            {
                //cameraTracker.backPressed(true);
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                cameraTracker.zoom(Input.mouseScrollDelta.y);
            }

            if (Input.GetKeyDown(KeyCode.Space) && turnManager.getActiveObject() != null)
            {
                cameraTracker.trackTarget(turnManager.getActiveObject().transform,true);
            }

            yield return null;
        }
    }
}
