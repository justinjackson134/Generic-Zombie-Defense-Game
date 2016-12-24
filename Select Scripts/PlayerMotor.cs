using UnityEngine;
using System.Collections;

public class PlayerMotor : Photon.MonoBehaviour {

	// Set at runtime
	private Rigidbody myRigidBody;
	private Camera myCamera;
    private float mySpeedModifier;

    // Player customization
    public float MouseSensitivity = 3.0F;

    // Remote player movement
    Vector3 position;
    Quaternion rotation;
    float smoothing = 10f;

    // Use this for initialization
    void Start () 
	{
		myRigidBody = GetComponent<Rigidbody>();
		myCamera = GetComponentInChildren<Camera>();
        mySpeedModifier = 1.0F;

        rotation = new Quaternion( 0, 0, 0, 0 );
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Debug.Log("SERIAL");

        if (stream.isWriting)
        {
            Debug.Log("PLAYER IS WRITING: " + transform.position + ", " + transform.rotation);

            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            position = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    // Update is called once per frame
    void Update() 
	{
        if (photonView.isMine)
        {
            // Debug log
            //Debug.Log("Vertical: " + Input.GetAxis("Vertical") + " Horizontal: " + Input.GetAxis("Horizontal"));

            // Are we moving in two directions?
            // Reset modifier
            mySpeedModifier = 1.0F;
            // Change modifier
            if (Mathf.Abs(Input.GetAxis("Vertical")) > 0 && Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
            {
                mySpeedModifier = 0.65F;
            }

            // Are we sprinting?
            if (Mathf.Abs(Input.GetAxis("Sprint")) > 0)
            {
                mySpeedModifier = mySpeedModifier + 0.8F;
            }

            // Forward/Backward motion
            if (Mathf.Abs(Input.GetAxis("Vertical")) > 0)
            {
                transform.Translate(0.0F, 0.0F, 7.0F * Input.GetAxis("Vertical") * mySpeedModifier * Time.deltaTime);
            }
            // Horizontal motion
            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
            {
                transform.Translate(6.0F * Input.GetAxis("Horizontal") * mySpeedModifier * Time.deltaTime, 0.0F, 0.0F);
            }
            // Jump
            if (Mathf.Abs(Input.GetAxis("Jump")) > 0)
            {
                Debug.Log("JUMP");
                // If Grounded
                if (Physics.Raycast(transform.position, -Vector3.up, GetComponent<Collider>().bounds.extents.y + 0.4F))
                {
                    myRigidBody.AddForce(0.0F, 150.0F, 0.0F);
                }
            }

            // Pitch Camera
            myCamera.transform.Rotate(Input.GetAxisRaw("Mouse Y") * -1.0F * MouseSensitivity, 0.0F, 0.0F);

            // Rotate Character
            transform.Rotate(0.0F, Input.GetAxisRaw("Mouse X") * MouseSensitivity, 0.0f);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * smoothing);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);
        }
    }
}