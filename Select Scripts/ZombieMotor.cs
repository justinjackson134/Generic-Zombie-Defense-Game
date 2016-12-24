using UnityEngine;
using System.Collections;

public class ZombieMotor : Photon.MonoBehaviour {

	// Destination var
	private Vector3 myDestination;
    private float distanceToTarget = Mathf.Infinity;
    private NavMeshAgent myAgent;

    // TESTING TESTING //
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
    // TESTING TESTING //

    // Animator
    private Animator myAnimator;
    private Animation myAnimation;

    // Use this for initialization
    void Start () 
	{
        // DONT DESTROY
        //DontDestroyOnLoad(transform.gameObject);

        myAgent = GetComponent<NavMeshAgent>();

        // Set pos and rot initally
        lastStoredPosition = transform.position;
        lastStoredRotation = transform.rotation;

        if (GameObject.Find("Player") == null)
        {
            myDestination = GameObject.FindGameObjectWithTag("Mission").transform.position;
        }
        else
        {
            myDestination = GameObject.Find("Player").transform.position;
        }

        myAgent.destination = myDestination;

        // Random animstate
        myAnimator = GetComponentInChildren<Animator>();
        // Do null check, cause crawler has no animator
        if( myAnimator != null )
        {
            myAnimator.Play("Run", -1, UnityEngine.Random.Range(0.0f, 1.0f));
        }

        // Legacy for crawler
        myAnimation = GetComponentInChildren<Animation>();
        if (myAnimation != null)
        {
            myAnimation["Walk"].wrapMode = WrapMode.Loop;
        }
    }

    // Update is called once per frame
    void Update () 
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
        
        if(!photonView.isMine)
        { 
            correctRemoteZombie();
        }
	}

    void correctRemoteZombie()
    {
        if (Vector3.Distance(transform.position, lastStoredPosition) > myAcceptablePositionError)
        {
            //Debug.Log("Crawler too far from acceptable.");

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
