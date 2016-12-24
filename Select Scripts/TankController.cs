using UnityEngine;
using System.Collections;

public class TankController : Photon.MonoBehaviour
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

    // Health
    public float myHealth = 100.0F;
    private bool isDead = false;
    private bool iDidTheDeathStuff = false;
    public float lingerTime = 5.0F;
    private CapsuleCollider myCapsuleCollider;

    // Attack
    private GameObject myAttackZone;

    // Use this for initialization
    void Start()
    {
        // DONT DESTROY
        //DontDestroyOnLoad(transform.gameObject);

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
                PhotonNetwork.Destroy(gameObject);
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
                Debug.Log("ZOMBIE DIED!");
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
        }

        myAgent.destination = myDestination;

        if (!photonView.isMine)
        {
            correctRemoteZombie();
        }
    }
    void correctRemoteZombie()
    {
        if (Vector3.Distance(transform.position, lastStoredPosition) > myAcceptablePositionError)
        {
            moveToSpeed = (Vector3.Distance(transform.position, lastStoredPosition) / timeToTravel);
            transform.position = Vector3.MoveTowards(transform.position, lastStoredPosition, moveToSpeed * Time.deltaTime);

            rotateToSpeed = (((transform.rotation.y - lastStoredRotation.y) * 360) / timeToTravel);
            if (rotateToSpeed < 0)
            {
                rotateToSpeed = rotateToSpeed * -1;
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lastStoredRotation, rotateToSpeed * Time.deltaTime);
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
