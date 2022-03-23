using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public Transform trackedObject;
    public float trackSpeed = 3;
    public float zoomSpeed = 0.1f;
    public Vector3 trackingOffset;

    public float playerInputMoveSpeed;
    public float playerMouseEdgeMoveSpeed;
    private Vector3 leftMoveOffset = new Vector3(-1,0,0);
    private Vector3 rightMoveOffset = new Vector3(1, 0, 0);
    private Vector3 forwardMoveOffset = new Vector3(0, 0, 1);
    private Vector3 backMoveOffset = new Vector3(0, 0, -1);

    private bool isAutoTracking = true;

    private Coroutine trackObjectEnumerator;

    private void Start()
    {
        trackingOffset = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    public void trackTarget(Transform target,bool force)
    {
        if(!isAutoTracking && !force) { return; }

        trackedObject = target;

        if (trackObjectEnumerator != null)
        {
            StopCoroutine(trackObjectEnumerator);
        }

        trackObjectEnumerator = StartCoroutine(trackTargetRoutine());

        isAutoTracking = true;
    }

    private void stopAutoTargeting()
    {
        if (trackObjectEnumerator != null)
        {
            StopCoroutine(trackObjectEnumerator);
            trackObjectEnumerator = null;
            trackedObject = null;
        }

        isAutoTracking = false;
    }

    IEnumerator trackTargetRoutine()
    {
        float i = 0;
        float rate = 1f / trackSpeed;
        while(i < 1.0 && trackedObject != null)
        {
            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(transform.position, trackedObject.position + trackingOffset, i);
            yield return null;
        }

        while (true && trackedObject != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, trackedObject.position + trackingOffset, trackSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void zoom(float amount)
    {
        float delta = amount * zoomSpeed;
        trackingOffset.y += delta + 0.04f;
        float zDelta = -delta;
        trackingOffset.z += zDelta + 0.04f;
        //trackingOffset.z += zDelta + ;
        if (!trackedObject)
        {
            transform.position = new Vector3(transform.position.x, trackingOffset.y, transform.position.z + zDelta);
        }
        //transform.position
    }

    public void leftPressed(bool isEdge = false)
    {
        
        if (!isAutoTracking)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + leftMoveOffset, (isEdge ? playerMouseEdgeMoveSpeed : playerInputMoveSpeed) * Time.deltaTime);
        }
        else {
            stopAutoTargeting(); 
        }
    }

    public void rightPressed(bool isEdge = false)
    {
        if (!isAutoTracking)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + rightMoveOffset, (isEdge ? playerMouseEdgeMoveSpeed : playerInputMoveSpeed) * Time.deltaTime);
        }
        else
        {
            stopAutoTargeting();
        }
    }

    public void forwardPressed(bool isEdge = false)
    {
        if (!isAutoTracking)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + forwardMoveOffset, (isEdge ? playerMouseEdgeMoveSpeed : playerInputMoveSpeed) * Time.deltaTime);
        }
        else
        {
            stopAutoTargeting();
        }
    }

    public void backPressed(bool isEdge = false)
    {
        if (!isAutoTracking)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + backMoveOffset, (isEdge ? playerMouseEdgeMoveSpeed : playerInputMoveSpeed) * Time.deltaTime);
        }
        else
        {
            stopAutoTargeting();
        }
    }

}
