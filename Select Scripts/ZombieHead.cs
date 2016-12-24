using UnityEngine;
using System.Collections;

public class ZombieHead : MonoBehaviour
{
    // THIS FUNCTION PASSES THE TAKE DAMAGE MESSAGE FROM HEAD TO BODY..
 
	// Use this for initialization
	void Start ()
    {
	
	}

    // Called from PlayerFire.cs
    void TakeDamage(float damage)
    {
        SendMessageUpwards("TakeDamage", damage);
    }

}
