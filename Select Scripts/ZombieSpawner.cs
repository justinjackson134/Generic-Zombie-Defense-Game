using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ZombieSpawner : Photon.MonoBehaviour
{
    // Zombie Prefab and clone
	public GameObject myZombie;

    // SpawnWave vars
    private int myZombiesToSpawn;

    // Spawn points
    private GameObject[] zombieSpawns = new GameObject[255];
    private int mySpawnRand = 0;

    [SerializeField]
    private Text gameStatusText;
    private float lastPrintTime;

    // Wave stuff
    private float lastZombieKilledTime;
    private float lastSpawnTime;
    private bool spawnWaveRunning = false;
    private int wave;
    private string updateMessage;

    // Use this for initialization
    void Start () 
	{
        // Find all zombie spawn points
        zombieSpawns = GameObject.FindGameObjectsWithTag("ZombieSpawnPoint");

        wave = 1;
        lastZombieKilledTime = Time.time;
        lastSpawnTime = Time.time;

        updateMessage = string.Empty;

        myZombiesToSpawn = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Q))
		{
            //myZombie = PhotonNetwork.InstantiateSceneObject("Crawler", new Vector3 (0, 10, 10), Quaternion.identity, 0, null);
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            //SpawnWave(0);
        }

        // Right now the last zombie killed time only delays the initial wave, it is never used anywhere else...
        if((GameObject.FindGameObjectsWithTag("Zombie").GetLength(0) <= 0) && !spawnWaveRunning)
        {
            if (Time.time - lastZombieKilledTime > 20.0f)
            {
                lastZombieKilledTime = Time.time;
            }           
            else if(Time.time - lastZombieKilledTime > 10.0f)
            {
                if (PhotonNetwork.isMasterClient || PhotonNetwork.playerList.Length == 1)
                {
                    updateMessage = "Wave " + wave + " Incoming";
                    photonView.RPC("UpdateStatusText", PhotonTargets.All, updateMessage);

                    //SpawnWave(wave);
                    spawnWaveRunning = true;
                    StartCoroutine(SpawnWaveCouroutine(wave));
                    wave++;
                    photonView.RPC("UpdateAllWaveNumbers", PhotonTargets.All, wave);
                }
            }
        }

        // Clear status text after 5 seconds
        if (Time.time - lastPrintTime > 5.0F)
        {
            gameStatusText.text = "";
        }

	}

    // Depreceated...
    void SpawnWave(int WaveNumber)
    {
        // Spawn wave number squared + 10 multiplied by the players present// NO PLAYER BASED MULTIPLIER FOR NOW.
        myZombiesToSpawn = (WaveNumber * WaveNumber + 10);

        while (myZombiesToSpawn > 0)
        {
            //if ((Time.time - lastSpawnTime) > 0.2f)
            //{
                lastSpawnTime = Time.time;

                // Select random spawn point
                mySpawnRand = Random.Range(0, (zombieSpawns.GetLength(0)));

                myZombie = PhotonNetwork.InstantiateSceneObject("Zombie", zombieSpawns[mySpawnRand].transform.position, new Quaternion(0, 0, 0, 0), 0, null);

                if ((wave >= 3) && (myZombiesToSpawn % 5 == 0))
                {
                    mySpawnRand = Random.Range(0, (zombieSpawns.GetLength(0)));
                    myZombie = PhotonNetwork.InstantiateSceneObject("Crawler", zombieSpawns[mySpawnRand].transform.position, new Quaternion(0, 0, 0, 0), 0, null);
                }
                if ((wave >= 6) && (myZombiesToSpawn % 15 == 0))
                {
                    mySpawnRand = Random.Range(0, (zombieSpawns.GetLength(0)));
                    myZombie = PhotonNetwork.InstantiateSceneObject("Tank", zombieSpawns[mySpawnRand].transform.position, new Quaternion(0, 0, 0, 0), 0, null);
                }

                myZombiesToSpawn--; 
            //}
        }
    }

    IEnumerator SpawnWaveCouroutine(int WaveNumber)
    {
        //Debug.Log("In spawn wave couroutine. WAVE: " + WaveNumber);
        EmailDebugger.DebugLogAppend("    Spawning Wave: " + WaveNumber);

        // Spawn wave number squared + 10 multiplied by the players present// NO PLAYER BASED MULTIPLIER FOR NOW.
        myZombiesToSpawn = (WaveNumber * WaveNumber + 10);

        while (myZombiesToSpawn > 0)
        {
            if(myZombiesToSpawn % 5 == 0)
            {
                //Debug.Log("Wait 1 sec (spawn 5 zeds +- others...");
                yield return new WaitForSeconds(1.0f);
            }
            

            //Debug.Log("SpawnLogic...");
            // Select random spawn point
            mySpawnRand = Random.Range(0, (zombieSpawns.GetLength(0)));

            myZombie = PhotonNetwork.InstantiateSceneObject("Zombie", zombieSpawns[mySpawnRand].transform.position, new Quaternion(0, 0, 0, 0), 0, null);

            if ((wave >= 3) && (myZombiesToSpawn % 5 == 0))
            {
                mySpawnRand = Random.Range(0, (zombieSpawns.GetLength(0)));
                myZombie = PhotonNetwork.InstantiateSceneObject("Crawler", zombieSpawns[mySpawnRand].transform.position, new Quaternion(0, 0, 0, 0), 0, null);
            }
            if ((wave >= 6) && (myZombiesToSpawn % 15 == 0))
            {
                mySpawnRand = Random.Range(0, (zombieSpawns.GetLength(0)));
                myZombie = PhotonNetwork.InstantiateSceneObject("Tank", zombieSpawns[mySpawnRand].transform.position, new Quaternion(0, 0, 0, 0), 0, null);
            }

            myZombiesToSpawn--;
        }

        spawnWaveRunning = false;
        yield return null;
    }

    // If a new player joined, tell them what wave it is!
    void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        photonView.RPC("UpdateAllWaveNumbers", PhotonTargets.All, wave);

        photonView.RPC("UpdateStatusText", PhotonTargets.All, "Player joined: " + other.name);
    }
    // This RPC should update new players to the current wave number so as to take over if the master client leaves the game
    [PunRPC]
    void UpdateAllWaveNumbers(int recievedWave)
    {
        if(recievedWave > wave)
        {
            wave = recievedWave;
        }
    }

    [PunRPC]
    void UpdateStatusText (string str)
    {
        lastPrintTime = Time.time;

        gameStatusText.text = str;
    }
}
