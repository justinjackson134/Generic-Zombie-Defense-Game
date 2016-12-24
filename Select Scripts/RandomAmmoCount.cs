using UnityEngine;
using System.Collections;

public class RandomAmmoCount : MonoBehaviour
{
    public int ammoCount = 0; // the ammount to add (in magazines)

    private float rand = 0.0F; // used to determine ammo type

    private bool wasPickedUp = false;

    // Use this for initialization
    void Start ()
    {
        rand = Random.Range(0.0F, 100.0F);

        if (rand < 20.0F)
        {
            ammoCount = 1;
        }
        else if (rand < 80.0F)
        {
            ammoCount = 2;
        }
        else if (rand < 100.0F)
        {
            ammoCount = 3;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (wasPickedUp)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    // punRpc DESTROY ME ACROSS NETWORK
    [PunRPC]
    public void pickedUp()
    {
        wasPickedUp = true;
    }
}
