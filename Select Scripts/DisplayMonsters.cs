using UnityEngine;
using System.Collections;

public class DisplayMonsters : Photon.MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.InstantiateSceneObject("Crawler", GameObject.Find("DisplayCrawlerSpawn").transform.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Zombie", GameObject.Find("DisplayZombieSpawn").transform.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Tank", GameObject.Find("DisplayTankSpawn").transform.position, Quaternion.identity, 0, null);
        }
    }
}
