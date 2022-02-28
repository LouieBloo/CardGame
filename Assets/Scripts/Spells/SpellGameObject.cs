using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellGameObject : MonoBehaviour
{
    private Action<CreatureModification> callback;

    public CreatureModification creatureModification;
    public float delayUntilApplied;
    private AudioSource audioSource;
    private DamageDealer damageDealer;

    private PermanentCell target;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        damageDealer = GetComponent<DamageDealer>();

        StartCoroutine(waitAndFire());
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void setup(PermanentCell target, Action<CreatureModification> callback)
    {
        this.target = target;
        this.callback = callback;
    }

    IEnumerator waitAndFire()
    {
        yield return new WaitForSecondsRealtime(delayUntilApplied);
        animationFinished();
    }

    protected virtual void animationFinished()
    {
        Debug.Log("Apply spell....");
        if (callback != null)
        {
            
            callback(creatureModification);
        }

        Destroy(this.gameObject);
    }

    protected virtual void playAudio()
    {
        audioSource.Play();
    }

    public void OnParticleSystemStopped()
    {
        Debug.Log("stopped");
    }
}
