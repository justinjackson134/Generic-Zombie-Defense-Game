using UnityEngine;
using System.Collections;

public class ZombieController : Photon.MonoBehaviour
{
    // Movement
    // Destination var
    private Vector3 myDestination;
    private float distanceToTarget = Mathf.Infinity;
    private NavMeshAgent myAgent;
    // Remote zombie movement
    private Vector3 lastStoredPosition;
    private float moveToSpeed = 1.0F;
    private Quaternion lastStoredRotation;
    private float rotateToSpeed = 1.0F;
    // Remote player universal vars
    private float lastMessageTime;
    private float timeToTravel;
    // Customization
    public float myAcceptablePositionError = 0.25F;
    public float lookAtDistance = 5.0F;
    // OnPhotonSerializeView vars
    public float targetMessageDelayTime = 0.1F; // Send 10 updates a second
    private float messageDelayTimer = 0.0F;
    
    // Animator
    private Animator myAnimator;

    // Sounds
    private AudioSource myAudioSource;
    public AudioClip footStep1;
    public AudioClip footStep2;
    public AudioClip footStep3;
    public AudioClip footStep4;
    public AudioClip footStep5;
    public AudioClip footStep6;

    // Health
    public float myHealth = 100.0F;
    private bool isDead = false;
    private bool iDidTheDeathStuff = false;
    public float lingerTime = 5.0F;
    private CapsuleCollider myCapsuleCollider;

    // Attack
    private GameObject myAttackZone;

    // Array of dead zombies
    private GameObject[] deadThings = new GameObject[512];

    // Use this for initialization
    void Start()
    {
        // DONT DESTROY
        //DontDestroyOnLoad(transform.gameObject);

        // Sound
        myAudioSource = GetComponent<AudioSource>();

        // Movement
        myAgent = GetComponent<NavMeshAgent>();
        // Set pos and rot initally
        lastStoredPosition = transform.position;
        lastStoredRotation = transform.rotation;
        // Set target
        if (GameObject.Find("Player") == null)
        {
            myDestination = GameObject.FindGameObjectWithTag("Mission").transform.position;
        }
        else
        {
            myDestination = GameObject.Find("Player").transform.position;
        }
        // Given target, set nav actor destination
        myAgent.destination = myDestination;

        // Animations
        myAnimator = GetComponentInChildren<Animator>();
        // Random animstate
        myAnimator.Play("Run", -1, UnityEngine.Random.Range(0.0f, 1.0f));

        // Health
        myCapsuleCollider = GetComponent<CapsuleCollider>();

        // Attack
        myAttackZone = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        didIDie();
        if (!isDead)
        {
            moveMe();
        }
    }

    // Health
    void didIDie()
    {
        if (isDead)
        {
            if (!iDidTheDeathStuff)
            {
                // Set to true to only perform once
                iDidTheDeathStuff = true;

                // No attacking if your dead!
                myAttackZone.SetActive(false);

                // Play death animation
                myAnimator.Play("Die");

                // Stop moving
                myAgent.enabled = false;

                // No more colisions!
                myCapsuleCollider.enabled = false;
            }

            // If weve waited long enough
            if (lingerTime < 0)
            {
                //gameObject.tag = "Dead";

                // Why can't I just do this?
                if (PhotonNetwork.isMasterClient)
                {
                    PhotonNetwork.Destroy(photonView);
                }

                //// Even though this only gets sent to the master, if everyone sends a copy, the master may end up trying to delete something that doesn't exist. This MAY fix it...
                //if (PhotonNetwork.isMasterClient)
                //{
                //    // dont use RPC here
                //    deadThings = GameObject.FindGameObjectsWithTag("Dead");
                //    foreach (GameObject go in deadThings)
                //    {
                //        if (go != null) // make sure this gameobject hasn't been destroyed already...
                //        {
                //            PhotonNetwork.Destroy(go);
                //        }
                //    }

                //    //photonView.RPC("MasterDestroyDeads", PhotonTargets.MasterClient);
                //}
            }
            else // Decrease remaining time
            {
                lingerTime -= Time.deltaTime;
            }
        }
    }
    // Called from PlayerFire.cs
    [PunRPC]
    public void TakeDamage(float damage)
    {
        if (!isDead)
        {
            myHealth = myHealth - damage;
            if (Random.Range(0, 2) == 1)
            {
                myAnimator.Play("HitLeft");
            }
            else
            {
                myAnimator.Play("HitRight");
            }

            // I died!
            if (myHealth <= 0)
            {
                Debug.Log("ZOMBIE DIED! Instantiation ID: " + photonView.instantiationId);
                // Set dead
                isDead = true;
            }
        }
    }

