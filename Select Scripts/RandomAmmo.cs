using UnityEngine;
using System.Collections;

public class RandomAmmo : Photon.MonoBehaviour
{
    public float ChanceToSpawnAmmo = 100.0f;

    private int ammoType = 0; // 1-pistol 2-shotgun 3-automatic 4-rifle 5-grenade
    private float rand = 0.0F; // used to determine ammo type

    // Use this for initialization
    void Start()
    {
        if (PhotonNetwork.isMasterClient)
        {
            rand = Random.Range(0.0F, 100.0F);

            // Chance to spawn - rand > 0, if we should spawn. EX; Chance: 60 rand = 60 out of 100, Chance-Rand >= 0. TRUE
            if((ChanceToSpawnAmmo - rand) >= 0)
            {
                rand = Random.Range(0.0F, 100.0F);

                if (rand < 30.0F)
                {
                    ammoType = 1;
                    PhotonNetwork.InstantiateSceneObject("PistolAmmo", transform.position, new Quaternion(0, 0, 0, 0), 0, null);
                }
                else if (rand < 45.0F)
                {
                    ammoType = 2;
                    PhotonNetwork.InstantiateSceneObject("ShotgunAmmo", transform.position, new Quaternion(0, 0, 0, 0), 0, null);
                }
                else if (rand < 60.0F)
                {
                    ammoType = 3;
                    PhotonNetwork.InstantiateSceneObject("AutomaticAmmo", transform.position, new Quaternion(0, 0, 0, 0), 0, null);
                }
                else if (rand < 75.0F)
                {
                    ammoType = 4;
                    PhotonNetwork.InstantiateSceneObject("RifleAmmo", transform.position, new Quaternion(0, 0, 0, 0), 0, null);
                }
                else if (rand < 100.0F)
                {
                    ammoType = 5;
                    PhotonNetwork.InstantiateSceneObject("GrenadeAmmo", transform.position, new Quaternion(0, 0, 0, 0), 0, null);
                }
            }
        }
    }
}
