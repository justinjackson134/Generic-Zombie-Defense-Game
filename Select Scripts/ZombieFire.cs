using UnityEngine;
using System.Collections;

public class ZombieFire : MonoBehaviour {

    // Set at runtime
    public float myAttackDelay = 0.75F;
    private float lastAttackTime;
    private float lastSwingTime;
    private float initialAttackBoost = 0.5F;

    private Animator myAnimator;
    private Animation myAnimation;

    // Sound
    public AudioClip attackSound;

    // Use this for initialization
    void Start ()
    {
        lastAttackTime = 0.0F;
        
        // Random animstate
        myAnimator = transform.root.GetComponentInChildren<Animator>();

        // Legacy for crawler
        myAnimation = transform.root.GetComponentInChildren<Animation>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        lastAttackTime = Time.time - initialAttackBoost; // Should increase the initial attack speed so that zombies hit faster, while consecutive hits remain slow
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if ((Time.time - lastSwingTime) > 0.5F)
            {
                lastSwingTime = Time.time;

                // Do null check, cause crawler has no animator
                if (myAnimator != null)
                {
                    switch (Random.Range(1, 4))
                    {
                        case 1:
                            myAnimator.Play("Attack", -1);
                            if (attackSound != null)
                            {
                                AudioSource.PlayClipAtPoint(attackSound, transform.position);
                            }
                            break;

                        case 2:
                            myAnimator.Play("Attack2", -1);
                            if (attackSound != null)
                            {
                                AudioSource.PlayClipAtPoint(attackSound, transform.position);
                            }
                            break;

                        case 3:
                            myAnimator.Play("Attack3", -1);
                            if (attackSound != null)
                            {
                                AudioSource.PlayClipAtPoint(attackSound, transform.position);
                            }
                            break;
                    }
                }

                // Legacy animation for crawler
                if (myAnimation != null)
                {
                    myAnimation["Attack"].wrapMode = WrapMode.Once;
                    myAnimation.Play("Attack");

                    myAnimation["Walk"].wrapMode = WrapMode.Loop;
                    myAnimation.PlayQueued("Walk");
                }
            }

            //Debug.Log("PLAYER STILL IN ZONE");
            attackPlayer(other);
        }
    }

    void attackPlayer(Collider other)
    {
        if ((Time.time - lastAttackTime) > myAttackDelay)
        {
            lastAttackTime = Time.time;
            other.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 15.0F);
        }
    }
}
