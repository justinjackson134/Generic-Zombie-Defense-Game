using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonManager : Photon.MonoBehaviour
{
    // Main menu
    [SerializeField]
    private GameObject pan_MainPanel;

    private string customRoomName = "";

    [SerializeField]
    private Text txt_CustomMatch;

    [SerializeField]
    private Button btn_QuickMatchObject;
    [SerializeField]
    private Button btn_CustomMatchObject;

    // Options menu
    [SerializeField]
    private GameObject pan_OptionsPanel;

    private string playerName = "";

    [SerializeField]
    private Text txt_PlayerName;
    [SerializeField]
    private Text placeholder_PlayerName;

    private void Start ()
    {
        Debug.Log("MainMenu Start!");

        // If we haven't added our initial data to EDEBUG
        if (!PlayerPrefs.HasKey("LogHasInit"))
        {
            // EDEBUG initial report and launch time
            EmailDebugger.PrepareInitialReport();
        }

        // Set once
        Application.targetFrameRate = 60;
        PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
        PhotonNetwork.automaticallySyncScene = true;

        // If not connected
        if (PhotonNetwork.connected == false)
        {
            //Debug.Log("I am connecting using settings");
            PhotonNetwork.ConnectUsingSettings("Pre-Alpha 0.1");
        }

        btn_CustomMatchObject.interactable = false;
        btn_QuickMatchObject.interactable = false;

        pan_OptionsPanel.SetActive(false);
    }

    private void Update()
    {
        if(GameObject.Find("ConnectionStatus") != null)
            GameObject.Find("ConnectionStatus").GetComponent<Text>().text = PhotonNetwork.connectionStateDetailed.ToString();
    }

    // Main Menu
    public void btn_Exit()
    {
        // EDEBUG email sent in onApplicationQuit        
        Application.Quit();
    }
    public void OnApplicationQuit()
    {
        // EDEBUG email sends on closing...
        EmailDebugger.DebugLogAppend(null);
        EmailDebugger.DebugLogAppend(" Application Quit");
        EmailDebugger.SendEmail();
    }

    public void btn_QuickMatch()
    {
        EmailDebugger.DebugLogAppend("Selected Quick Match");
        EmailDebugger.DebugLogAppend(" Moving to Level Select");

        RoomOptions ro = new RoomOptions() { isVisible = true, maxPlayers = 4 };
        PhotonNetwork.JoinRandomRoom();
    }

    public void btn_CustomMatch()
    {
        customRoomName = txt_CustomMatch.text;

        if (customRoomName != "")
        {
            EmailDebugger.DebugLogAppend("Selected Custom Match - Room Name: " + customRoomName);
            EmailDebugger.DebugLogAppend(" Moving to Level Select");

            RoomOptions ro = new RoomOptions() { isVisible = false, maxPlayers = 4 };
            PhotonNetwork.JoinOrCreateRoom(customRoomName, ro, TypedLobby.Default);
        }
        else
        {
            EmailDebugger.DebugLogAppend("Selected Custom Match - No Room Name Selected!");
        }
    }

    public void btn_options()
    {
        EmailDebugger.DebugLogAppend(" Opening Options Menu");

        pan_MainPanel.SetActive(false);
        pan_OptionsPanel.SetActive(true);

        placeholder_PlayerName.text = PlayerPrefs.GetString("PlayerName");
    }

    // Options menu
    public void btn_Return()
    {
        if (txt_PlayerName.text != null && txt_PlayerName.text != "")
        {
            PlayerPrefs.SetString("PlayerName", txt_PlayerName.text);
            PlayerPrefs.Save();
        }

        if (PhotonNetwork.connected == true)
        {
            //Debug.Log("We are connected and the player changed their name, so we are updating it");
            PhotonNetwork.playerName = PlayerPrefs.GetString("PlayerName");
        }

        EmailDebugger.DebugLogAppend(" Closing Options Menu");
        EmailDebugger.DebugLogAppend(null);

        pan_OptionsPanel.SetActive(false);
        pan_MainPanel.SetActive(true);
    }

    // Photon
    void OnConnectedToMaster()
    {
        //Debug.Log("Connected, So join lobby");
        PhotonNetwork.JoinLobby();
    }
    void OnDisconnectedFromPhoton()
    {
        //Debug.Log("Disconnected, so lets join lobby");
        PhotonNetwork.JoinLobby();
    }
    void OnJoinedLobby()
    {
        //Debug.Log("Joined Lobby");

        if (PhotonNetwork.connected == true)
        {
            //Debug.Log("We are connected and updated players name");
            PhotonNetwork.playerName = PlayerPrefs.GetString("PlayerName");
        }

        btn_CustomMatchObject.interactable = true;
        btn_QuickMatchObject.interactable = true;
    }

    void OnCreatedRoom()
    {
        // Load menu
        EmailDebugger.DebugLogAppend("  Created New Game");
        PhotonNetwork.LoadLevel(1);
    }

    void OnJoinedRoom()
    {
        // Join whatever scene the master is on
        if (!PhotonNetwork.isMasterClient)
        {
            EmailDebugger.DebugLogAppend("  Join in Progress: Scene Number " + PhotonNetwork.masterClient.customProperties["SceneID"]);
            PhotonNetwork.LoadLevel((int)PhotonNetwork.masterClient.customProperties["SceneID"]);
        }
    }

    void OnPhotonRandomJoinFailed()
    {
        RoomOptions ro = new RoomOptions() { isVisible = true, maxPlayers = 4 };
        PhotonNetwork.CreateRoom(null, ro, TypedLobby.Default);
    }
}
