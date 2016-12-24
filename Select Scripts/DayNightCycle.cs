using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.Rotate(0.6F * Time.deltaTime,0.0F,0.0F);
	}
}
