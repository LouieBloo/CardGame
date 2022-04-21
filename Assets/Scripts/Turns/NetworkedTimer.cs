using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkedTimer : NetworkBehaviour
{
    private List<TextMeshProUGUI> subscriberTexts = new List<TextMeshProUGUI>();
    private Action callback;

    private Coroutine timerRoutine;

    void Start()
    {
        
    }

    public void start(float time, Action callback)
    {
        this.callback = callback;

        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        timerRoutine = StartCoroutine(timer(time));
    }

    IEnumerator timer(float time)
    {
        float timer = time;
        while (timer > 0)
        {
            foreach(TextMeshProUGUI text in subscriberTexts)
            {
                text.text = Mathf.CeilToInt(timer) + "";
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        timerRoutine = null;

        if(callback != null)
        {
            callback();
        }
    }

    public void subscribeToChanges(TextMeshProUGUI textToUpdate)
    {
        subscriberTexts.Add(textToUpdate);
    }
}
