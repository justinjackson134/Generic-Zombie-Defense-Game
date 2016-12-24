using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameLogic : MonoBehaviour {
    [SerializeField]
    private Text gameStatusText;

    private float lastWaveSpawnTime;
    private int wave;

	// Use this for initialization
	void Start ()
    {
        lastWaveSpawnTime = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(lastWaveSpawnTime - Time.time > 5.0F)
        {

        }
	}
}
