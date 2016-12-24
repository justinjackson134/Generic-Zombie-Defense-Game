using UnityEngine;
using System.Collections;

public class Grenade : Photon.MonoBehaviour
{
    // My Vars
    private float myDetonationTime = 3.0F;
    private float mySpawnTime;
    private Collider[] colls;
    private ParticleSystem myParticleSystem;
    private bool detonated = false;

    // Remote player movement
    private Vector3 endPosition;
    private Vector3 lastStoredPosition;
    private float moveToSpeed = 1.0F;
    // Remote player universal vars
    private float lastMessageTime;
    private float timeToTravel;

    // Cover check
    private RaycastHit myRayHit;

    // Damage effectiveness
    private float myRange = 5.0F;
    private float myDamage = 250;
    private float proximity = 0.0F;
    private float effect = 0.0F;

    // Use this for initialization
    void Start()
    {
        mySpawnTime = Time.time;
        myParticleSystem = GetComponent<ParticleSystem>();

        // Remote player movement
        lastMessageTime = Time.time;
        timeToTravel = 0.1F;
        lastStoredPosition = transform.position;
        endPosition = transform.position;

        if(!photonView.isMine)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveRemoteGrenade();

        if (Time.time - mySpawnTime >= myDetonationTime && !detonated)
        {
            detonated = true;

            // Play sound
            GetComponent<AudioSource>().Play();
            //AudioSource.PlayClipAtPoint(GetComponent<AudioSource>().clip, transform.position, 1000.0F);

            // Play particleSystem
            myParticleSystem.Play();

            colls = Physics.OverlapSphere(transform.position, myRange);

            foreach (Collider col in colls)
            {
                if (col.tag == "Zombie" || col.tag == "Crawler" || col.tag == "Player")
                {
                    // Shoot ray from grenade towards target to check for exposure
                    if (Physics.Raycast (transform.position, (col.transform.position-transform.position), out myRayHit, myRange))
                    {
                        // Is the col exposed?
                        if ((col.transform.root.GetComponent<PhotonView>() != null) && (myRayHit.transform.root.GetComponent<PhotonView>() != null))
                        {
                            if (col.transform.root.GetComponent<PhotonView>().viewID == myRayHit.transform.root.GetComponent<PhotonView>().viewID)
                            {
                                //Debug.DrawRay(transform.position, (col.transform.position - transform.position), Color.green, 60.0F);
                                proximity = (transform.position - col.transform.position).magnitude;
                                effect = 1 - (proximity / myRange);

                                // The grenade is healing at its max extents.... WHY?!
                                if (effect > 0)
                                {
                                    // NEW! I once got an error here, so this should fix it (it was a null reference)
                                    if (col.transform.root.GetComponent<PhotonView>() != null)
                                    {
                                        col.transform.root.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, myDamage * effect);
                                    }
                                }
                            }
                        }
                        else // not exposed
                        {
                            //Debug.DrawRay(transform.position, (col.transform.position - transform.position), Color.red, 60.0F);
                        }
                    }                    
                }
            }
        }
        if ((Time.time - mySpawnTime) >= (myDetonationTime + 0.5F))
        {
            if (photonView.isMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    void moveRemoteGrenade()
    {
        if (!photonView.isMine)
        {
            // Handle movement
            if (endPosition == lastStoredPosition)
            {
                // Move clone towards end position
                transform.position = Vector3.MoveTowards(transform.position, endPosition, moveToSpeed * Time.deltaTime);
            }
            else
            {
                // Handles remote destination setting
                endPosition = lastStoredPosition;
                // Calculate Lerp speeds
                moveToSpeed = (Vector3.Distance(transform.position, endPosition) / timeToTravel);
            } 
        }
    }

    // Send/Recieve info
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
        }

        // Dont care about when this comes, recieve updates whenever sent
        if (stream.isReading)
        {
            // Store recieved position
            lastStoredPosition = (Vector3)stream.ReceiveNext();

            // Record time vars
            timeToTravel = Time.time - lastMessageTime;
            lastMessageTime = Time.time;
        }
    }
}
