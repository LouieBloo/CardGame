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
    private Vector3 leftMoveOffset = new Vector3(-1,0,0);
    private Vector3 rightMoveOffset = new Vector3(1, 0, 0);
    private Vector3 forwardMoveOffset = new Vector3(0, 0, 1);
    private Vector3 backMoveOffset = new Vector3(0, 0, -1);


    private Coroutine trackObjectEnumerator;

    private void Start()
    {
        trackingOffset = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    public void trackTarget(Transform target)
    {
        trackedObject = target;

        if (trackObjectEnumerator != null)
        {
            StopCoroutine(trackObjectEnumerator);
        }

        trackObjectEnumerator = StartCoroutine(trackTargetRoutine());
    }

    public void stopTargeting()
    {
        if (trackObjectEnumerator != null)
        {
            StopCoroutine(trackObjectEnumerator);
            trackObjectEnumerator = null;
            trackedObject = null;
        }
    }

    IEnumerator trackTargetRoutine()
    {
        float i = 0;
        float rate = 1f / trackSpeed;
        while(i < 1.0)
        {
            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(transform.position, trackedObject.position + trackingOffset, i);
            yield return null;
        }

        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, trackedObject.position + trackingOffset, trackSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void zoom(float amount)
    {
        float delta = amount * zoomSpeed;
        trackingOffset.y += delta;
        if (!trackedObject)
        {
            transform.position = new Vector3(transform.position.x, trackingOffset.y, transform.position.z);
        }
        //transform.position
    }

    public void leftPressed()
    {
        
        if (!trackedObject)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + leftMoveOffset, playerInputMoveSpeed * Time.deltaTime);
        }
        else { 
            stopTargeting(); 
        }
    }

    public void rightPressed()
    {
        if (!trackedObject)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + rightMoveOffset, playerInputMoveSpeed * Time.deltaTime);
        }
        else
        {
            stopTargeting();
        }
    }

    public void forwardPressed()
    {
        if (!trackedObject)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + forwardMoveOffset, playerInputMoveSpeed * Time.deltaTime);
        }
        else
        {
            stopTargeting();
        }
    }

    public void backPressed()
    {
        if (!trackedObject)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + backMoveOffset, playerInputMoveSpeed * Time.deltaTime);
        }
        else
        {
            stopTargeting();
        }
    }

}
