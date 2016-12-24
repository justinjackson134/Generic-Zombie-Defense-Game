using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class EscapeMenuManager : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject escapePanel;

    [SerializeField]
    private GameObject btn_QuitObject;
    [SerializeField]
    private GameObject btn_ReturnObject;

    // Turn off the panel, lock and hide the cursor
    void Start ()
    {
        escapePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.Escape))
        {
            escapePanel.SetActive(!escapePanel.GetActive());
            if(escapePanel.GetActive())
            {
                EmailDebugger.DebugLogAppend("    Opening Escape Menu");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                EmailDebugger.DebugLogAppend("    Closing Escape Menu");
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
	}

    public void btn_Quit()
    {
        Application.Quit();
    }
    public void OnApplicationQuit()
    {
        // EDEBUG email sends on closing...
        EmailDebugger.DebugLogAppend(null);
        EmailDebugger.DebugLogAppend(" Application Quit");
        EmailDebugger.SendEmail();
    }

    public void btn_Return()
    {
        EmailDebugger.DebugLogAppend("  Returning to Main Menu");
        EmailDebugger.DebugLogAppend(null);
        PhotonNetwork.LeaveRoom();
    }

    void OnLeftRoom()
    {        
        PhotonNetwork.LoadLevel(0);
    }
}


//// BACKUP
//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class EscapeMenuManager : Photon.MonoBehaviour
//{
//    [SerializeField]
//    private GameObject escapePanel;

//    [SerializeField]
//    private GameObject btn_QuitObject;
//    [SerializeField]
//    private GameObject btn_ReturnObject;

//    // Use this for initialization
//    void Start()
//    {
//        escapePanel.SetActive(false);
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Escape))
//        {
//            escapePanel.SetActive(!escapePanel.GetActive());
//            if (escapePanel.GetActive())
//            {
//                Cursor.lockState = CursorLockMode.None;
//                Cursor.visible = true;
//            }
//            else
//            {
//                Cursor.lockState = CursorLockMode.Locked;
//                Cursor.visible = false;
//            }
//        }
//    }

//    public void btn_Quit()
//    {
//        Application.Quit();
//    }

//    public void btn_Return()
//    {
//        foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
//        {
//            Destroy(o);
//        }

//        // Disconnecting should allow the main menu to work again, as it expects that you start disconnected...
//        //PhotonNetwork.Disconnect();
//        // calls OnDisc.. load menu there...

//        // THIS BREAKS ALL THE THINGS.
//        PhotonNetwork.LeaveRoom();
//        PhotonNetwork.LoadLevel(0);
//    }

//    //public void OnDisconnectedFromPhoton()
//    //{
//    //    Debug.Log("OnDisconnectedFromPhoton was called, returning to main menu");
//    //    Debug.Log("ON-DISCONNECTED-FROM-PHOTON: " + PhotonNetwork.connectionState.ToString());

//    //    PhotonNetwork.LoadLevel(0);
//    //}
//}