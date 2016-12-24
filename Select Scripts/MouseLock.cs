using UnityEngine;
using System.Collections;

public class MouseLock : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	
    void OnApplicationFocus(bool focusStatus)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}