    // Movement
    void moveMe()
    {
        distanceToTarget = Mathf.Infinity;

        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            myDestination = GameObject.FindGameObjectWithTag("Mission").transform.position;
        }
        else
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (Vector3.Distance(transform.position, player.transform.position) < distanceToTarget)
                {
                    distanceToTarget = Vector3.Distance(transform.position, player.transform.position);
                    myDestination = player.transform.position;
                }
            }
            if (distanceToTarget < lookAtDistance)
            {
                transform.LookAt(new Vector3(myDestination.x, transform.position.y, myDestination.z));
            }

            // Don't play footstep if zombie is probably already attacking player (Theoretically it should be stopped by now...)
            if (distanceToTarget > 2.0f)
            {
                playFootstep();
            }
        }

        myAgent.destination = myDestination;

        if (!photonView.isMine)
        {
            correctRemoteZombie();
        }
    }
    void correctRemoteZombie()
    {
        //transform.position = Vector3.Lerp(transform.position, lastStoredPosition, Time.deltaTime * 15);

        if (Vector3.Distance(transform.position, lastStoredPosition) > myAcceptablePositionError)
        {
            //Debug.Log("Zombie too far from acceptable");

            moveToSpeed = (Vector3.Distance(transform.position, lastStoredPosition) / timeToTravel);
            transform.position = Vector3.MoveTowards(transform.position, lastStoredPosition, moveToSpeed * Time.deltaTime);

            rotateToSpeed = (((transform.rotation.y - lastStoredRotation.y) * 360) / timeToTravel);
            if (rotateToSpeed < 0)
            {
                rotateToSpeed = rotateToSpeed * -1;
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lastStoredRotation, rotateToSpeed * Time.deltaTime);
        }

        // if 3x acceptable position error, jump to correct spot
        if (Vector3.Distance(transform.position, lastStoredPosition) > (myAcceptablePositionError * 3))
        {
            transform.position = lastStoredPosition;
        }
    }
    
    // Play local sounds-- these are not network sync'd
    void playFootstep()
    {
        if (!myAudioSource.isPlaying)
        {
            switch (Random.Range(1, 7))
            {
                case 1:
                    //Debug.Log("1");
                    myAudioSource.clip = footStep1;
                    myAudioSource.Play();
                    break;
                case 2:
                    //Debug.Log("2");
                    myAudioSource.clip = footStep2;
                    myAudioSource.Play();
                    break;
                case 3:
                    //Debug.Log("3");
                    myAudioSource.clip = footStep3;
                    myAudioSource.Play();
                    break;
                case 4:
                    //Debug.Log("4");
                    myAudioSource.clip = footStep4;
                    myAudioSource.Play();
                    break;
                case 5:
                    //Debug.Log("5");
                    myAudioSource.clip = footStep5;
                    myAudioSource.Play();
                    break;
                case 6:
                    //Debug.Log("6");
                    myAudioSource.clip = footStep6;
                    myAudioSource.Play();
                    break;
            }
        }

    }

    // Send/Recieve info
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Update Message delayTimer
        messageDelayTimer += Time.deltaTime;

        // If I havent sent an update for awhile
        if (messageDelayTimer >= targetMessageDelayTime)
        {
            //This is mine to send
            if (stream.isWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            // Reset timer
            messageDelayTimer = 0.0F;
        }

        // Dont care about when this comes, recieve updates whenever sent
        if (stream.isReading)
        {
            // Store recieved position
            lastStoredPosition = (Vector3)stream.ReceiveNext();

            //// Store recieved rotation
            lastStoredRotation = (Quaternion)stream.ReceiveNext();

            // Record time vars
            timeToTravel = Time.time - lastMessageTime;
            lastMessageTime = Time.time;
        }
    }
}
