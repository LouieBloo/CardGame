using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    public bool respondingToInput = true;

    private CameraTracker cameraTracker;
    private SpellBook spellBook;

    private Coroutine pollingRoutine;

    // Start is called before the first frame update
    void Start()
    {
        spellBook = GetComponent<SpellBook>();
        if (IsOwner)
        {
            startRespondingToInput();
            cameraTracker = Camera.main.GetComponent<CameraTracker>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            respondingToInput = !respondingToInput;
        }
    }

    public void startRespondingToInput()
    {
        respondingToInput = true;
        if(pollingRoutine == null)
        {
            pollingRoutine = StartCoroutine(pollForInput());
        }
    }

    public void stopRespondingToInput()
    {
        respondingToInput = false;
        if (pollingRoutine != null)
        {
            StopCoroutine(pollingRoutine);
            pollingRoutine = null;
        }
    }


    // Update is called once per frame
    IEnumerator pollForInput()
    {
        yield return null;
        while (true)
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

            if (Input.GetKeyDown(KeyCode.Space) && GlobalVars.gv.turnManager.getActiveObject() != null)
            {
                cameraTracker.trackTarget(GlobalVars.gv.turnManager.getActiveObject().transform,true);
            }


            if (Input.GetKeyDown(KeyCode.T))
            {
                GetComponent<TownManager>().townButtonPressed();
                cameraTracker.zoomToTownToggle(GetComponent<Player>().townManager.getTown());
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                NetworkObject activeCreature = GlobalVars.gv.turnManager.getActiveObject();
                if(activeCreature != null)
                {
                    activeCreature.GetComponent<DamageTaker>().defendAction();
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                GlobalVars.gv.player.passPriorityServerRpc();
            }

            yield return null;
        }
    }
}
