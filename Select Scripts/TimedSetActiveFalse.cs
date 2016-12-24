using UnityEngine;
using System.Collections;

public class TimedSetActiveFalse : MonoBehaviour
{
    public float LightActiveTime = 0.05f;

    private float TurnedOnAt = 0.0f;
    private bool isOff = true;
	
	// Update is called once per frame
	void Update ()
    {
        if(isOff)
        {
            //Debug.Log(gameObject.name.ToString() + "Turned on at: " + Time.time);
            TurnedOnAt = Time.time;
            isOff = false;
        }
	    if(Time.time - TurnedOnAt >= LightActiveTime)
        {
            //Debug.Log("Turning off at: " + Time.time + " After: " + (Time.time - TurnedOnAt) + " seconds");
            gameObject.SetActive(false);
            isOff = true;
        }
	}
}
