using UnityEngine;
using System.Collections;

public class PlayerFire : MonoBehaviour {

	// Set at runtime
	private Camera myCamera;
    private int myAmmoCount;

    // Muzzleflash stuff
    //private Light myMuzzleFlashLight;
    //private ParticleSystem myMuzzleFlashParticleSystem;

	// Used in script
	private RaycastHit myHitInfo;

	// Use this for initialization
	void Start () 
	{
		myCamera = GetComponentInChildren<Camera>();
        myAmmoCount = 10;
        //myMuzzleFlashLight = GetComponentInChildren<Light>();
        //myMuzzleFlashParticleSystem = GetComponentInChildren<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () 
	{
        // Reload?
        if (Input.GetButtonDown("Reload"))
        {
            Debug.Log("Reloading...");
            myAmmoCount = 10;
            Debug.Log("Reloaded");
        }

        // Shoot?
		if(Input.GetButtonDown("Fire1"))
		{
            // Print ammo count
            Debug.Log("myAmmoCount: " + myAmmoCount);

            Debug.Log("FIRE");
            if(myAmmoCount > 0)
            {
                myAmmoCount--;

                // Do flash in coroutine
                //myMuzzleFlashLight.enabled = true;
                //myMuzzleFlashParticleSystem.Play();
                // Turn light off
                //myMuzzleFlashLight.enabled = false;

                if (Physics.Raycast(myCamera.transform.position, myCamera.transform.TransformDirection(Vector3.forward), out myHitInfo, Mathf.Infinity))
                {
                    Debug.Log("Object Hit: " + myHitInfo.collider.tag.ToString());

                    if (myHitInfo.collider.tag == "Enemy")
                    {
                        myHitInfo.collider.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 50.0F);
                    }
                    if (myHitInfo.collider.tag == "EnemyHead")
                    {
                        myHitInfo.collider.GetComponentInParent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 100.0F);
                    }
                }                
            }
        }
	}
}
