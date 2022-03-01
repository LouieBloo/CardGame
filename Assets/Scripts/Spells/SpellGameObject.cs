using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellGameObject : NetworkBehaviour
{
    private Action<CreatureModification> callback;

    public CreatureModification creatureModification;
    public float delayBeforeApplyingModification;
    public float delayBeforeStoppingAnimation;
    private AudioSource audioSource;
    private DamageDealer damageDealer;

    private PermanentCell target;
    public GameObject objectToDisableWhenAnimationDone;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        damageDealer = GetComponent<DamageDealer>();

        StartCoroutine(waitBeforeApplyingModification());
        StartCoroutine(waitBeforeStoppingAnimation());
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

    IEnumerator waitBeforeApplyingModification()
    {
        yield return new WaitForSecondsRealtime(delayBeforeApplyingModification);
        applyEffect();
    }

    IEnumerator waitBeforeStoppingAnimation()
    {
        yield return new WaitForSecondsRealtime(delayBeforeStoppingAnimation);
        animationFinished();
    }

    protected virtual void applyEffect()
    {
        if (IsServer)
        {
            Debug.Log("Apply spell");
            target.getAttachedPermanent().GetComponent<Modifiable>().applyModification(GetComponent<NetworkObject>());
            //callback(creatureModification);
            //GetComponent<NetworkObject>().Despawn(true);
        }
    }

    protected virtual void animationFinished()
    {
        if (objectToDisableWhenAnimationDone != null)
        {
            objectToDisableWhenAnimationDone.SetActive(false);
        }
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
