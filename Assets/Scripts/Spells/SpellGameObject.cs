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
    public bool destroyAfterAnimation;
    private AudioSource audioSource;
    private DamageDealer damageDealer;

    //note we have two targets, one is best for netwroking, the other is just a raw object for the server
    private PermanentCell target;
    private GameObject targetGameObject;
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

    public void setup(GameObject target, Action<CreatureModification> callback)
    {
        this.targetGameObject = target;
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
            if(creatureModification != null)
            {
                if(target != null)
                {
                    target.getAttachedPermanent().GetComponent<Modifiable>().applyModification(GetComponent<NetworkObject>());
                }else if(targetGameObject != null)
                {
                    targetGameObject.GetComponent<Modifiable>().applyModification(GetComponent<NetworkObject>());
                }
                
            }
            if(damageDealer != null)
            {
                target.getAttachedPermanent().GetComponent<Creature>().attacked(damageDealer,null);
            }
            //callback(creatureModification);
        }
    }

    protected virtual void animationFinished()
    {
        if (objectToDisableWhenAnimationDone != null)
        {
            objectToDisableWhenAnimationDone.SetActive(false);
        }

        if (IsServer && destroyAfterAnimation && GetComponent<NetworkObject>() != null)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }else if (destroyAfterAnimation)
        {
            Destroy(this.gameObject);
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

    public void destroy()
    {
        if (IsServer)
        {
            if (creatureModification != null)
            {
                if (target != null)
                {
                    target.getAttachedPermanent().GetComponent<Modifiable>().removeModification(GetComponent<NetworkObject>());
                }
                else if (targetGameObject != null)
                {
                    targetGameObject.GetComponent<Modifiable>().removeModification(GetComponent<NetworkObject>());
                }
            }
        }
    }
}
