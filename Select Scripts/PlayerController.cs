using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : Photon.MonoBehaviour
{
    // My name
    string myName;

    // My objects
    private Rigidbody myRigidBody;
    private Camera myCamera;
    private AudioSource myAudioSource;
    private CapsuleCollider myCapsuleCollider;
    private GameObject myGunProp;
    private GameObject myFlashlight;

    // NetworkManager
    private GameObject sceneNetworkManager;

    // Variables
    // Player stats
    public float myHealth = 100.0F;

    // Player respawn vars...
    private GameObject player;

    // Player movement
    // Movement customization
    public float baseSpeedModifier = 0.85F;
    public float sprintModifier = 0.35F;
    public float myJumpVelocity = 5.0F;
    public float myIsGroundedHeight = 0.5F;
    private float calculatedSpeedModifier;
    public float mouseSensitivity = 3.0F;
    private float maxYAxisAngle = -90.0F;
    private float minYAxisAngle = 90.0F;
    private Vector3 cameraYRotation;
    // Local player physics based movement stuff
    Vector3 myTargetVelocity;
    Vector3 myVelocity;
    Vector3 myVelocityChange;
    public float myMaxVelocityChange = 1.0F;
    // Remote player universal vars
    private float lastMessageTime;
    private float timeToTravel;
    // Remote player movement
    private Vector3 endPosition;
    private Vector3 lastStoredPosition;
    private float moveToSpeed = 1.0F;
    // Remote player rotation
    private Quaternion endRotation;
    private Quaternion lastStoredRotation;
    private float rotateToSpeed = 1.0F;
    // Remote player pitch
    private Quaternion endPitch;
    private Quaternion lastStoredPitch;
    private float pitchToSpeed = 1.0F;
    // used for initilization?
    private Quaternion rotation;

    // Grenade handle
    private GameObject grenadeClone;
    private float grenadeDelay = 1.5F;

    // Player shoot info
    private RaycastHit myHitInfo;
    private int currentGun = 1; // 1: pistol, 2: shotgun, 3: automatic, 4: rifle
    private float shotDelayLifetime = 0.0F; // Time until we can shoot again - used for actions that limit all guns, aka, grenade tosses
    private float shotDelayLifetimePistol = 0.0F; // Time until we can shoot again
    private float shotDelayLifetimeShotgun = 0.0F; // Time until we can shoot again
    private float shotDelayLifetimeAutomatic = 0.0F; // Time until we can shoot again
    private float shotDelayLifetimeRifle = 0.0F; // Time until we can shoot again

    // Kick vars
    public float pistolKick = 1.0F;
    public float shotgunKick = 3.0F;
    public float automaticKick = 1.0F;
    public float rifleKick = 2.0F;

    // Reload vars
    public float pistolReloadTime = 1.0F;
    public float shotgunReloadTime = 4.0F;
    public float automaticReloadTime = 2.0F;
    public float rifleReloadTime = 3.0F;

    // FireRate vars
    public float pistolFireRate = 0.1F;
    public float shotgunFireRate = 0.5F;
    public float automaticFireRate = 0.1F;
    public float rifleFireRate = 0.0F;

    // Innaccuracy vars
    private float shotgunVerticalInnacuracy = 0.0F;
    private float shotgunRotationalInnacuracy = 0.0F;
    private Vector3 shotgunInnacurateDirection;
    private PhotonView[] shotgunHitsPhotonView = new PhotonView[8];
    private float[] shotgunHitsDamage = new float[8];

    // Rifle zoom stuffs
    [SerializeField]
    private float targetZoomFOV;
    private float defaultFOV;

    // Gun Sounds
    [SerializeField]
    private AudioClip pistolFire;
    [SerializeField]
    private AudioClip pistolReload;
    [SerializeField]
    private AudioClip shotgunFire;
    [SerializeField]
    private AudioClip shotgunReload;
    [SerializeField]
    private AudioClip automaticFire;
    [SerializeField]
    private AudioClip automaticReload;
    [SerializeField]
    private AudioClip rifleFire;
    [SerializeField]
    private AudioClip rifleReload;

    // Automatic fire selector
    private bool automaticKeepFiring = false; // 0, semi -> 1, full

    // Player Inventory
    private GameObject pistol;
    private GameObject pistolSlide;
    private GameObject pistolMuzzleFlash;
    private GameObject pistolMuzzleFlashLight;
    private GameObject shotgun;
    private GameObject shotgunSlide;
    private GameObject shotgunMuzzleFlash;
    private GameObject shotgunMuzzleFlashLight;
    private GameObject automatic;
    private GameObject automaticBolt;
    private GameObject automaticMuzzleFlash;
    private GameObject automaticMuzzleFlashLight;
    private GameObject rifle;
    private GameObject rifleBolt;
    private GameObject rifleMuzzleFlash;
    private GameObject rifleMuzzleFlashLight;

    // Ammo
    private int pistolAmmo = 120; // No longer infinite!
    private int pistolMag;
    public int pistolMagSize = 8;
    private int shotgunAmmo;
    private int shotgunMag;
    public int shotgunMagSize = 8;
    private int automaticAmmo;
    private int automaticMag;
    public int automaticMagSize = 30;
    private int rifleAmmo;
    private int rifleMag;
    public int rifleMagSize = 1;

    // OnPhotonSerializeView vars
    public float targetMessageDelayTime = 0.02F; // Send 50 updates a second
    private float messageDelayTimer = 0.0F;

    // Player UI
    private GameObject playerUI;
    private GameObject hitMarker;
    private float hitMarkerTimer = 0.0F;
    private float hitMarkerLifetime = 0.0F;
    private RectTransform healthBarLevel;
    private Text magCountUI;
    private Text ammoCountUI;
    private Text currentGunUI;
    private Image readyToFireImage;
    private GameObject scopeOverlay;
    private GameObject bloodOverlay1;
    private GameObject bloodOverlay2;
    private GameObject bloodOverlay3;
    private GameObject bloodOverlay4;
    private GameObject spawnOverlay;

    // Debugging
    // set in code
    private bool drawDebugRays = true;
    // set in editor
    public bool hasAllTheAmmo = true;
    public bool has120Ammo = false;
    public bool isGodMode = true;

    // Use this for initialization
    void Start()
    {        
        Debug.Log("PLAYER PREFS NAME (START): " + PlayerPrefs.GetString("PlayerName"));
        Debug.Log("PHOTON PLAYR NAME (START): " + PhotonNetwork.player.name.ToString());

        // DEBUGGING CHEATS
        if (hasAllTheAmmo)
        {
            pistolAmmo = int.MaxValue;
            shotgunAmmo = int.MaxValue;
            automaticAmmo = int.MaxValue;
            rifleAmmo = int.MaxValue;
        }
        if (has120Ammo)
        {
            pistolAmmo = 120;
            shotgunAmmo = 120;
            automaticAmmo = 120;
            rifleAmmo = 120;
        }
        if (isGodMode)
        {
            myHealth = Mathf.Infinity;
        }

        // Variable Inits
        // Scene objects
        sceneNetworkManager = GameObject.Find("NetworkManager");

        // Player Stats

        // Player game objects
        myRigidBody = GetComponent<Rigidbody>();
        myCapsuleCollider = GetComponent<CapsuleCollider>();
        myCamera = GetComponentInChildren<Camera>();
        defaultFOV = myCamera.fieldOfView;
        myAudioSource = GetComponent<AudioSource>();

        myFlashlight = myCamera.transform.GetChild(1).gameObject;
        myGunProp = myCamera.transform.GetChild(0).gameObject;

        pistol = myGunProp.transform.GetChild(0).gameObject;
        pistolSlide = myGunProp.transform.GetChild(0).GetChild(0).gameObject;
        pistolMuzzleFlash = myGunProp.transform.GetChild(0).GetChild(1).gameObject;
        pistolMuzzleFlashLight = myGunProp.transform.GetChild(0).GetChild(1).GetChild(1).gameObject;

        shotgun = myGunProp.transform.GetChild(1).gameObject;
        shotgunSlide = shotgun.transform.GetChild(0).gameObject;
        shotgunMuzzleFlash = myGunProp.transform.GetChild(1).GetChild(1).gameObject;
        shotgunMuzzleFlashLight = myGunProp.transform.GetChild(1).GetChild(1).GetChild(1).gameObject;

        automatic = myGunProp.transform.GetChild(2).gameObject;
        automaticBolt = automatic.transform.GetChild(0).gameObject;
        automaticMuzzleFlash = myGunProp.transform.GetChild(2).GetChild(1).gameObject;
        automaticMuzzleFlashLight = myGunProp.transform.GetChild(2).GetChild(1).GetChild(1).gameObject;

        rifle = myGunProp.transform.GetChild(3).gameObject;
        rifleBolt = automatic.transform.GetChild(0).gameObject;
        rifleMuzzleFlash = myGunProp.transform.GetChild(3).GetChild(1).gameObject;
        rifleMuzzleFlashLight = myGunProp.transform.GetChild(3).GetChild(1).GetChild(1).gameObject;

        // Local player movement
        calculatedSpeedModifier = baseSpeedModifier;
        cameraYRotation = new Vector3(0.0F, 0.0F, 0.0F);

        // Remote player movement
        lastMessageTime = Time.time;
        timeToTravel = 0.1F;
        lastStoredPosition = transform.position;
        endPosition = transform.position;
        lastStoredRotation = transform.rotation;
        endRotation = transform.rotation;
        lastStoredPitch = myCamera.transform.rotation;
        endPitch = myCamera.transform.rotation;

        // Shtgun inaccuracy stuffs
        // Make sure arrays init to nulls...
        System.Array.Clear(shotgunHitsPhotonView, 0, shotgunHitsPhotonView.Length);
        System.Array.Clear(shotgunHitsDamage, 0, shotgunHitsDamage.Length);

        // Player Inventory
        // Start with pistol loaded! only load the rest if using HASALLTHEAMMO 0r HAS120AMMO
        pistolMag = pistolMagSize;
        pistolAmmo -= pistolMag;
        if (hasAllTheAmmo )
        {
            shotgunMag = shotgunMagSize;
            automaticMag = automaticMagSize;
            rifleMag = rifleMagSize;
        }
        else if (has120Ammo)
        {
            shotgunMag = shotgunMagSize;
            shotgunAmmo -= shotgunMag;

            automaticMag = automaticMagSize;
            automaticAmmo -= automaticMag;

            rifleMag = rifleMagSize;
            rifleAmmo -= rifleMag;
        }

        // Player UI
        if (photonView.isMine)
        {
            playerUI = GameObject.Find("PlayerUI");
            hitMarker = GameObject.Find("HitMarker");
            healthBarLevel = GameObject.Find("HealthLevel").GetComponent<RectTransform>();
            magCountUI = GameObject.Find("MagCount").GetComponent<Text>();
            ammoCountUI = GameObject.Find("AmmoCount").GetComponent<Text>();
            currentGunUI = GameObject.Find("CurrentGunUI").GetComponent<Text>();
            readyToFireImage = GameObject.Find("ReadyToFireGreen").GetComponent<Image>();
            scopeOverlay = GameObject.Find("ScopeOverlay");
            bloodOverlay1 = GameObject.Find("BloodOverlay1");
            bloodOverlay2 = GameObject.Find("BloodOverlay2");
            bloodOverlay3 = GameObject.Find("BloodOverlay3");
            bloodOverlay4 = GameObject.Find("BloodOverlay4");
            spawnOverlay = GameObject.Find("SpawnOverlay");

            // Disable all overlays on start
            scopeOverlay.SetActive(false);
            bloodOverlay1.SetActive(false);
            bloodOverlay2.SetActive(false);
            bloodOverlay3.SetActive(false);
            bloodOverlay4.SetActive(false);
            spawnOverlay.SetActive(false);
        }
    }

    void OnPhotonPlayerConnected()
    {
        // FINALLY, UPDATE ANY AND ALL PREVIOUSLY CONNECTED PLAYERS!
        // somehow....

        // AFTER HOURS AND HOURS, it appears that my code was good all along, however, if the new player asking for updates hadn't warmed up yet,
        // this would all break. By using a coroutine, we let the player think about things for a bit, aka, initialize all of his game objects

        // Might want to implement a is initialized var in the start function that would let this coroutine know if its safe to update the existing players
        StartCoroutine(updateExistingPlayers());
    }

    IEnumerator updateExistingPlayers()
    {
        yield return new WaitForSeconds(2.0F);

        // Gun works!
        photonView.RPC("playerChangeGunUpdate", PhotonTargets.All, currentGun);

        // Flashlight works
        photonView.RPC("playerToggleFlashlightUpdate", PhotonTargets.All, !myFlashlight.GetActive());

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        playerMove();

        if (photonView.isMine)
        {
            playerReload();
            playerChangeGun();
            playerToggleFlashlight();

            // player throw before shoot to stop player from shooting and throwing in the same frame via shotDelayLifetime
            playerPickup();
            playerThrow();
            playerShoot();

            // Temp Health regen
            if (myHealth < 100.0f)
            {
                myHealth += (2 * Time.deltaTime);

                if (myHealth > 100.0f)
                {
                    myHealth = 100.0f;
                }
            }

            playerDidIDie();
            playerUpdateUI();
        }
    }

    void playerMove()
    {
        if (photonView.isMine)
        {
            moveLocalPlayer();
        }
        else
        {
            moveRemotePlayer();
        }
    }
    void moveLocalPlayer()
    {
        // Reset modifier
        calculatedSpeedModifier = baseSpeedModifier;

        // Change modifier
        // Are we moving in two directions?
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0 && Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
        {
            calculatedSpeedModifier = (baseSpeedModifier * 0.65F);
        }
        // Are we sprinting? AND not going backwards?
        if ((Mathf.Abs(Input.GetAxis("Sprint")) > 0) && (Input.GetAxis("Vertical") >= 0))
        {
            calculatedSpeedModifier = calculatedSpeedModifier * sprintModifier;
        }

        // Forward, Backward, Right, Left motion
        Vector3 myTargetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0.0F, Input.GetAxis("Vertical"));
        myTargetVelocity = transform.TransformDirection(myTargetVelocity);
        myTargetVelocity *= calculatedSpeedModifier;

        Vector3 myVelocity = myRigidBody.velocity;
        Vector3 myVelocityChange = (myTargetVelocity - myVelocity);
        myVelocityChange.x = Mathf.Clamp(myVelocityChange.x, -myMaxVelocityChange, myMaxVelocityChange);
        myVelocityChange.z = Mathf.Clamp(myVelocityChange.z, -myMaxVelocityChange, myMaxVelocityChange);
        myVelocityChange.y = 0;
        myRigidBody.AddForce(myVelocityChange, ForceMode.VelocityChange);

        /*
        // Forward/Backward motion
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0)
        {
            //transform.Translate(0.0F, 0.0F, 7.0F * Input.GetAxis("Vertical") * calculatedSpeedModifier * Time.deltaTime);
        }
        // Horizontal motion
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
        {
            //transform.Translate(6.0F * Input.GetAxis("Horizontal") * calculatedSpeedModifier * Time.deltaTime, 0.0F, 0.0F);
        }
        */

        // Jump
        if (Mathf.Abs(Input.GetAxis("Jump")) > 0)
        {
            // If Grounded we can control ourselves
            if (Physics.Raycast(transform.position, -Vector3.up, GetComponent<Collider>().bounds.extents.y + myIsGroundedHeight))
            {
                myRigidBody.velocity = new Vector3(myRigidBody.velocity.x, myJumpVelocity, myRigidBody.velocity.z);
            }
        }

        // Pitch Camera (Clamp between maxYAxisAngle and minYAxisAngle)
        cameraYRotation.x += Input.GetAxisRaw("Mouse Y") * -1.0F * mouseSensitivity;
        cameraYRotation.x = Mathf.Clamp(cameraYRotation.x, maxYAxisAngle, minYAxisAngle);
        myCamera.transform.localEulerAngles = cameraYRotation;

        // Rotate Character
        transform.Rotate(0.0F, Input.GetAxisRaw("Mouse X") * mouseSensitivity, 0.0f);
    }
    void moveRemotePlayer()
    {
        // Handle movement
        if (endPosition == lastStoredPosition)
        {
            // Move clone towards end position
            transform.position = Vector3.MoveTowards(transform.position, endPosition, moveToSpeed * Time.deltaTime);
        }
        else
        {
            // Handles remote destination setting
            endPosition = lastStoredPosition;
            // Calculate Lerp speeds
            moveToSpeed = (Vector3.Distance(transform.position, endPosition) / timeToTravel);
        }

        // Handle rotation
        if (endRotation == lastStoredRotation)
        {
            // Rotate towards target rotation
            //transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, 10);    
            transform.rotation = Quaternion.RotateTowards(transform.rotation, endRotation, rotateToSpeed * Time.deltaTime);
        }
        else
        {
            // Handles remote rotation setting
            endRotation = lastStoredRotation;
            // Calculate Lerp speeds
            rotateToSpeed = (((transform.rotation.y - endRotation.y) * 360) / timeToTravel);
            if (rotateToSpeed < 0)
            {
                rotateToSpeed = rotateToSpeed * -1;
            }
        }

        // Handle pitch
        if (endPitch == lastStoredPitch)
        {
            // Rotate towards target rotation
            //transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, 10);    
            myCamera.transform.rotation = Quaternion.RotateTowards(myCamera.transform.rotation, endPitch, pitchToSpeed * Time.deltaTime);
        }
        else
        {
            // Handles remote pitch setting
            endPitch = lastStoredPitch;
            // Calculate Lerp speeds
            pitchToSpeed = (((myCamera.transform.rotation.x - endPitch.x) * 360) / timeToTravel);
            if (pitchToSpeed < 0)
            {
                pitchToSpeed = pitchToSpeed * -1;
            }
        }
    }

    void playerReload()
    {
        if (Input.GetButtonDown("Reload"))
        {
            switch (currentGun)
            {
                case 1:
                    // Full reload
                    if ((pistolMag < pistolMagSize) && (pistolAmmo > (pistolMagSize - pistolMag)))
                    {
                        pistolAmmo -= (pistolMagSize - pistolMag);
                        pistolMag = pistolMagSize;
                        shotDelayLifetime = pistolReloadTime;

                        // Make noise!
                        myAudioSource.PlayOneShot(pistolReload);
                    }
                    // Partial reload
                    else if (pistolMag < pistolMagSize)
                    {
                        pistolMag = (pistolAmmo + pistolMag);
                        pistolAmmo = 0;
                        shotDelayLifetime = pistolReloadTime;

                        // Make noise!
                        myAudioSource.PlayOneShot(pistolReload);
                    }
                    break;

                case 2:
                    // Full reload
                    if ((shotgunMag < shotgunMagSize) && (shotgunAmmo > (shotgunMagSize - shotgunMag)))
                    {
                        shotgunAmmo -= (shotgunMagSize-shotgunMag);
                        shotgunMag = shotgunMagSize;
                        shotDelayLifetime = pistolReloadTime;

                        // Make noise!
                        myAudioSource.PlayOneShot(shotgunReload);
                    }
                    // Partial reload
                    else if (shotgunMag < shotgunMagSize)
                    {
                        shotgunMag = (shotgunAmmo + shotgunMag);
                        shotgunAmmo = 0;
                        shotDelayLifetime = pistolReloadTime;

                        // Make noise!
                        myAudioSource.PlayOneShot(shotgunReload);
                    }
                    break;

                case 3:
                    // Full reload
                    if ((automaticMag < automaticMagSize) && (automaticAmmo > (automaticMagSize - automaticMag)))
                    {
                        automaticAmmo -= (automaticMagSize - automaticMag);
                        automaticMag = automaticMagSize;
                        shotDelayLifetime = shotgunReloadTime;

                        // Make noise!
                        myAudioSource.PlayOneShot(automaticReload);
                    }
                    // Partial reload
                    else if (automaticMag < automaticMagSize)
                    {
                        automaticMag = (automaticAmmo + automaticMag);
                        automaticAmmo = 0;
                        shotDelayLifetime = automaticReloadTime;

                        // Make noise!
                        myAudioSource.PlayOneShot(automaticReload);
                    }
                    break;

                case 4:
                    // Full reload
                    if ((rifleMag < rifleMagSize) && (rifleAmmo > (rifleMagSize - rifleMag)))
                    {
                        rifleAmmo -= (rifleMagSize - rifleMag);
                        rifleMag = rifleMagSize;
                        shotDelayLifetime = rifleReloadTime;

                        // Make noise!
                        myAudioSource.PlayOneShot(rifleReload);
                    }
                    // Partial reload
                    else if (rifleMag < rifleMagSize)
                    {
                        rifleMag = (rifleAmmo + rifleMag);
                        rifleAmmo = 0;
                        shotDelayLifetime = rifleReloadTime;

                        // Make noise!
                        myAudioSource.PlayOneShot(rifleReload);
                    }
                    break;
            }
        }
    }
    void playerChangeGun()
    {
        //Debug.Log("CHANGE GUN: SHOT DELAY LIFETIME = " + shotDelayLifetime);
        if (shotDelayLifetime > 0.0F) // IF WE STARTED RELOADING, WERE STUCK!
        {
            // DELAYED! No weapon swap while reloading!
            //Debug.Log("DELAYED!");
        }
        else
        {
            //Debug.Log("Allowed!");
            if (Input.GetButtonDown("Gun1"))
            {
                photonView.RPC("playerChangeGunUpdate", PhotonTargets.All, 1);
            }
            else if (Input.GetButtonDown("Gun2"))
            {
                photonView.RPC("playerChangeGunUpdate", PhotonTargets.All, 2);
            }
            else if (Input.GetButtonDown("Gun3"))
            {
                photonView.RPC("playerChangeGunUpdate", PhotonTargets.All, 3);
            }
            else if (Input.GetButtonDown("Gun4"))
            {
                photonView.RPC("playerChangeGunUpdate", PhotonTargets.All, 4);
            }
        }
    }
    [PunRPC]
    void playerChangeGunUpdate(int gun)
    {
        if (gun == 1)
        {
            currentGun = 1;
            pistol.SetActive(true);
            shotgun.SetActive(false);
            automatic.SetActive(false);
            rifle.SetActive(false);
        }
        else if (gun == 2)
        {
            currentGun = 2;
            pistol.SetActive(false);
            shotgun.SetActive(true);
            automatic.SetActive(false);
            rifle.SetActive(false);
        }
        else if (gun == 3)
        {
            currentGun = 3;
            pistol.SetActive(false);
            shotgun.SetActive(false);
            automatic.SetActive(true);
            rifle.SetActive(false);
        }
        else if (gun == 4)
        {
            currentGun = 4;
            pistol.SetActive(false);
            shotgun.SetActive(false);
            automatic.SetActive(false);
            rifle.SetActive(true);
        }
    }

    void playerToggleFlashlight()
    {
        if (Input.GetButtonDown("Flashlight"))
        {
            photonView.RPC("playerToggleFlashlightUpdate", PhotonTargets.All, myFlashlight.GetActive());
        }

    }
    [PunRPC]
    void playerToggleFlashlightUpdate(bool state)
    {
        if (myFlashlight != null)
        {
            myFlashlight.SetActive(!state);
        }
    }

    void playerPickup()
    {
        if (Input.GetButtonDown("Interact"))
        {
            if (Physics.Raycast(myCamera.transform.position, myCamera.transform.TransformDirection(Vector3.forward), out myHitInfo, 2.0F))
            {
                Debug.Log("Object To Interact: " + myHitInfo.collider.tag.ToString() + " - " + myHitInfo.transform.name);

                if (myHitInfo.collider.tag == "Pickup")
                {
                    Debug.Log("Object Was A Pickup");

                    Debug.Log("OBJECT: " + myHitInfo.collider.ToString());
                    Debug.Log("COUNT:  " + myHitInfo.collider.transform.root.GetComponent<RandomAmmoCount>().ammoCount);

                    if (myHitInfo.collider.transform.GetChild(0).tag == "PistolAmmo")
                    {
                        pistolAmmo += (pistolMagSize * myHitInfo.collider.transform.root.GetComponent<RandomAmmoCount>().ammoCount);
                    }
                    if (myHitInfo.collider.transform.GetChild(0).tag == "ShotgunAmmo")
                    {
                        shotgunAmmo += (shotgunMagSize * myHitInfo.collider.transform.root.GetComponent<RandomAmmoCount>().ammoCount);
                    }
                    if (myHitInfo.collider.transform.GetChild(0).tag == "AutomaticAmmo")
                    {
                        automaticAmmo += (automaticMagSize * myHitInfo.collider.transform.root.GetComponent<RandomAmmoCount>().ammoCount);
                    }
                    if (myHitInfo.collider.transform.GetChild(0).tag == "RifleAmmo")
                    {
                        rifleAmmo += (rifleMagSize * myHitInfo.collider.transform.root.GetComponent<RandomAmmoCount>().ammoCount);
                    }
                    if (myHitInfo.collider.transform.GetChild(0).tag == "GrenadeAmmo")
                    {
                        // NO GRENADE AMMO YET!!!

                        // automaticAmmo += (automaticMagSize * myHitInfo.collider.transform.root.GetComponent<RandomAmmoCount>().ammoCount);
                    }

                    myHitInfo.collider.transform.root.GetComponent<PhotonView>().RPC("pickedUp", PhotonTargets.All);
                }
            }
        }
    }

    void playerThrow()
    {
        // Throw grenade if not delayed
        if (Input.GetButtonDown("Grenade") && shotDelayLifetime <= 0.0F)
        {
            grenadeClone = PhotonNetwork.Instantiate("Grenade", transform.position + myCamera.transform.forward, transform.rotation, 0);
            grenadeClone.GetComponent<Rigidbody>().velocity = (myRigidBody.velocity + (myCamera.transform.forward * 35.0F));
            grenadeClone.GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(-5.0F, 5.0F), Random.Range(-5.0F, 5.0F), Random.Range(-5.0F, 5.0F)), ForceMode.VelocityChange);

            // add to shot delay
            if (shotDelayLifetime < 0.0F)
            {
                shotDelayLifetime = grenadeDelay;
            }
            else
            {
                shotDelayLifetime += grenadeDelay;
            }
        }
    }

    void playerShoot()
    {
        if (hitMarker != null)
        {
            // Reset hit marker (toggle off)
            if (hitMarker.GetActive())
            {
                hitMarkerLifetime = Time.time - hitMarkerTimer;
            }
            if (hitMarkerLifetime > 0.1F)
            {
                hitMarker.SetActive(false);
                hitMarkerLifetime = 0;
            }
        }

        // Reset gun moving parts
        resetGunPositions();

        // Decrease shot delay timer...
        //Debug.Log("Shot delay remaining: " + shotDelayLifetime);
        shotDelayLifetime = shotDelayLifetime - Time.deltaTime;
        shotDelayLifetimePistol = shotDelayLifetimePistol - Time.deltaTime;
        shotDelayLifetimeShotgun = shotDelayLifetimeShotgun - Time.deltaTime;
        shotDelayLifetimeAutomatic = shotDelayLifetimeAutomatic - Time.deltaTime;
        shotDelayLifetimeRifle = shotDelayLifetimeRifle - Time.deltaTime;

        // Aim down sights
        aimDownSights();

        // Shoot?
        if (Input.GetButtonDown("Fire1"))
        {
            if (shotDelayLifetime > 0.0F)
            {
                // DELAYED, NO SHOOT FOR YOU!
            }
            else
            {
                switch (currentGun)
                {
                    case 1:
                        if (shotDelayLifetimePistol <= 0.0F)
                        {
                            if (pistolMag > 0)
                            {
                                firePistol();
                                pistolMag--;

                                // Since we fired
                                playGunAnimation();
                            }
                        }
                        break;

                    case 2:
                        if (shotDelayLifetimeShotgun <= 0.0F)
                        {
                            if (shotgunMag > 0)
                            {
                                fireShotgun();
                                shotgunMag--;

                                // Since we fired
                                playGunAnimation();
                            }
                        }
                        break;

                    case 3:
                        // This function only handles semi-automatic fire, automatic is handled in the next onInput
                        if (!automaticKeepFiring)
                        {
                            if (shotDelayLifetimeAutomatic <= 0.0F)
                            {
                                if (automaticMag > 0)
                                {
                                    fireAutomatic();
                                    automaticMag--;

                                    // Since we fired
                                    playGunAnimation();
                                }
                            }
                        }
                        break;

                    case 4:
                        if (shotDelayLifetimeRifle <= 0.0F)
                        {
                            if (rifleMag > 0)
                            {
                                fireRifle();
                                rifleMag--;

                                // Since we fired
                                playGunAnimation();
                            }
                        }
                        break;
                }
            }
        }

        if (Input.GetButton("Fire1"))
        {
            // Only the automatic is allowed to be automatic!
            if (currentGun == 3)
            {
                if (automaticKeepFiring)
                {
                    if (shotDelayLifetime > 0.0F)
                    {
                        // DELAYED, NO SHOOT FOR YOU!
                    }
                    else
                    {
                        if (shotDelayLifetimeAutomatic <= 0.0F)
                        {
                            if (automaticMag > 0)
                            {
                                fireAutomatic();
                                automaticMag--;

                                // Since we fired
                                playGunAnimation();
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetButtonDown("FireSelector"))
        {
            automaticKeepFiring = !automaticKeepFiring;
        }
    }
    void firePistol()
    {
        if (Physics.Raycast(myCamera.transform.position, myCamera.transform.TransformDirection(Vector3.forward), out myHitInfo, Mathf.Infinity))
        {
            Debug.Log("Object Hit: " + myHitInfo.collider.tag.ToString() + " - " + myHitInfo.transform.name);

            if (myHitInfo.collider.tag == "Enemy")
            {
                hitMarker.SetActive(true);
                hitMarkerTimer = Time.time;
                myHitInfo.collider.transform.root.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 25.0F);
            }
            if (myHitInfo.collider.tag == "EnemyHead")
            {
                hitMarker.SetActive(true);
                hitMarkerTimer = Time.time;
                myHitInfo.collider.transform.root.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 100.0F);
            }
            //if (myHitInfo.collider.tag == "PlayerHitBox")
            if (myHitInfo.collider.tag == "Player")
            {
                // This is now a unique collider that ignores collision! Hopefully...
                Debug.Log(photonView.viewID + " shot " + myHitInfo.collider.GetComponentInParent<PhotonView>().viewID);

                // if not shooting self...
                if (!myHitInfo.collider.GetComponentInParent<PhotonView>().isMine)
                {
                    hitMarker.SetActive(true);
                    hitMarkerTimer = Time.time;
                    //myHitInfo.collider.GetComponentInParent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 25.0F);
                    myHitInfo.collider.GetComponentInParent<PhotonView>().RPC("TakeDamageFromPlayer", PhotonTargets.All, 25.0F, PlayerPrefs.GetString("PlayerName"));
                }
            }
        }

        shotDelayLifetimePistol = pistolFireRate;
    }
    void fireShotgun()
    {
        for (int pellets = 0; pellets < 8; pellets++)
        {
            // Calculate spread
            shotgunInnacurateDirection = Vector3.zero;
            shotgunVerticalInnacuracy = Random.Range(0, 0.15F);
            shotgunRotationalInnacuracy = Random.Range(0, 360);

            // Adjust and rotate
            shotgunInnacurateDirection.y = shotgunInnacurateDirection.y + shotgunVerticalInnacuracy;
            shotgunInnacurateDirection.x = ((Mathf.Cos(shotgunRotationalInnacuracy) * shotgunInnacurateDirection.x) + (Mathf.Sin(shotgunRotationalInnacuracy) * shotgunInnacurateDirection.y));
            shotgunInnacurateDirection.y = ((Mathf.Cos(shotgunRotationalInnacuracy) * shotgunInnacurateDirection.y) - (Mathf.Sin(shotgunRotationalInnacuracy) * shotgunInnacurateDirection.x));

            // Debugging view rays
            if (drawDebugRays)
            {
                Debug.DrawRay(myCamera.transform.position, myCamera.transform.TransformDirection(Vector3.forward * 100), Color.red, 5.0F);
                Debug.DrawRay(myCamera.transform.position, myCamera.transform.TransformDirection(Vector3.forward + shotgunInnacurateDirection) * 100, Color.green, 5.0F);
            }

            if (Physics.Raycast(myCamera.transform.position, myCamera.transform.TransformDirection(Vector3.forward + shotgunInnacurateDirection), out myHitInfo, Mathf.Infinity))
            {
                Debug.Log("Object Hit: " + myHitInfo.collider.tag.ToString() + " - " + myHitInfo.transform.name);

                if (myHitInfo.collider.tag == "Enemy")
                {
                    hitMarker.SetActive(true);
                    hitMarkerTimer = Time.time;

                    // add all hits to array
                    for (int i = 0; i < shotgunHitsPhotonView.Length; i++)
                    {
                        // adding new view
                        if (shotgunHitsPhotonView[i] == null)
                        {
                            //Debug.Log("Adding new view: enemy");
                            shotgunHitsPhotonView[i] = myHitInfo.collider.transform.root.GetComponent<PhotonView>();
                            shotgunHitsDamage[i] = shotgunHitsDamage[i] + 25.0F;
                            break;
                        }
                        // view exists
                        else if (shotgunHitsPhotonView[i].Equals(myHitInfo.collider.transform.root.GetComponent<PhotonView>()))
                        {
                            //Debug.Log("View Exists");
                            shotgunHitsDamage[i] = shotgunHitsDamage[i] + 25.0F;
                            break;
                        }
                    }
                }
                if (myHitInfo.collider.tag == "EnemyHead")
                {
                    hitMarker.SetActive(true);
                    hitMarkerTimer = Time.time;

                    // add all hits to array
                    for (int i = 0; i < shotgunHitsPhotonView.Length; i++)
                    {
                        // adding new view
                        if (shotgunHitsPhotonView[i] == null)
                        {
                            //Debug.Log("Adding new view: enemy head");
                            shotgunHitsPhotonView[i] = myHitInfo.collider.transform.root.GetComponent<PhotonView>();
                            shotgunHitsDamage[i] = shotgunHitsDamage[i] + 100.0F;
                            break;
                        }
                        // view exists
                        else if (shotgunHitsPhotonView[i].Equals(myHitInfo.collider.transform.root.GetComponent<PhotonView>()))
                        {
                            //Debug.Log("View Exists");
                            shotgunHitsDamage[i] = shotgunHitsDamage[i] + 100.0F;
                            break;
                        }
                    }
                }
                //if (myHitInfo.collider.tag == "PlayerHitBox")
                if (myHitInfo.collider.tag == "Player")
                {
                    // This is now a unique collider that ignores collision! Hopefully...
                    Debug.Log(photonView.viewID + " shot " + myHitInfo.collider.GetComponentInParent<PhotonView>().viewID);

                    // if not shooting self...
                    if (!myHitInfo.collider.GetComponentInParent<PhotonView>().isMine)
                    {
                        hitMarker.SetActive(true);
                        hitMarkerTimer = Time.time;
                        //myHitInfo.collider.GetComponentInParent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 25.0F);

                        // add all hits to array
                        for (int i = 0; i < shotgunHitsPhotonView.Length; i++)
                        {
                            // adding new view
                            if (shotgunHitsPhotonView[i] == null)
                            {
                                //Debug.Log("Adding new view: player");
                                shotgunHitsPhotonView[i] = myHitInfo.collider.GetComponent<PhotonView>();
                                shotgunHitsDamage[i] = shotgunHitsDamage[i] + 25.0F;
                                break;
                            }
                            // view exists
                            else if (shotgunHitsPhotonView[i].Equals(myHitInfo.collider.GetComponent<PhotonView>()))
                            {
                                //Debug.Log("View Exists");
                                shotgunHitsDamage[i] = shotgunHitsDamage[i] + 25.0F;
                                break;
                            }
                        }
                    }
                }
            }
        }

        // After calculating hits iterate through hashtabe and send one damage rpc per target
        //Debug.Log("ARRAY SIZE: " + shotgunHitsPhotonView.Length);
        for (int i = 0; i < shotgunHitsPhotonView.Length; i++)
        {
            if (shotgunHitsPhotonView[i] != null)
            {
                Debug.Log("Shotgun hit: " + shotgunHitsPhotonView[i].viewID + " for " + shotgunHitsDamage[i]);

                if (shotgunHitsPhotonView[i].tag == "Player")
                {
                    shotgunHitsPhotonView[i].RPC("TakeDamageFromPlayer", PhotonTargets.All, shotgunHitsDamage[i], PlayerPrefs.GetString("PlayerName"));
                }
                else
                {
                    shotgunHitsPhotonView[i].RPC("TakeDamage", PhotonTargets.All, shotgunHitsDamage[i]);
                }
            }
        }

        System.Array.Clear(shotgunHitsPhotonView, 0, shotgunHitsPhotonView.Length);
        System.Array.Clear(shotgunHitsDamage, 0, shotgunHitsDamage.Length);

        shotDelayLifetimeShotgun = shotgunFireRate;
    }
    void fireAutomatic()
    {
        if (Physics.Raycast(myCamera.transform.position, myCamera.transform.TransformDirection(Vector3.forward), out myHitInfo, Mathf.Infinity))
        {
            Debug.Log("Object Hit: " + myHitInfo.collider.tag.ToString() + " - " + myHitInfo.transform.name);

            if (myHitInfo.collider.tag == "Enemy")
            {
                hitMarker.SetActive(true);
                hitMarkerTimer = Time.time;
                myHitInfo.collider.transform.root.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 20.0F);
            }
            if (myHitInfo.collider.tag == "EnemyHead")
            {
                hitMarker.SetActive(true);
                hitMarkerTimer = Time.time;
                myHitInfo.collider.transform.root.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 100.0F);
            }
            //if (myHitInfo.collider.tag == "PlayerHitBox")
            if (myHitInfo.collider.tag == "Player")
            {
                // This is now a unique collider that ignores collision! Hopefully...
                Debug.Log(photonView.viewID + " shot " + myHitInfo.collider.GetComponentInParent<PhotonView>().viewID);

                // if not shooting self...
                if (!myHitInfo.collider.GetComponentInParent<PhotonView>().isMine)
                {
                    hitMarker.SetActive(true);
                    hitMarkerTimer = Time.time;
                    //myHitInfo.collider.GetComponentInParent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 20.0F);
                    myHitInfo.collider.GetComponentInParent<PhotonView>().RPC("TakeDamageFromPlayer", PhotonTargets.All, 20.0F, PlayerPrefs.GetString("PlayerName"));
                }
            }
        }

        shotDelayLifetimeAutomatic = automaticFireRate;
    }
    void fireRifle()
    {
        if (Physics.Raycast(myCamera.transform.position, myCamera.transform.TransformDirection(Vector3.forward), out myHitInfo, Mathf.Infinity))
        {
            Debug.Log("Object Hit: " + myHitInfo.collider.tag.ToString() + " - " + myHitInfo.transform.name);

            if (myHitInfo.collider.tag == "Enemy")
            {
                hitMarker.SetActive(true);
                hitMarkerTimer = Time.time;
                myHitInfo.collider.transform.root.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 100.0F);
            }
            if (myHitInfo.collider.tag == "EnemyHead")
            {
                hitMarker.SetActive(true);
                hitMarkerTimer = Time.time;
                myHitInfo.collider.transform.root.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 200.0F);
            }
            //if (myHitInfo.collider.tag == "PlayerHitBox")
            if (myHitInfo.collider.tag == "Player")
            {
                // This is now a unique collider that ignores collision! Hopefully...
                Debug.Log(photonView.viewID + " shot " + myHitInfo.collider.GetComponentInParent<PhotonView>().viewID);

                // if not shooting self...
                if (!myHitInfo.collider.GetComponentInParent<PhotonView>().isMine)
                {
                    hitMarker.SetActive(true);
                    hitMarkerTimer = Time.time;
                    //myHitInfo.collider.GetComponentInParent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 100.0F);
                    myHitInfo.collider.GetComponentInParent<PhotonView>().RPC("TakeDamageFromPlayer", PhotonTargets.All, 100.0F, PlayerPrefs.GetString("PlayerName"));
                }
            }
        }

        shotDelayLifetimeRifle = rifleFireRate;
    }

    void aimDownSights()
    {
        // Zoom?
        // If not rifle, slide into zoom
        if ((Input.GetButton("Zoom")) && (currentGun != 4))
        {
            //myCamera.fieldOfView = defaultFOV-10;
            myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, (defaultFOV - 10), 0.25f);

            switch (currentGun)
            {
                case 1:
                    pistol.transform.localPosition = Vector3.MoveTowards(pistol.transform.localPosition, new Vector3(0.0F, -0.15f, 1.018822f), 5 * Time.deltaTime);
                    break;
                case 2:
                    shotgun.transform.localPosition = Vector3.MoveTowards(shotgun.transform.localPosition, new Vector3(0.0F, -0.04f, 1.1f), 5 * Time.deltaTime);
                    break;
                case 3:
                    automatic.transform.localPosition = Vector3.MoveTowards(automatic.transform.localPosition, new Vector3(0.0F, -0.07f, 1.22f), 5 * Time.deltaTime);
                    break;
            }
        }
        // If rifle, snap into zoom
        else if ((Input.GetButton("Zoom")) && (currentGun == 4))
        {
            myCamera.fieldOfView = targetZoomFOV;
            if (rifle.GetActive())
            {
                rifle.SetActive(false);

                // Enable Scope Overlay
                scopeOverlay.SetActive(true);
            }

        }
        // If was rifle, snap back
        else if (currentGun == 4)
        {
            myCamera.fieldOfView = defaultFOV; // This may not be the desired action here...
            if (!rifle.GetActive())
            {
                rifle.SetActive(true);

                // Disable Scope Overlay
                scopeOverlay.SetActive(false);
            }
        }
        // If wasnt rifle, slide out of zoom
        else
        {
            myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, defaultFOV, 0.25f);

            switch(currentGun)
            {
                case 1:
                    pistol.transform.localPosition = Vector3.MoveTowards(pistol.transform.localPosition, new Vector3(0.383F, -0.469f, 1.018822f), 5 * Time.deltaTime);
                    break;
                case 2:
                    shotgun.transform.localPosition = Vector3.MoveTowards(shotgun.transform.localPosition, new Vector3(0.1f, -0.22f, 0.89f), 5 * Time.deltaTime);
                    break;
                case 3:
                    automatic.transform.localPosition = Vector3.MoveTowards(automatic.transform.localPosition, new Vector3(0.17F, -0.28f, 1.22f), 5 * Time.deltaTime);
                    break;
            }            
        }
    }
    void resetGunPositions()
    {
        // We can technically check for player shooting after the player has been destroyed, so, do null checks
        if ((pistolSlide != null) && (pistolMag != 0) && (shotDelayLifetimePistol <= 0.1))
            pistolSlide.transform.localPosition = Vector3.MoveTowards(pistolSlide.transform.localPosition, new Vector3(0.00008964539F, -1, 0.0001907349F), 25 * Time.deltaTime);
        if ((shotgunSlide != null) && (shotgunMag != 0) && (shotDelayLifetimeShotgun <= 0.1))
            shotgunSlide.transform.localPosition = Vector3.MoveTowards(shotgunSlide.transform.localPosition, new Vector3(-0.0317F, 0.003F, -0.5258F), 1 * Time.deltaTime);
        if ((automaticBolt != null) && (automaticMag != 0) && (shotDelayLifetimeAutomatic <= 0.1))
            automaticBolt.transform.localPosition = Vector3.MoveTowards(automaticBolt.transform.localPosition, new Vector3(-0.013F, 0.0F, -0.1185F), 1 * Time.deltaTime);
        if ((rifleBolt != null) && (rifleMag != 0) && (shotDelayLifetimeRifle <= 0.1))
            rifleBolt.transform.localPosition = Vector3.MoveTowards(rifleBolt.transform.localPosition, new Vector3(-0.0F, 0.0F, -0.0139F), 1 * Time.deltaTime);
    }
    void playGunAnimation()
    {
        if (photonView.isMine)
        {
            // Gun animation
            if (currentGun == 1)
            {
                // play pistol animation
                if (pistolSlide != null)
                    pistolSlide.transform.localPosition = pistolSlide.transform.localPosition - new Vector3(-1.0F, 0, 0);

                // play muzzle flash
                if (pistolMuzzleFlash != null)
                    pistolMuzzleFlash.GetComponent<ParticleSystem>().Play();

                // turn on pistol light
                if (pistolMuzzleFlashLight != null)
                    pistolMuzzleFlashLight.SetActive(true);

                cameraYRotation.x -= pistolKick;
                // Smoothed
                myCamera.transform.localEulerAngles = Vector3.MoveTowards(myCamera.transform.localEulerAngles, cameraYRotation, pistolKick/5.0f);

                // Make noise!
                photonView.RPC("playSound", PhotonTargets.All, "pistolFire", transform.position);
            }
            else if (currentGun == 2)
            {
                // play shotgun animation
                if (shotgunSlide != null)
                    shotgunSlide.transform.localPosition = shotgunSlide.transform.localPosition - new Vector3(0, 0, 0.1F);

                // play muzzle flash
                if (shotgunMuzzleFlash != null)
                    shotgunMuzzleFlash.GetComponent<ParticleSystem>().Play();

                // turn on shotgun light
                if (shotgunMuzzleFlashLight != null)
                    shotgunMuzzleFlashLight.SetActive(true);

                cameraYRotation.x -= shotgunKick;
                // myCamera.transform.localEulerAngles = cameraYRotation;
                // Smoothed
                myCamera.transform.localEulerAngles = Vector3.MoveTowards(myCamera.transform.localEulerAngles, cameraYRotation, shotgunKick / 5.0f);

                // Make noise!
                photonView.RPC("playSound", PhotonTargets.All, "shotgunFire", transform.position);
            }
            else if (currentGun == 3)
            {
                // play auto animation
                if (automaticBolt != null)
                    automaticBolt.transform.localPosition = automaticBolt.transform.localPosition - new Vector3(0, 0, 0.05F);

                // play muzzle flash
                if (automaticMuzzleFlash != null)
                    automaticMuzzleFlash.GetComponent<ParticleSystem>().Play();

                // turn on auto light
                if (automaticMuzzleFlashLight != null)
                    automaticMuzzleFlashLight.SetActive(true);

                cameraYRotation.x -= automaticKick;
                //myCamera.transform.localEulerAngles = cameraYRotation;
                // Smoothed
                myCamera.transform.localEulerAngles = Vector3.MoveTowards(myCamera.transform.localEulerAngles, cameraYRotation, automaticKick / 5.0f);

                // Make noise!
                photonView.RPC("playSound", PhotonTargets.All, "automaticFire", transform.position);
            }
            else if (currentGun == 4)
            {
                // play rifle animation
                if (rifleBolt != null)
                    rifleBolt.transform.localPosition = automaticBolt.transform.localPosition - new Vector3(0, 0, 0.09F);

                // play muzzle flash
                if (rifleMuzzleFlash != null)
                    rifleMuzzleFlash.GetComponent<ParticleSystem>().Play();

                // turn on rifle light
                if (rifleMuzzleFlashLight != null)
                    rifleMuzzleFlashLight.SetActive(true);

                cameraYRotation.x -= rifleKick;
                //myCamera.transform.localEulerAngles = cameraYRotation;
                // Smoothed
                myCamera.transform.localEulerAngles = Vector3.MoveTowards(myCamera.transform.localEulerAngles, cameraYRotation, rifleKick / 5.0f);

                // Make noise!
                photonView.RPC("playSound", PhotonTargets.All, "rifleFire", transform.position);
            }

            // Play flash for everyone else!
            photonView.RPC("playMuzzleFlash", PhotonTargets.Others, currentGun);
        }
    }
    [PunRPC]
    void playMuzzleFlash(int gun)
    {
        switch (gun)
        {
            case 1:
                // play muzzle flash
                if (pistolMuzzleFlash != null)
                    pistolMuzzleFlash.GetComponent<ParticleSystem>().Play();

                // turn on pistol light
                if (pistolMuzzleFlashLight != null)
                    pistolMuzzleFlashLight.SetActive(true);
                break;
            case 2:
                // play muzzle flash
                if (shotgunMuzzleFlash != null)
                    shotgunMuzzleFlash.GetComponent<ParticleSystem>().Play();

                // turn on shotgun light
                if (shotgunMuzzleFlashLight != null)
                    shotgunMuzzleFlashLight.SetActive(true);
                break;
            case 3:
                // play muzzle flash
                if (automaticMuzzleFlash != null)
                    automaticMuzzleFlash.GetComponent<ParticleSystem>().Play();

                // turn on auto light
                if (automaticMuzzleFlashLight != null)
                    automaticMuzzleFlashLight.SetActive(true);
                break;
            case 4:
                // play muzzle flash
                if (rifleMuzzleFlash != null)
                    rifleMuzzleFlash.GetComponent<ParticleSystem>().Play();

                // turn on rifle light
                if (rifleMuzzleFlashLight != null)
                    rifleMuzzleFlashLight.SetActive(true);
                break;
        }
    }
    [PunRPC]
    void playSound(string sound, Vector3 soundPosition)
    {
        //Debug.Log("Playing: " + sound);
        switch (sound)
        {
            case "pistolFire":
                //myAudioSource.PlayOneShot(pistolFire);
                AudioSource.PlayClipAtPoint(pistolFire, soundPosition);
                break;
            case "shotgunFire":
                //myAudioSource.PlayOneShot(shotgunFire);
                AudioSource.PlayClipAtPoint(shotgunFire, soundPosition);
                break;
            case "automaticFire":
                //myAudioSource.PlayOneShot(automaticFire);
                AudioSource.PlayClipAtPoint(automaticFire, soundPosition);
                break;
            case "rifleFire":
                //myAudioSource.PlayOneShot(rifleFire);
                AudioSource.PlayClipAtPoint(rifleFire, soundPosition);
                break;
        }
    }

    void playerUpdateUI()
    {
        // Temporary health bar
        if (!isGodMode)
        {
            healthBarLevel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, myHealth);
        }

        // Set fire indicator to false
        readyToFireImage.enabled = false;

        switch (currentGun)
        {
            case 1:
                magCountUI.text = "" + pistolMag;
                ammoCountUI.text = "" + pistolAmmo;

                if (pistolMag > 0 && shotDelayLifetime <= 0.0F)
                {
                    readyToFireImage.enabled = true;
                }
                break;
            case 2:
                magCountUI.text = "" + shotgunMag;
                ammoCountUI.text = "" + shotgunAmmo;

                if (shotgunMag > 0 && shotDelayLifetime <= 0.0F)
                {
                    readyToFireImage.enabled = true;
                }
                break;
            case 3:
                magCountUI.text = "" + automaticMag;
                ammoCountUI.text = "" + automaticAmmo;

                if (automaticMag > 0 && shotDelayLifetime <= 0.0F)
                {
                    readyToFireImage.enabled = true;
                }
                break;
            case 4:
                magCountUI.text = "" + rifleMag;
                ammoCountUI.text = "" + rifleAmmo;

                if (rifleMag > 0 && shotDelayLifetime <= 0.0F)
                {
                    readyToFireImage.enabled = true;
                }
                break;
            default:
                break;
        }

        currentGunUI.text = "" + currentGun;
    }

    // Take Damage/Die/Respawn
    [PunRPC]
    void TakeDamage(float damage) // , string source add source eventually
    {
        if (myHealth <= 0)
        {
            // Guarentee that the player cant regen enough to survive beforethe death RPC is called
            myHealth -= 999;
        }

        Debug.Log(photonView.viewID + "'s old health: " + myHealth);
        myHealth -= damage;
        Debug.Log(photonView.viewID + "'s new health: " + myHealth);
    }
    [PunRPC]
    void TakeDamageFromPlayer(float damage, string damageSource) // , string source add source eventually
    {
        Debug.Log(photonView.viewID + "'s old health: " + myHealth);
        myHealth -= damage;
        Debug.Log(photonView.viewID + "'s new health: " + myHealth);

        if(myHealth <= 0)
        {
            if (photonView.isMine) // then im the player getting damaged
            {
                //Debug.Log("STRING:::: " + "" + PlayerPrefs.GetString("PlayerName") + " was killed by " + damageSource);
                string str = "" + PlayerPrefs.GetString("PlayerName") + " was killed by " + damageSource;
                GameObject.FindGameObjectWithTag("ZombieSpawner").GetComponent<PhotonView>().RPC("UpdateStatusText", PhotonTargets.All, str);

                // Guarentee that the player cant regen enough to survive beforethe death RPC is called
                myHealth -= 999;
            }
        }
    }
    void playerDidIDie()
    {
        // Health < 90
        if(myHealth < 90.0f)
        {
            bloodOverlay1.SetActive(true);
        }
        else
        {
            bloodOverlay1.SetActive(false);
        }

        // Health < 75        
        if (myHealth < 75.0f)
        {
            bloodOverlay2.SetActive(true);
        }
        else
        {
            bloodOverlay2.SetActive(false);
        }

        // Health < 50
        if (myHealth < 50.0f)
        {
            bloodOverlay3.SetActive(true);
        }
        else
        {
            bloodOverlay3.SetActive(false);
        }

        // Health < 25
        if (myHealth < 25.0f)
        {
            bloodOverlay4.SetActive(true);
        }
        else
        {
            bloodOverlay4.SetActive(false);
        }

        // If dead
        if (myHealth <= 0 && photonView.isMine)
        {
            Debug.Log(photonView.viewID + "Died!");

            //ERROR TEMP REPLACE SEARCH TERMS
            //Debug.Log(photonView.viewID + " is respawning");
            //sceneNetworkManager.SendMessage("respawnPlayer");

            Debug.Log(photonView.viewID + "is getting destroyed by the master");
            photonView.RPC("playerKillMe", PhotonTargets.All);

            // Make sure player death only occurs once
            myHealth = 1.0F;

            // Make sure to re-enable the hit marker and overlays! If disabled when player destroyed, then Bad juju. (If disabled next player clone wont be able to access it...)
            // SpawnOverlayfirst in order to black out screen temporarily...
            spawnOverlay.SetActive(true);
            hitMarker.SetActive(true);
            scopeOverlay.SetActive(true);
            bloodOverlay1.SetActive(true);
            bloodOverlay2.SetActive(true);
            bloodOverlay3.SetActive(true);
            bloodOverlay4.SetActive(true);
        }
    }
    [PunRPC]
    void playerKillMe()
    {
        if (photonView.isMine)
        {
            Debug.Log("CLIENT - " + photonView.viewID + ": I AM DESTROYING " + gameObject.GetComponent<PhotonView>().viewID);
            PhotonNetwork.Destroy(gameObject);
        }

        // Old...
        //if (photonView.isMine)
        //{
        //    Debug.Log("CLIENT - " + photonView.viewID + ": I AM DESTROYING " + gameObject.GetComponent<PhotonView>().viewID);
        //    PhotonNetwork.Destroy(gameObject);
        //}
        //else if (PhotonNetwork.isMasterClient)
        //{
        //    Debug.Log("MASTER: I AM DESTROYING " + gameObject.GetComponent<PhotonView>().viewID);
        //    PhotonNetwork.Destroy(gameObject);
        //}
    }

    // Send/Recieve info
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Update Message delayTimer
        messageDelayTimer += Time.deltaTime;

        // If I havent sent an update for awhile
        if (messageDelayTimer >= targetMessageDelayTime)
        {
            // Update player across network
            // Debug.Log(photonView.viewID + " sent msg after: " + messageDelayTimer + " seconds");

            // This is mine to send, and the time is right
            if (stream.isWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(myCamera.transform.rotation);
            }



            // Dont care about when this comes, recieve updates whenever sent
            if (stream.isReading)
            {
                // Store recieved position
                lastStoredPosition = (Vector3)stream.ReceiveNext();

                // Store recieved rotation
                lastStoredRotation = (Quaternion)stream.ReceiveNext();

                // Store recieved pitch (camera rotation)
                lastStoredPitch = (Quaternion)stream.ReceiveNext();

                // Record time vars
                timeToTravel = Time.time - lastMessageTime;
                lastMessageTime = Time.time;
            }
        }
    }
}