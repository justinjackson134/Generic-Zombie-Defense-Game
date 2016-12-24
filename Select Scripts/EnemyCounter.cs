using UnityEngine;
using System.Collections;

public class EnemyCounter : MonoBehaviour {

	private int mobCount;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{	
		mobCount = 0;

		mobCount = GameObject.FindGameObjectsWithTag("Enemy").GetLength(0);

		Debug.Log("Zombies Alive: " + mobCount);
	}
}
