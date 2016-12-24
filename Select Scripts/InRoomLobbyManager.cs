using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InRoomLobbyManager : Photon.MonoBehaviour
{
    private PhotonView menuManager;
    private ExitGames.Client.Photon.Hashtable playerCustomProps;
    private float lastUpdateTime = 0.0f;
    private float voteTimer = 30.0f;
    private float launchTimer = 5.0f;
    private string statusNameString = "Voting";

    public Text Player1Name;
    public Text Player2Name;
    public Text Player3Name;
    public Text Player4Name;

    public Text Player1Ping;
    public Text Player2Ping;
    public Text Player3Ping;
    public Text Player4Ping;

    public Button ReplayLevelButton;
    public Button SelectLevel1;
    public Button SelectLevel2;
    public Button SelectLevel3;

    public string ReplayLevelButtonString;
    public string SelectLevel1String;
    public string SelectLevel2String;
    public string SelectLevel3String;

    public Text RoomName;
    public Text StatusName;
    public Text LevelName;

    private int lastScene;
    private int nextScene;
    private List<int> scenes = new List<int>();
    private List<string> sceneNames = new List<string>();
    private List<int> scenesToVote = new List<int>();
    private List<int> voteList = new List<int>();
    private List<int> voteValues = new List<int>();
    private List<int> checkVotesList = new List<int>();
    private int randHolder = 0;
 
    public Text TimerText;
    public Text TimerTime;
    
    // Use this for initialization
    void Start ()
    {
        // GIVE CURSOR BACK IN CASE ITS LOCKED FROM A SCENE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        menuManager = GameObject.Find("MenuManager").GetComponent<PhotonView>();
        
        playerCustomProps = new ExitGames.Client.Photon.Hashtable();
        playerCustomProps["SceneID"] = 1; // Default to menu
        playerCustomProps["MyVote"] = -1; // Default to noVote
        // Grab last scene ID before calling update and resetting to 1
        if (PhotonNetwork.masterClient.customProperties["SceneID"] != null)
        {
            lastScene = (int)PhotonNetwork.masterClient.customProperties["SceneID"];
        }
        else
        {
            lastScene = 1;
        }
        updatePlayerCustomPrefs();

////////////////////// BEGIN SCENES //////////////////////

        // Populate total scene list HARDCODE
        sceneNames.Add("Not a map");
        sceneNames.Add("Not a map");

        scenes.Add(2); // NAME: dev_Open
        sceneNames.Add("dev_Open");

        scenes.Add(3); // NAME: dev_Pillars
        sceneNames.Add("dev_Pillars");

        scenes.Add(4); // NAME: dev_House
        sceneNames.Add("dev_House");

        scenes.Add(5); // NAME: dev_Maze
        sceneNames.Add("dev_Maze");

        scenes.Add(6); // NAME: dev_Desert
        sceneNames.Add("dev_Desert");

        scenes.Add(7); // NAME: dev_Dungeon
        sceneNames.Add("dev_Dungeon");

        scenes.Add(8); // NAME: dev_School(OLD)
        sceneNames.Add("dev_School(OLD)");

////////////////////// ENDED SCENES //////////////////////

        ReplayLevelButton = GameObject.Find("ReplayLevelButton").GetComponent<Button>();
        SelectLevel1 = GameObject.Find("SelectLevel1").GetComponent<Button>();
        SelectLevel2 = GameObject.Find("SelectLevel2").GetComponent<Button>();
        SelectLevel3 = GameObject.Find("SelectLevel3").GetComponent<Button>();

        Player1Name = GameObject.Find("Player1Name").GetComponent<Text>();
        Player2Name = GameObject.Find("Player2Name").GetComponent<Text>();
        Player3Name = GameObject.Find("Player3Name").GetComponent<Text>();
        Player4Name = GameObject.Find("Player4Name").GetComponent<Text>();

        Player1Ping = GameObject.Find("Player1PingValue").GetComponent<Text>();
        Player2Ping = GameObject.Find("Player2PingValue").GetComponent<Text>();
        Player3Ping = GameObject.Find("Player3PingValue").GetComponent<Text>();
        Player4Ping = GameObject.Find("Player4PingValue").GetComponent<Text>();

        RoomName = GameObject.Find("RoomName").GetComponent<Text>();
        RoomName.text = PhotonNetwork.room.name;
        StatusName = GameObject.Find("StatusName").GetComponent<Text>();
        LevelName = GameObject.Find("LevelName").GetComponent<Text>();

        TimerTime = GameObject.Find("TimerTime").GetComponent<Text>();
        TimerTime.text = voteTimer.ToString();
        TimerText = GameObject.Find("TimerText").GetComponent<Text>();

        if (PhotonNetwork.playerList.Length == 1)
        {
            Debug.Log("I am the first player on the menu");
        }
        else
        {
            Debug.Log("I am NOT the first player on the menu");
        }

        // Master Client Generate Map Selections, Clients Update buttons
        masterGetRandomLevels();
        updateLevelSelect();

        updatePlayerCustomPrefs();
    }

    // Update is called once per frame
    void Update()
    {
        // Update always
        if (statusNameString == "Voting")
        {
            startOrUpdateVoteTimer();
        }
        else if (statusNameString == "Launching" || statusNameString == "In Game")
        {
            startOrUpdateLaunchTimer();
        }
        updatePlayerCustomPrefs();

        //has everyone voted?
        hasEveryoneVoted();

        // Update once per second
        if (Time.time - lastUpdateTime > 1.0f)
        {
            lastUpdateTime = Time.time;

            updateRoomInfo();
            updatePlayersOnScoreboard();
            updateLevelSelect();
        }        
    }
    
    void masterGetRandomLevels()
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (lastScene == 1) // WE HAVE NOT PLAYED YET, FIRST VOTE
            {
                Debug.Log("We haven't played before, generate 4 rand maps");

                // Init random to whatever scene is already in the list
                randHolder = Random.Range(2, scenes.Count + 2);
                scenesToVote.Add(randHolder);

                // Run this three times
                for (int i = 0; i < 3; i++)
                {
                    //    // Run until you generate a random number not already in the list
                    while (scenesToVote.Contains(randHolder))
                    {
                        randHolder = Random.Range(2, scenes.Count + 2); // Plus 2 because 2 is the base that we started with         
                    }

                    // now that we have a random not in the list, add it to the scene
                    scenesToVote.Add(randHolder);
                }
            }
            else // WE HAVE PLAYED ALREADY, NOT FIRST VOTE
            {
                Debug.Log("We haveplayed before, generate 3 rand maps");
                /// First scene must be last scene
                scenesToVote.Add(lastScene);
                //// Init random to whatever scene is already in the list
                randHolder = lastScene;
                // Run this three times
                for (int i = 0; i < 3; i++)
                {
                    // Run until you generate a random number not already in the list
                    while (scenesToVote.Contains(randHolder))
                    {
                        randHolder = Random.Range(2, scenes.Count + 2); // Plus 2 because 2 is the base that we started with         
                    }

                    // now that we have a random not in the list, add it to the scene
                    scenesToVote.Add(randHolder);
                }
            }

            updatePlayerCustomPrefs(scenesToVote[0], scenesToVote[1], scenesToVote[2], scenesToVote[3]);
            updatePlayerCustomPrefs(sceneNames[scenesToVote[0]], sceneNames[scenesToVote[1]], sceneNames[scenesToVote[2]], sceneNames[scenesToVote[3]]);
        }
        else
        {
            updateLevelSelect();
        }
    }

    void startOrUpdateVoteTimer()
    {
        if (PhotonNetwork.isMasterClient)
        {
            // Start Timer
            voteTimer -= Time.deltaTime;
            TimerTime.text = Mathf.Clamp(Mathf.Ceil(voteTimer), 0.0f, Mathf.Infinity).ToString();

            if (voteTimer <= 0.0f) // Or if vote is complete as all have voted
            {
                // Votes over
                TimerText.text = "Launching in:";
                statusNameString = "Launching";

                setNextLevel();
            }
        }
        else
        {
            // Update Timer
            // Get time from master
            voteTimer = (float)PhotonNetwork.masterClient.customProperties["VoteTimer"];
            TimerTime.text = Mathf.Clamp(Mathf.Ceil(voteTimer), 0.0f, Mathf.Infinity).ToString();

            if (voteTimer <= 0.0f)
            {
                // Votes over
                TimerText.text = "Launching in:";
            }
        }
    }
    void startOrUpdateLaunchTimer()
    {
        if (PhotonNetwork.isMasterClient) // If Master Client
        {
            // Start Timer
            launchTimer -= Time.deltaTime;
            TimerTime.text = Mathf.Clamp(Mathf.Ceil(launchTimer), 0.0f, Mathf.Infinity).ToString();

            if(LevelName.text == "Undetermined")
            {
                // If no vote, make next scene the replay scene
                if(PhotonNetwork.masterClient.customProperties["NextScene"] == null)
                {
                    updatePlayerCustomPrefs((int)playerCustomProps["scenesToVote1"], 0);
                }
                nextScene = (int)PhotonNetwork.masterClient.customProperties["NextScene"];
                LevelName.text = sceneNames[(int)PhotonNetwork.masterClient.customProperties["NextScene"]];

                if (GameObject.Find("ReplayLevelButton").GetComponentInChildren<Text>().text == LevelName.text)
                {
                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = Color.green;
                }
                else if (GameObject.Find("SelectLevel1").GetComponentInChildren<Text>().text == LevelName.text)
                {
                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = Color.green;
                }
                else if (GameObject.Find("SelectLevel2").GetComponentInChildren<Text>().text == LevelName.text)
                {
                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = Color.green;
                }
                else if (GameObject.Find("SelectLevel3").GetComponentInChildren<Text>().text == LevelName.text)
                {
                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = Color.green;
                }

                disableButtons();
            }

            if (launchTimer <= 0.0f)
            {
                // launching complete, switch scenes
                TimerText.text = "Traveling:";
                statusNameString = "In Game";
                
                playerCustomProps["SceneID"] = nextScene;

                // Open new scene
                //PhotonNetwork.automaticallySyncScene = true;
                Debug.Log("I am the master, leaving inRoomLobby JOINING SCENE: " + sceneNames[nextScene]);

                EmailDebugger.DebugLogAppend(null);
                EmailDebugger.DebugLogAppend("  Next Level: " + sceneNames[nextScene]);

                PhotonNetwork.LoadLevel(nextScene);
            }
        }
        else // If not master client
        {
            // Update Timer
            // Get time from master
            launchTimer = (float)PhotonNetwork.masterClient.customProperties["LaunchTimer"];
            TimerTime.text = Mathf.Clamp(Mathf.Ceil(launchTimer), 0.0f, Mathf.Infinity).ToString();

            if (LevelName.text == "Undetermined")
            {
                nextScene = (int)PhotonNetwork.masterClient.customProperties["NextScene"];
                LevelName.text = sceneNames[(int)PhotonNetwork.masterClient.customProperties["NextScene"]];

                if (GameObject.Find("ReplayLevelButton").GetComponentInChildren<Text>().text == LevelName.text)
                {
                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = Color.green;
                }
                else if (GameObject.Find("SelectLevel1").GetComponentInChildren<Text>().text == LevelName.text)
                {
                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = Color.green;
                }
                else if (GameObject.Find("SelectLevel2").GetComponentInChildren<Text>().text == LevelName.text)
                {
                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = Color.green;
                }
                else if (GameObject.Find("SelectLevel3").GetComponentInChildren<Text>().text == LevelName.text)
                {
                    GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

                    GameObject.Find("RandomLevel3").GetComponent<Image>().color = Color.green;
                }

                disableButtons();
            }

            if (launchTimer <= 0.0f)
            {
                // launching complete, switch scenes
                TimerText.text = "Traveling:";
                playerCustomProps["SceneID"] = nextScene;
                Debug.Log("I am a client, leaving inRoomLobby JOINING SCENE: " + sceneNames[nextScene]);

                EmailDebugger.DebugLogAppend(null);
                EmailDebugger.DebugLogAppend("  Next Level: " + sceneNames[nextScene]);

                PhotonNetwork.LoadLevel(nextScene);
            }
        }
    }

    void hasEveryoneVoted()
    {
        if(PhotonNetwork.isMasterClient)
        {
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                if (PhotonNetwork.playerList[i].customProperties["MyVote"] != null)
                {
                    if ((int)PhotonNetwork.playerList[i].customProperties["MyVote"] != -1)
                    {
                        checkVotesList.Add((int)PhotonNetwork.playerList[i].customProperties["MyVote"]);
                        if (checkVotesList.Count >= PhotonNetwork.playerList.Length)
                        {
                            voteTimer = 0.0f;
                        }
                    }
                }
            }

            checkVotesList.Clear();
        }
    }

    void updateLevelSelect()
    {
        // If we are not the master client, and the master has generated a list of scenes...
        updatePlayerCustomPrefs((int)PhotonNetwork.masterClient.customProperties["scenesToVote1"],
                                (int)PhotonNetwork.masterClient.customProperties["scenesToVote2"],
                                (int)PhotonNetwork.masterClient.customProperties["scenesToVote3"],
                                (int)PhotonNetwork.masterClient.customProperties["scenesToVote4"]);

        ReplayLevelButtonString = (string)PhotonNetwork.masterClient.customProperties["ReplayLevelButtonString"];
        SelectLevel1String = (string)PhotonNetwork.masterClient.customProperties["SelectLevel1String"];
        SelectLevel2String = (string)PhotonNetwork.masterClient.customProperties["SelectLevel2String"];
        SelectLevel3String = (string)PhotonNetwork.masterClient.customProperties["SelectLevel3String"];

        ReplayLevelButton.GetComponentInChildren<Text>().text = ReplayLevelButtonString;
        SelectLevel1.GetComponentInChildren<Text>().text = SelectLevel1String;
        SelectLevel2.GetComponentInChildren<Text>().text = SelectLevel2String;
        SelectLevel3.GetComponentInChildren<Text>().text = SelectLevel3String;

        updatePlayerCustomPrefs(ReplayLevelButtonString, SelectLevel1String, SelectLevel2String, SelectLevel3String);
    }
    
    void setNextLevel()
    {
        voteValues.Add(0); // Make sure that vote values has enough allocated space
        voteValues.Add(0); // Make sure that vote values has enough allocated space
        voteValues.Add(0); // Make sure that vote values has enough allocated space
        voteValues.Add(0); // Make sure that vote values has enough allocated space

        // Get all votes from all players. Stores 1,2,3, or 4 in a new List, voteList
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            if (PhotonNetwork.playerList[i].customProperties["MyVote"] != null)
            {
                voteList.Add((int)PhotonNetwork.playerList[i].customProperties["MyVote"]); // example, 1,2,2,3
            }
        }

        // compiles how many 1s,2s,3s, and 4s are in the voteList
        for (int i = 0; i < voteList.Count; i++)
        {
            if (voteList[i] != -1)
            {
                voteValues[voteList[i]]++; // example, 1,2,1
            }
        }

        // Output vote counts
        Debug.Log("VOTES;\nReplay: " + voteValues[0] + "\nOne:     " + voteValues[1] + "\nTwo:     " + voteValues[2] + "\nThree:   " + voteValues[3]);

        // find out which 1,2,3,4 has the most votes
        int mostVotes = 0;

        // ERROR: If masterclient has switched in lobby, this breaks.
        // Note, we are initializing nextScene to the first generated scene in the case of noVotes, if there are votes this will be overwritten.
        nextScene = (int)playerCustomProps["scenesToVote1"];

        for (int i = 0; i < voteValues.Count; i++)
        {
            //Debug.Log("Vote Counting: if " + voteValues[i] + " is > than " + mostVotes);
            if(voteValues[i] > mostVotes)
            {
                mostVotes = voteValues[i];
                //nextScene = scenesToVote[i];

                switch (i)
                {
                    case 0:
                        nextScene = (int)playerCustomProps["scenesToVote1"];
                        break;
                    case 1:
                        nextScene = (int)playerCustomProps["scenesToVote2"];
                        break;
                    case 2:
                        nextScene = (int)playerCustomProps["scenesToVote3"];
                        break;
                    case 3:
                        nextScene = (int)playerCustomProps["scenesToVote4"];
                        break;
                }
            }
        }

        //Debug.Log("ScenesToVote contains;");
        //for (int i = 0; i < scenesToVote.Count; i++)
        //{
        //    Debug.Log("scenesToVote[ " + i + " ] = " + scenesToVote[i]);
        //}
        //Debug.Log("NextLevel: " + nextScene);
        Debug.Log("NextLevel: " + nextScene + " (" + sceneNames[nextScene] + ")");

        updatePlayerCustomPrefs(nextScene, 0);
    }

    void updateRoomInfo()
    {
        statusNameString = (string)PhotonNetwork.masterClient.customProperties["StatusName"];
        StatusName.text = statusNameString;
    }

    void updatePlayersOnScoreboard()
    {
        for (int i = 0; i < 4; i++)
        {
            if (PhotonNetwork.playerList.Length > i)
            {
                if(PhotonNetwork.playerList[i] != null)
                {
                    switch (i + 1)
                    {
                        case 1:
                            Player1Name.text = PhotonNetwork.playerList[i].name;
                            if (PhotonNetwork.playerList[i].customProperties["Ping"] != null)
                            {
                                Player1Ping.text = PhotonNetwork.playerList[i].customProperties["Ping"].ToString();
                            }
                            break;
                        case 2:
                            Player2Name.text = PhotonNetwork.playerList[i].name;
                            if (PhotonNetwork.playerList[i].customProperties["Ping"] != null)
                            {
                                Player2Ping.text = PhotonNetwork.playerList[i].customProperties["Ping"].ToString();
                            }
                            break;
                        case 3:
                            Player3Name.text = PhotonNetwork.playerList[i].name;
                            if (PhotonNetwork.playerList[i].customProperties["Ping"] != null)
                            {
                                Player3Ping.text = PhotonNetwork.playerList[i].customProperties["Ping"].ToString();
                            }
                            break;
                        case 4:
                            Player4Name.text = PhotonNetwork.playerList[i].name;
                            if (PhotonNetwork.playerList[i].customProperties["Ping"] != null)
                            {
                                Player4Ping.text = PhotonNetwork.playerList[i].customProperties["Ping"].ToString();
                            }
                            break;
                    }
                }
            }            
            else
            {
                switch (i + 1)
                {
                    case 1:
                        Player1Name.text = "Not Connected";
                        Player1Ping.text = "-";
                        break;
                    case 2:
                        Player2Name.text = "Not Connected";
                        Player2Ping.text = "-";
                        break;
                    case 3:
                        Player3Name.text = "Not Connected";
                        Player3Ping.text = "-";
                        break;
                    case 4:
                        Player4Name.text = "Not Connected";
                        Player4Ping.text = "-";
                        break;
                }
            }            
        }
    }

    void OnPhotonPlayerConnected()
    {
        // Avoids a null reference caused when a player joins and does not have an updated list of all playerProps
        // Also guarentees all new players have correct info
        updatePlayerCustomPrefs();
    }

    void updatePlayerCustomPrefs()
    {
        playerCustomProps["VoteTimer"] = voteTimer;
        playerCustomProps["LaunchTimer"] = launchTimer;

        playerCustomProps["StatusName"] = statusNameString;
        playerCustomProps["Ping"] = PhotonNetwork.GetPing();

        playerCustomProps["SceneID"] = playerCustomProps["SceneID"];
        playerCustomProps["MyVote"] = playerCustomProps["MyVote"];
        playerCustomProps["NextScene"] = playerCustomProps["NextScene"];

        playerCustomProps["scenesToVote1"] = playerCustomProps["scenesToVote1"];
        playerCustomProps["scenesToVote2"] = playerCustomProps["scenesToVote2"];
        playerCustomProps["scenesToVote3"] = playerCustomProps["scenesToVote3"];
        playerCustomProps["scenesToVote4"] = playerCustomProps["scenesToVote4"];

        playerCustomProps["ReplayLevelButtonString"] = playerCustomProps["ReplayLevelButtonString"];
        playerCustomProps["SelectLevel1String"] = playerCustomProps["SelectLevel1String"];
        playerCustomProps["SelectLevel2String"] = playerCustomProps["SelectLevel2String"];
        playerCustomProps["SelectLevel3String"] = playerCustomProps["SelectLevel3String"];

        PhotonNetwork.player.SetCustomProperties(playerCustomProps);
    }
    void updatePlayerCustomPrefs(int MyVote)
    {
        playerCustomProps["VoteTimer"] = voteTimer;
        playerCustomProps["LaunchTimer"] = launchTimer;

        playerCustomProps["StatusName"] = statusNameString;
        playerCustomProps["Ping"] = PhotonNetwork.GetPing();

        playerCustomProps["SceneID"] = playerCustomProps["SceneID"];
        playerCustomProps["MyVote"] = MyVote;
        playerCustomProps["NextScene"] = playerCustomProps["NextScene"];

        playerCustomProps["scenesToVote1"] = playerCustomProps["scenesToVote1"];
        playerCustomProps["scenesToVote2"] = playerCustomProps["scenesToVote2"];
        playerCustomProps["scenesToVote3"] = playerCustomProps["scenesToVote3"];
        playerCustomProps["scenesToVote4"] = playerCustomProps["scenesToVote4"];

        playerCustomProps["ReplayLevelButtonString"] = playerCustomProps["ReplayLevelButtonString"];
        playerCustomProps["SelectLevel1String"] = playerCustomProps["SelectLevel1String"];
        playerCustomProps["SelectLevel2String"] = playerCustomProps["SelectLevel2String"];
        playerCustomProps["SelectLevel3String"] = playerCustomProps["SelectLevel3String"];

        PhotonNetwork.player.SetCustomProperties(playerCustomProps);
    }
    void updatePlayerCustomPrefs(int NextScene, int funcNumber)
    {
        playerCustomProps["VoteTimer"] = voteTimer;
        playerCustomProps["LaunchTimer"] = launchTimer;

        playerCustomProps["StatusName"] = statusNameString;
        playerCustomProps["Ping"] = PhotonNetwork.GetPing();

        playerCustomProps["SceneID"] = playerCustomProps["SceneID"];
        playerCustomProps["MyVote"] = playerCustomProps["MyVote"];
        playerCustomProps["NextScene"] = NextScene;

        playerCustomProps["scenesToVote1"] = playerCustomProps["scenesToVote1"];
        playerCustomProps["scenesToVote2"] = playerCustomProps["scenesToVote2"];
        playerCustomProps["scenesToVote3"] = playerCustomProps["scenesToVote3"];
        playerCustomProps["scenesToVote4"] = playerCustomProps["scenesToVote4"];

        playerCustomProps["ReplayLevelButtonString"] = playerCustomProps["ReplayLevelButtonString"];
        playerCustomProps["SelectLevel1String"] = playerCustomProps["SelectLevel1String"];
        playerCustomProps["SelectLevel2String"] = playerCustomProps["SelectLevel2String"];
        playerCustomProps["SelectLevel3String"] = playerCustomProps["SelectLevel3String"];

        PhotonNetwork.player.SetCustomProperties(playerCustomProps);
    }
    void updatePlayerCustomPrefs(string ReplayLevelButtonString, string SelectLevel1String, string SelectLevel2String, string SelectLevel3String)
    {
        playerCustomProps["VoteTimer"] = voteTimer;
        playerCustomProps["LaunchTimer"] = launchTimer;

        playerCustomProps["StatusName"] = statusNameString;
        playerCustomProps["Ping"] = PhotonNetwork.GetPing();

        playerCustomProps["SceneID"] = playerCustomProps["SceneID"];
        playerCustomProps["MyVote"] = playerCustomProps["MyVote"];
        playerCustomProps["NextScene"] = playerCustomProps["NextScene"];

        playerCustomProps["scenesToVote1"] = playerCustomProps["scenesToVote1"];
        playerCustomProps["scenesToVote2"] = playerCustomProps["scenesToVote2"];
        playerCustomProps["scenesToVote3"] = playerCustomProps["scenesToVote3"];
        playerCustomProps["scenesToVote4"] = playerCustomProps["scenesToVote4"];

        playerCustomProps["ReplayLevelButtonString"] = ReplayLevelButtonString;
        playerCustomProps["SelectLevel1String"] = SelectLevel1String;
        playerCustomProps["SelectLevel2String"] = SelectLevel2String;
        playerCustomProps["SelectLevel3String"] = SelectLevel3String;

        PhotonNetwork.player.SetCustomProperties(playerCustomProps);
    }
    void updatePlayerCustomPrefs(int scenesToVote1, int scenesToVote2, int scenesToVote3, int scenesToVote4)
    {
        playerCustomProps["VoteTimer"] = voteTimer;
        playerCustomProps["LaunchTimer"] = launchTimer;

        playerCustomProps["StatusName"] = statusNameString;
        playerCustomProps["Ping"] = PhotonNetwork.GetPing();

        playerCustomProps["SceneID"] = playerCustomProps["SceneID"];
        playerCustomProps["MyVote"] = playerCustomProps["MyVote"];
        playerCustomProps["NextScene"] = playerCustomProps["NextScene"];

        playerCustomProps["scenesToVote1"] = scenesToVote1;
        playerCustomProps["scenesToVote2"] = scenesToVote2;
        playerCustomProps["scenesToVote3"] = scenesToVote3;
        playerCustomProps["scenesToVote4"] = scenesToVote4;

        playerCustomProps["ReplayLevelButtonString"] = playerCustomProps["ReplayLevelButtonString"];
        playerCustomProps["SelectLevel1String"] = playerCustomProps["SelectLevel1String"];
        playerCustomProps["SelectLevel2String"] = playerCustomProps["SelectLevel2String"];
        playerCustomProps["SelectLevel3String"] = playerCustomProps["SelectLevel3String"];

        PhotonNetwork.player.SetCustomProperties(playerCustomProps);
    }

    // VOTE BUTTONS
    public void voteReplay()
    {
        Debug.Log("I vote 0");

        GameObject.Find("ReplayLevel").GetComponent<Image>().color = Color.yellow;
        GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f,1.000f,1.000f,0.392f);
        GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
        GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

        updatePlayerCustomPrefs(0);
        //playerCustomProps["MyVote"] = 0;
    }
    public void voteOne()
    {
        Debug.Log("I vote 1");

        Debug.Log("COLOR:" + GameObject.Find("ReplayLevel").GetComponent<Image>().color.ToString());

        GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
        GameObject.Find("RandomLevel1").GetComponent<Image>().color = Color.yellow;
        GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
        GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

        updatePlayerCustomPrefs(1);
        //playerCustomProps["MyVote"] = 1;
    }
    public void voteTwo()
    {
        Debug.Log("I vote 2");

        GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
        GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
        GameObject.Find("RandomLevel2").GetComponent<Image>().color = Color.yellow;
        GameObject.Find("RandomLevel3").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);

        updatePlayerCustomPrefs(2);
        //playerCustomProps["MyVote"] = 2;
    }
    public void voteThree()
    {
        Debug.Log("I vote 3");

        GameObject.Find("ReplayLevel").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
        GameObject.Find("RandomLevel1").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
        GameObject.Find("RandomLevel2").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.392f);
        GameObject.Find("RandomLevel3").GetComponent<Image>().color = Color.yellow;

        updatePlayerCustomPrefs(3);
        //playerCustomProps["MyVote"] = 3;
    }

    public void disableButtons()
    {
        ReplayLevelButton.interactable = false;
        SelectLevel1.interactable = false;
        SelectLevel2.interactable = false;
        SelectLevel3.interactable = false;
    }

    public void OnApplicationQuit()
    {
        // EDEBUG email sends on closing...
        EmailDebugger.DebugLogAppend(null);
        EmailDebugger.DebugLogAppend(" Application Quit");
        EmailDebugger.SendEmail();
    }
}
