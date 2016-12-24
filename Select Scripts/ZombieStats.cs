using UnityEngine;
using System.Collections;

public class ZombieStats : Photon.MonoBehaviour {

	// Set at runtime
	public float myHealth;

	// Use this for initialization
	void Start () 
	{
		myHealth = 100;
	}

	// Called from PlayerFire.cs
    [PunRPC]
	public void TakeDamage(float damage)
	{
		myHealth = myHealth - damage;

		if(myHealth <= 0 && photonView.isMine)
		{
			Debug.Log("CRAWLER DIED! INSTANTIATION ID: " + photonView.instantiationId);
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
		}        
	}
}
