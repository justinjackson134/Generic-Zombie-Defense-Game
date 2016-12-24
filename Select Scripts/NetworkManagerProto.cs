using UnityEngine;
using System.Collections;

public class NetworkManagerProto : Photon.MonoBehaviour
{
    // Player
    GameObject player;

    // main camera
    GameObject mainCam;

    // Player UI
    GameObject playerUI;

    // Spawn vars
    private GameObject[] playerSpawns = new GameObject[30];
    private int mySpawnRand = 0;
    private bool respawnPlayerRunning = false;
    private bool weLostBool = false;

    // ONLY USE THE BELOW COMMENT BLOCK WHEN TESTING IN EDITOR AND AUTO LOADING THE SCENE!!
    /*
    // Use this for initialization
	void Start ()
    {
        PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.ConnectUsingSettings("Pre-Alpha 0.1");
	}
        
    void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");

        RoomOptions ro = new RoomOptions() { isVisible = true, maxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("Room", ro, TypedLobby.Default);
    }

    void OnJoinedRoom()
    {
        //Debug.Log("Joined Room");

        // Moved to own function to allow yielding for seconds to allow new users to catch up on all of the existing zombies data.... ROTATIONS GOD DAMN IT!
        StartCoroutine(spawnPlayer());
    }
    //*/

    // USE THE BELOW FOR ACTUAL BUILDS
    ///*
    void Start ()
    {
        //Debug.Log("Start! (CALLED FROM NETMANAGER)");

        // Find main camera
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");

        // Find player UI
        playerUI = GameObject.Find("PlayerUI");

        // Lookup playerName
        if (PlayerPrefs.GetString("PlayerName") != null)
        {
            PhotonNetwork.playerName = PlayerPrefs.GetString("PlayerName");
        }
        else
        {
            PhotonNetwork.playerName = PhotonNetwork.player.ID.ToString();
        }

        // Find player spawns
        playerSpawns = GameObject.FindGameObjectsWithTag("PlayerSpawnPoint");

        // Commented out, NEW PLAYERS SHOULD NOT GET TOO SPAWN WITHOUT CHECKING LOGIC FIRST.
        // Moved to own function to allow yielding for seconds to allow new users to catch up on all of the existing zombies data.... ROTATIONS GOD DAMN IT!
        //if (GameObject.FindGameObjectsWithTag("Zombie").GetLength(0) <= 0)
        //{
        //    StartCoroutine(respawnPlayer());
        //}
    }
    //*/

    // DUPLICATE FUNCTION!!
    //IEnumerator spawnPlayer()
    //{
    //    yield return new WaitForSeconds(1.0F);

    //    player = PhotonNetwork.Instantiate("Player", new Vector3(250, 35, 225), new Quaternion(0, 0, 0, 0), 0);
    //    player.GetComponentInChildren<Camera>().enabled = true;
    //    player.GetComponentInChildren<AudioListener>().enabled = true;

    //    yield return null;
    //}

    IEnumerator respawnPlayer()
    {
        respawnPlayerRunning = true;

        yield return new WaitForSeconds(2.5F);
        Debug.Log("I AM IN THE COUROUTINE");

        // If we lost, leave without spawning player
        if (weLostBool)
        {
            Debug.Log("I AM IN THE COUROUTINE BUT SEE THAT WE LOST, Kill Couroutine, do not spawn player");
            respawnPlayerRunning = false;
            yield return null;
        }
        else
        {
            // Disable main camera
            mainCam.SetActive(false);

            // Reactivate Player UI
            playerUI.SetActive(true);

            // Select random spawn point
            mySpawnRand = Random.Range(0, (playerSpawns.GetLength(0)));

            player = PhotonNetwork.Instantiate("Player", playerSpawns[mySpawnRand].transform.position, new Quaternion(0, 0, 0, 0), 0);
            player.GetComponentInChildren<Camera>().enabled = true;
            player.GetComponentInChildren<AudioListener>().enabled = true;

            respawnPlayerRunning = false;

            EmailDebugger.DebugLogAppend("    Player Respawned");

            yield return null;
        }
    }

    // Update is called once per frame
    void Update ()
    {
	    if ((player == null) && (mainCam.GetActive() != true))
        {
            mainCam.SetActive(true);
            Debug.Log("NET MAN: I see that local player has died");
            EmailDebugger.DebugLogAppend("    Player Died");

            // Turn off player UI
            playerUI.SetActive(false);
        }
        
        if ((player == null) && (GameObject.FindGameObjectsWithTag("Zombie").GetLength(0) <= 0) && (respawnPlayerRunning == false))
        {
            Debug.Log("I see that local player is dead and there are no zombies, RESPAWN");
            StartCoroutine(respawnPlayer());
        }

        if (Time.timeSinceLevelLoad > 10.0f && (GameObject.FindGameObjectsWithTag("Player").GetLength(0) <= 0) && (GameObject.FindGameObjectsWithTag("Zombie").GetLength(0) > 0))
        {
            Debug.Log("All players are dead and no one can be respawning as there still are zombies! So, WE LOST. Return to menu");
            EmailDebugger.DebugLogAppend("    Game Lost");
            weLostBool = true;
            photonView.RPC("weLost", PhotonTargets.All);            
        }
    }

    [PunRPC]
    void weLost()
    {
        weLostBool = true;

        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("I am the MASTER CLIENT and I was told we lost! Now Cleaning all players RPCs");
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                PhotonNetwork.RemoveRPCs(PhotonNetwork.playerList[i]);
            }
        }
        else
        {
            Debug.Log("I am a CLIENT and I was told we lost!");
        }
        StopAllCoroutines();

        //foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
        //{
        //    Destroy(go);
        //}

        EmailDebugger.DebugLogAppend("  Returning to Level Select");
        PhotonNetwork.LoadLevel(1);
    }
}
