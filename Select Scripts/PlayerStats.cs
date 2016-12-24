using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour {

    public float myHealth;

	// Use this for initialization
	void Start ()
    {
        myHealth = 100.0F;
	}
	
	// Update is called once per frame
	void Update ()
    {
        
	}

    void TakeDamage(float damage)
    {
        myHealth = myHealth - damage;

        if (myHealth <= 0)
        {
            Debug.Log("PLAYER DIED!");
            Destroy(gameObject);
        }
    }
}
