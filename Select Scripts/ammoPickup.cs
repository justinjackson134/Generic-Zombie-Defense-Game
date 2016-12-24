using UnityEngine;
using System.Collections;

public class ammoPickup : MonoBehaviour
{
    public int ammoType = 0; // 1-pistol 2-shotgun 3-automatic 4-rifle 5-grenade
    public int ammoCount = 0; // the ammount to add (in magazines)

    [SerializeField]
    private GameObject pistolAmmo;
    [SerializeField]
    private GameObject shotgunAmmo;
    [SerializeField]
    private GameObject automaticAmmo;
    [SerializeField]
    private GameObject rifleAmmo;
    [SerializeField]
    private GameObject grenadeAmmo;

    private float rand = 0.0F; // used to determine ammo type

	// Use this for initialization
	void Start ()
    {
        pistolAmmo.SetActive(false);
        shotgunAmmo.SetActive(false);
        automaticAmmo.SetActive(false);
        rifleAmmo.SetActive(false);
        grenadeAmmo.SetActive(false);

        //rand = Random.Range(0.0F, 100.0F);


        if (rand < 40.0F)
        {
            ammoType = 1;
            pistolAmmo.SetActive(true);
        }
        else if (rand < 60.0F)
        {
            ammoType = 2;
            shotgunAmmo.SetActive(true);
        }
        else if (rand < 80.0F)
        {
            ammoType = 3;
            automaticAmmo.SetActive(true);
        }
        else if (rand < 95.0F)
        {
            ammoType = 4;
            rifleAmmo.SetActive(true);
        }
        else if (rand < 100.0F)
        {
            ammoType = 5;
            grenadeAmmo.SetActive(true);
        }

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
	void Update ()
    {
	
	}

    // punRpc DESTROY ME ACROSS NETWORK
}
