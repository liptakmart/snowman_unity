using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /**
     * Prefabs
     */
    public GameObject GameManagerRef;
    public GameObject PlayerSnowmanPrefabRef;
    public GameObject NpcSnowmanPrefabRef;
    public GameObject ProjectilePrefabRef;
    public GameObject CanvasRef;

    public GameObject PickablePistol;
    public GameObject PickableShotgun;
    public GameObject PickableSmg;

    public AudioClip PistolShotAudio;
    public AudioClip PistolReloadAudio;
    public AudioClip PistolEmptyAudio;

    public AudioClip SmgShotAudio;
    public AudioClip SmgReloadAudio;
    public AudioClip SmgEmptyAudio;

    public AudioClip ShotgunShotAudio;
    public AudioClip ShotgunReload;
    public AudioClip ShotgunEmptyAudio;

    public AudioClip GunCock;

    public List<GameObject> SnowmanSpawnPoints;
    public List<GameObject> GunSpawnPoints;
    public List<GameObject> PatrolPoints;

    private void Awake()
    {
        InitState();
    }

    void Start()
    {
        //State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
        //Invoke("InitAfterStart", 0.5f); //TODO solve that spawn is made after everything is loaded etc, is this neccesarry?
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void InitAfterStart()
    //{
    //    State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    //}

    /// <summary>
    /// Initialzies global state of level
    /// </summary>
    private void InitState()
    {
        //State._state.GameManagerRef = GameManagerRef;
        State._state.GameManagerScriptObj = this;
        State._state.SpawnManagerScriptObj = GetComponent<SpawnManager>();
        State._state.SnowmanSpawnPoints = SnowmanSpawnPoints;
        State._state.GunSpawnPoints = GunSpawnPoints;
        State._state.PatrolPoints = PatrolPoints;
        State._state.Canvas = CanvasRef;

        State._prefabs.PlayerSnowmanPrefabRef = PlayerSnowmanPrefabRef;
        State._prefabs.NpcSnowmanPrefabRef = NpcSnowmanPrefabRef;
        State._prefabs.ProjectilePrefabRef = ProjectilePrefabRef;

        State._prefabs.PickablePistol = PickablePistol;
        State._prefabs.PickableShotgun = PickableShotgun;
        State._prefabs.PickableSmg = PickableSmg;

        State._prefabs.AudioPrefabs.PistolEmptyAudio = PistolEmptyAudio;
        State._prefabs.AudioPrefabs.PistolReloadAudio = PistolReloadAudio;
        State._prefabs.AudioPrefabs.PistolShotAudio = PistolShotAudio;

        State._prefabs.AudioPrefabs.ShotgunEmptyAudio = ShotgunEmptyAudio;
        State._prefabs.AudioPrefabs.ShotgunReload = ShotgunReload;
        State._prefabs.AudioPrefabs.ShotgunShotAudio = ShotgunShotAudio;

        State._prefabs.AudioPrefabs.SmgEmptyAudio = SmgEmptyAudio;
        State._prefabs.AudioPrefabs.SmgReloadAudio = SmgReloadAudio;
        State._prefabs.AudioPrefabs.SmgShotAudio = SmgShotAudio;

        State._prefabs.AudioPrefabs.GunCock = GunCock;
    }
}

public static class State
{
    public static LevelState _state = new LevelState();
    public static Prefabs _prefabs = new Prefabs(); 
}

public class Prefabs
{
    public GameObject PlayerSnowmanPrefabRef;
    public GameObject NpcSnowmanPrefabRef;
    public GameObject ProjectilePrefabRef;

    public GameObject PickablePistol;
    public GameObject PickableShotgun;
    public GameObject PickableSmg;

    public AudioPrefabs AudioPrefabs = new AudioPrefabs();
}

public class AudioPrefabs
{
    public AudioClip PistolShotAudio;
    public AudioClip PistolReloadAudio;
    public AudioClip PistolEmptyAudio;

    public AudioClip SmgShotAudio;
    public AudioClip SmgReloadAudio;
    public AudioClip SmgEmptyAudio;

    public AudioClip ShotgunShotAudio;
    public AudioClip ShotgunReload;
    public AudioClip ShotgunEmptyAudio;

    public AudioClip GunCock;
}

public class LevelState
{
    //public GameObject GameManagerRef;
    public GameManager GameManagerScriptObj;
    public SpawnManager SpawnManagerScriptObj;
    public List<GameObject> SnowmanSpawnPoints = new List<GameObject>();
    public List<GameObject> GunSpawnPoints = new List<GameObject>();
    public List<GameObject> PatrolPoints = new List<GameObject>();
    public List<GameObject> PlayerList = new List<GameObject>();
    public List<GameObject> NpcList = new List<GameObject>();
    public GameObject Canvas;
    public List<PlayerStat> PlayerStats = new List<PlayerStat>();
}

public static class Constants
{
    public static readonly string TAG_PLAYER = "PlayerTag";
    public static readonly string TAG_NPC = "NpcTag";
    public static readonly Color TEAM_01_COLOR = Color.red;
    public static readonly Color TEAM_02_COLOR = Color.blue;
    public static readonly Color TEAM_03_COLOR = Color.green;
    public static readonly int MAX_NUM_OF_PICKABLE_GUNS_IN_LEVEL = 3;
    /// <summary>
    /// Delay until guns start to being spawned in level
    /// </summary>
    public static readonly float DELAY_TO_START_SPAWN_GUNS = 1f;
    /// <summary>
    /// Lower bound of time in seconds o rng generator for waiting till next gun is spawned
    /// </summary>
    public static readonly float DELAY_TO_SPAWN_GUNS_LOWER_BOUND = 5f;
    /// <summary>
    /// Upper bound of time in seconds o rng generator for waiting till next gun is spawned
    /// </summary>
    public static readonly float DELAY_TO_SPAWN_GUNS_UPPER_BOUND = 10f;
    /// <summary>
    /// Respawn time for each snowman in seconds.
    /// </summary>
    public static readonly float RESPAWN_TIME_SEC = 3f;
}

/// <summary>
/// Persistent stats about player. SO when snowman dies, there are persistent data about player
/// </summary>
public class PlayerStat
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TeamId { get; set; }
    public bool IsAlive { get; set; }
    public bool WaitingToRespawn { get; set; }
    public int PlayerNumber { get; set; }
    public int KillsCount { get; set; }
    public int DeathCount { get; set; }

    public PlayerStat(int id, string name, int teamId, bool isAlive, int playerNumber, int killsCount, int deathCount, bool waitingToRespawn)
    {
        Id = id;
        Name = name;
        TeamId = teamId;
        IsAlive = isAlive;
        KillsCount = killsCount;
        DeathCount = deathCount;
        PlayerNumber = playerNumber;
        WaitingToRespawn = waitingToRespawn;
    }
}


/// <summary>
/// Movement state of npc
/// </summary>
public enum NPC_MOVEMENT_STATE
{
    IDLE = 1,
    PATROL = 2,
    FOLLOW = 3,
    ATTACK = 4,
}

public enum GUN_TYPE
{
    PISTOL = 1,
    SHOTGUN = 2,
    SMG = 3
}

public abstract class Gun : MonoBehaviour
{
    /// <summary>
    /// Name of the gun
    /// </summary>
    public string Name { get; set; }

    public GUN_TYPE GunType { get; set; }

    /// <summary>
    /// Current ammo in magazine
    /// </summary>
    public int AmmoInMagazine { get; set; }
    /// <summary>
    /// Total magazine capacity
    /// </summary>
    public int MagazineCapacity { get; set; }
    /// <summary>
    /// Number of ammo outside magazine
    /// </summary>
    public int SpareAmmo { get; set; }
    /// <summary>
    /// Is weapon semi or full auto
    /// </summary>
    public bool IsAutomatic { get; set; }
    /// <summary>
    /// Velocity of fired projectile
    /// </summary>
    public float ProjectileVelocity { get; set; }
    /// <summary>
    /// Mass of the fired projectile
    /// </summary>
    public float ProjectileMass { get; set; }
    /// <summary>
    /// Reload time in sec between magazines
    /// </summary>
    public float ReloadTimeSec { get; set; }
    /// <summary>
    /// Delay after firing before the gun can fire again, in seconds.
    /// </summary>
    public float FireDelayInSec { get; set; }
    /// <summary>
    /// Max range of this gun
    /// </summary>
    public float MaxRange { get; set; }

    private float dynamicDispersion = 0f;
    /// <summary>
    /// Dispersion level. If fires consecuntly it increaces and if not fire gradually decreaces
    /// </summary>
    public float DynamicDispersion
    {
        get { return dynamicDispersion; }
        private set { dynamicDispersion = Mathf.Clamp(value, 0f, 1f); }
    }

    /// <summary>
    /// Base dispersion. Default. It combines with dynamic one. This represents angle. Y axis.
    /// </summary>
    public float YBaseDispersion { get; set; }    
    /// <summary>
    /// Base dispersion. Default. It combines with dynamic one. This represents angle. Z axis.
    /// </summary>
    public float ZBaseDispersion { get; set; }
    /// <summary>
    /// Dispersion decreace per second
    /// </summary>
    public float DispersionDecayRate = 0.05f;
    /// <summary>
    /// Dispersion increace by each shot
    /// </summary>
    public float DispersionIncreaseByShot = 0.5f;
    /// <summary>
    /// Interval in seconds between each dispersion decay.
    /// </summary>
    private float DispersionDecayInterval = 0.2f;
    /// <summary>
    /// Size of projectile fired from this gun
    /// </summary>
    public float ProjectileSize { get; set; }

    /// <summary>
    /// Reference for instantiated game object Weapon
    /// </summary>
    public GameObject GameObjectRef { get; set; }
    /// <summary>
    /// Audio source ref of snowman
    /// </summary>
    public AudioSource AudioSourceRef { get; set; }

    protected bool isReloading = false;
    protected Coroutine reloadCoroutine;

    // New properties for firing delay
    protected bool canFire = true;
    protected Coroutine fireDelayCoroutine;

    // Events
    public event Action OnReloadStarted;
    public event Action OnReloadFinished;
    public event Action OnReloadCanceled;
    public event Action OnFired;
    public event Action OnReadyToFire;
    public event Action<int, int> OnAmmoChanged; // Current ammo, Spare ammo

    protected virtual void Awake()
    {
        StartCoroutine(DispersionDecayCoroutine());
    }

    public bool IsAmmoInMagazine()
    {
        return AmmoInMagazine > 0;
    }

    public bool CanReload()
    {
        return SpareAmmo > 0 && AmmoInMagazine < MagazineCapacity;
    }

    public virtual void AddAmmo()
    {

    }

    public virtual void Fire(GameObject snowmanModel, SnowmanState snowman)
    {
        if (isReloading)
            return;

        if (!canFire)
            return;

        if (AmmoInMagazine > 0)
        {
            IncreaseDispersion();
            AmmoInMagazine--;

            // Calculate the spawn position using the child object's position and applying the offset in local space
            Vector3 spawnPosition = snowmanModel.transform.TransformPoint(new Vector3(-2.87f, 4.42f, 0.83f));
            // Since the snowmanModel's forward is not aligned as expected, let's use its right axis for the projectile's line of sight
            Quaternion spawnRotation = Quaternion.LookRotation(snowmanModel.transform.right);

            // Fire logic here
            //Debug.Log(Name + " fired. Ammo left: " + AmmoInMagazine);
            if (this is Pistol)
            {
                FirePistolProjectile(spawnPosition, spawnRotation, snowman);
            }
            else if (this is Shotgun)
            {
                FireShotgunProjectile(spawnPosition, spawnRotation, snowman);
            }
            else if (this is Smg)
            {
                FireSmgProjectile(spawnPosition, spawnRotation, snowman);
            }
            PlayShotAudio();
            // Invoke the OnFired event
            OnFired?.Invoke();

            // Invoke ammo changed event
            OnAmmoChanged?.Invoke(AmmoInMagazine, SpareAmmo);

            // Start the firing delay coroutine
            if (FireDelayInSec > 0)
            {
                canFire = false;
                fireDelayCoroutine = StartCoroutine(FireDelayCoroutine());
            }
        }
        else
        {
            //Debug.Log(Name + " is out of ammo. Reload needed.");
            Reload();
        }
    }

    /// <summary>
    /// Applies random dispersion to a direction vector within specified angle ranges.
    /// </summary>
    /// <param name="originalDirection">The original direction vector.</param>
    /// <param name="maxAngleY">Maximum dispersion angle on the Y-axis in degrees.</param>
    /// <param name="maxAngleZ">Maximum dispersion angle on the Z-axis in degrees.</param>
    /// <returns>A new direction vector with applied dispersion.</returns>
    private Vector3 ApplyDispersion(Vector3 originalDirection, float maxAngleY, float maxAngleZ)
    {
        // Generate random angles within the specified range
        float randomAngleY = UnityEngine.Random.Range(-maxAngleY, maxAngleY);
        float randomAngleZ = UnityEngine.Random.Range(-maxAngleZ, maxAngleZ);

        // Create a rotation based on the random angles
        //Quaternion dispersionRotation = Quaternion.Euler(randomAngleY, 0f, randomAngleZ);
        Quaternion dispersionRotation = Quaternion.Euler(0f, randomAngleY, randomAngleZ);

        // Apply the rotation to the original direction
        Vector3 dispersedDirection = dispersionRotation * originalDirection;

        return dispersedDirection;
    }

    private void FirePistolProjectile(Vector3 projectilePos, Quaternion projectileRot, SnowmanState snowman)
    {
        GameObject projectilePrefab = State._prefabs.ProjectilePrefabRef;
        projectilePrefab.transform.localScale = new Vector3(ProjectileSize, ProjectileSize, ProjectileSize);

        var projectile = Instantiate(projectilePrefab, projectilePos, projectileRot);
        projectile.GetComponent<Projectile>().FiredBySnowmanId = snowman.SnowmanId;
        projectile.GetComponent<Projectile>().FiredBy = snowman;

        var rb = projectile.GetComponent<Rigidbody>();

        Vector3 direction = projectile.transform.forward * -1;
        float yDispersion = YBaseDispersion + (DynamicDispersion * 3f);
        float zDispersion = ZBaseDispersion + (DynamicDispersion * 3f);
        rb.velocity = ApplyDispersion(direction, yDispersion, zDispersion) * ProjectileVelocity;

        var script = projectile.GetComponent<Projectile>();
        script.MaxRange = MaxRange;
        script.OriginPoint = transform.position;
    }

    private void FireShotgunProjectile(Vector3 projectilePos, Quaternion projectileRot, SnowmanState snowman)
    {
        List<float> widthAngleOffsets = new List<float>();
        float yLowerBound = -6.0f;
        float yStep = 0.50f;
        for (float i = yLowerBound; i <= yLowerBound * -1; i += yStep)
        {
            widthAngleOffsets.Add(i);
        }

        List<float> heightAnglesOffsets = new List<float>();
        float xLowerBound = -6f;
        float xStep = 2f;
        for (float i = xLowerBound; i <= xLowerBound * -1; i += xStep)
        {
            heightAnglesOffsets.Add(i);
        }

        for (int i = 0; i < heightAnglesOffsets.Count; i++)
        {
            for (int j = 0; j < widthAngleOffsets.Count; j++)
            {
                GameObject projectilePrefab = State._prefabs.ProjectilePrefabRef;
                projectilePrefab.transform.localScale = new Vector3(ProjectileSize, ProjectileSize, ProjectileSize);

                // Instantiate the projectile
                var projectile = Instantiate(projectilePrefab, projectilePos, projectileRot);
                projectile.GetComponent<Projectile>().FiredBySnowmanId = snowman.SnowmanId;
                projectile.GetComponent<Projectile>().FiredBy = snowman;

                var rb = projectile.GetComponent<Rigidbody>();
                // Original forward direction (negative due to your setup)
                Vector3 forward = projectile.transform.forward * -1;

                // Rotate the forward vector by the angle offset around the Y-axis
                Quaternion rotation = Quaternion.Euler(heightAnglesOffsets[i], widthAngleOffsets[j], 0);
                Vector3 adjustedDirection = rotation * forward;

                float yDispersion = YBaseDispersion + (DynamicDispersion * 3f);
                float zDispersion = ZBaseDispersion + (DynamicDispersion * 3f);

                // Apply the adjusted velocity
                rb.velocity = ApplyDispersion(adjustedDirection.normalized, yDispersion, zDispersion) * ProjectileVelocity;

                var script = projectile.GetComponent<Projectile>();
                script.MaxRange = MaxRange;
                script.OriginPoint = transform.position;
            }
        }
    }

    private void FireSmgProjectile(Vector3 projectilePos, Quaternion projectileRot, SnowmanState snowman)
    {
        GameObject projectilePrefab = State._prefabs.ProjectilePrefabRef;
        projectilePrefab.transform.localScale = new Vector3(ProjectileSize, ProjectileSize, ProjectileSize);

        var projectile = Instantiate(projectilePrefab, projectilePos, projectileRot);
        projectile.GetComponent<Projectile>().FiredBySnowmanId = snowman.SnowmanId;
        projectile.GetComponent<Projectile>().FiredBy = snowman;
        var rb = projectile.GetComponent<Rigidbody>();

        Vector3 direction = projectile.transform.forward * -1;
        float yDispersion = YBaseDispersion + (DynamicDispersion * 3f);
        float zDispersion = ZBaseDispersion + (DynamicDispersion * 3f);
        rb.velocity = ApplyDispersion(direction, yDispersion, zDispersion) * ProjectileVelocity;

        var script = projectile.GetComponent<Projectile>();
        script.MaxRange = MaxRange;
        script.OriginPoint = transform.position;
    }

    /// <summary>
    /// Increases the Dispersion by a predefined amount.
    /// </summary>
    protected void IncreaseDispersion()
    {
        DynamicDispersion += DispersionIncreaseByShot;
        //Debug.Log($"{Name} dispersion increased to {DynamicDispersion}");
    }

    /// <summary>
    /// Coroutine that decreases Dispersion periodically.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DispersionDecayCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(DispersionDecayInterval);

            if (DynamicDispersion > 0f)
            {
                DynamicDispersion -= DispersionDecayRate;
                if (DynamicDispersion < 0f)
                    DynamicDispersion = 0f;

                //Debug.Log($"{Name} dispersion decreased to {DynamicDispersion}");
            }
        }
    }

    protected virtual IEnumerator FireDelayCoroutine()
    {
        yield return new WaitForSeconds(FireDelayInSec);
        canFire = true;
        fireDelayCoroutine = null;
        OnReadyToFire?.Invoke();
    }

    public virtual void Reload()
    {
        if (!isReloading && CanReload())
        {
            // Cancel firing delay if reloading starts
            CancelFireDelay();

            // Invoke the OnReloadStarted event
            PlayReloadAudio();
            OnReloadStarted?.Invoke();

            // Start the reload coroutine
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }
    }

    protected virtual IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        // Optional: Trigger reload animation
        //Debug.Log(Name + " is reloading...");

        // Wait for the reload time
        yield return new WaitForSeconds(ReloadTimeSec);

        // Complete the reload
        int ammoNeeded = MagazineCapacity - AmmoInMagazine;
        int ammoToReload = Mathf.Min(SpareAmmo, ammoNeeded);
        AmmoInMagazine += ammoToReload;
        SpareAmmo -= ammoToReload;
        isReloading = false;

        // Invoke the OnReloadFinished event
        OnReloadFinished?.Invoke();

        // Invoke ammo changed event
        OnAmmoChanged?.Invoke(AmmoInMagazine, SpareAmmo);

        //Debug.Log(Name + " reload complete.");
    }

    public virtual void CancelReload()
    {
        if (isReloading)
        {
            // Stop the reload coroutine
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
            isReloading = false;
            //Debug.Log(Name + " reload canceled.");

            // Invoke the OnReloadCanceled event
            OnReloadCanceled?.Invoke();
        }
    }

    public virtual void CancelFireDelay()
    {
        if (fireDelayCoroutine != null)
        {
            StopCoroutine(fireDelayCoroutine);
            fireDelayCoroutine = null;
            canFire = true;
            //Debug.Log(Name + " firing delay canceled.");
        }
    }

    // New method to increase dispersion
    protected void IncreaseDispersion(float amount)
    {
        DynamicDispersion += amount;
        Debug.Log($"{Name} dispersion increased to {DynamicDispersion}");

        // Start coroutine to decrease dispersion after delay
        StartCoroutine(DecreaseDispersionAfterDelay(amount, 0.2f)); // 0.2 seconds delay
    }

    // Coroutine to decrease dispersion
    private IEnumerator DecreaseDispersionAfterDelay(float amount, float delay)
    {
        yield return new WaitForSeconds(delay);

        DynamicDispersion -= amount;
        Debug.Log($"{Name} dispersion decreased to {DynamicDispersion}");
    }

    /// <summary>
    /// Can fire projectile now
    /// </summary>
    /// <returns></returns>
    public bool CanFire()
    {
        return canFire && AmmoInMagazine > 0;
    }

    /// <summary>
    /// If is reloading in process now
    /// </summary>
    /// <returns></returns>
    public bool IsReloading()
    {
        return isReloading;
    }

    /// <summary>
    /// If magazine is empty
    /// </summary>
    /// <returns></returns>
    public bool IsEmptyMagazine()
    {
        return AmmoInMagazine == 0;
    }

    /// <summary>
    /// If magazine is full return true, else false
    /// </summary>
    /// <returns></returns>
    public bool IsMagazineFull()
    {
        return AmmoInMagazine == MagazineCapacity;
    }

    /// <summary>
    /// Plays reload audio clip for selected gun
    /// </summary>
    private void PlayReloadAudio()
    {
        AudioClip clipToPlay = null;
        float reloadTime = ReloadTimeSec;
        float clipLength = 0f;

        if (this is Pistol)
        {
            clipToPlay = State._prefabs.AudioPrefabs.PistolReloadAudio;
        }
        else if (this is Shotgun)
        {
            clipToPlay = State._prefabs.AudioPrefabs.ShotgunReload;
        }
        else if (this is Smg)
        {
            clipToPlay = State._prefabs.AudioPrefabs.SmgReloadAudio;
        }

        if (clipToPlay != null)
        {
            clipLength = clipToPlay.length;

            // Calculate the required pitch adjustment
            float pitch = clipLength / reloadTime;

            // Clamp the pitch to prevent extreme adjustments
            pitch = Mathf.Clamp(pitch, 0.5f, 2f);

            // Set the pitch on the audio source
            AudioSourceRef.pitch = pitch;

            // Play the audio clip
            AudioSourceRef.PlayOneShot(clipToPlay);

            // Start a coroutine to reset the pitch after the adjusted playback duration
            StartCoroutine(ResetAudioPitchAfterDelay(clipLength / pitch));
        }
    }

    /// <summary>
    /// Plays shot audio clip for selected gun
    /// </summary>
    private void PlayShotAudio()
    {
        if (this is Pistol)
        {
            AudioSourceRef.PlayOneShot(State._prefabs.AudioPrefabs.PistolShotAudio);
        }
        else if (this is Shotgun)
        {
            var shotAudio = State._prefabs.AudioPrefabs.ShotgunShotAudio;
            float clipLength = shotAudio.length;
            // Calculate the required pitch adjustment
            float pitch = clipLength / FireDelayInSec;

            // Clamp the pitch to prevent extreme adjustments
            pitch = Mathf.Clamp(pitch, 0.5f, 2f);

            // Set the pitch on the audio source
            AudioSourceRef.pitch = pitch;

            // Play the audio clip
            AudioSourceRef.PlayOneShot(shotAudio);

            // Start a coroutine to reset the pitch after the adjusted playback duration
            StartCoroutine(ResetAudioPitchAfterDelay(clipLength / pitch));
        }
        else if (this is Smg)
        {
            AudioSourceRef.PlayOneShot(State._prefabs.AudioPrefabs.SmgShotAudio);
        }
    }

    /// <summary>
    /// Play empty click audio clip for selected gun
    /// </summary>
    public void PlayEmptyAudio()
    {
        //prevent duplical plays
        if (AudioSourceRef.isPlaying)
        {
            return;
        }

        if (this is Pistol)
        {
            AudioSourceRef.PlayOneShot(State._prefabs.AudioPrefabs.PistolEmptyAudio);
        }
        else if (this is Shotgun)
        {
            AudioSourceRef.PlayOneShot(State._prefabs.AudioPrefabs.ShotgunEmptyAudio);
        }
        else if (this is Smg)
        {
            AudioSourceRef.PlayOneShot(State._prefabs.AudioPrefabs.SmgEmptyAudio);
        }
    }

    /// <summary>
    /// Stops audio
    /// </summary>
    public void StopAudio()
    {
        AudioSourceRef.Stop();
    }

    public void PlayCockAudio()
    {
        AudioSourceRef.PlayOneShot(State._prefabs.AudioPrefabs.GunCock);
    }

    private IEnumerator ResetAudioPitchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioSourceRef.pitch = 1f;
    }
}

public class Pistol : Gun
{
    protected override void Awake()
    {
        base.Awake();
        Name = "Pistol";
        GunType = GUN_TYPE.PISTOL;
        AmmoInMagazine = 12;
        MagazineCapacity = 12;
        SpareAmmo = 48;
        IsAutomatic = false;
        ProjectileVelocity = 20;
        ProjectileMass = 2;
        ReloadTimeSec = 1.5f;
        FireDelayInSec = 0.2f;
        MaxRange = 50f;
        YBaseDispersion = 5f;
        ZBaseDispersion = 3f;
        DispersionIncreaseByShot = 0.1f;
        DispersionDecayRate= 0.2f;
        ProjectileSize = 5f; 
    }

    /// <summary>
    /// Adds default ammo
    /// </summary>
    public override void AddAmmo()
    {
        SpareAmmo += 48;
    }
}

public class Shotgun : Gun
{
    protected override void Awake()
    {
        base.Awake();
        Name = "Shotgun";
        GunType = GUN_TYPE.SHOTGUN;
        AmmoInMagazine = 6;
        MagazineCapacity = 6;
        SpareAmmo = 6;
        IsAutomatic = false;
        ProjectileVelocity = 15;
        ProjectileMass = 3;
        ReloadTimeSec = 7f;
        FireDelayInSec = 1.5f;
        MaxRange = 12.5f;
        YBaseDispersion = 5f;
        ZBaseDispersion = 9f;
        DispersionIncreaseByShot = 1f;
        DispersionDecayRate = 0.2f;
        ProjectileSize = 1f;
    }

    /// <summary>
    /// Adds default ammo
    /// </summary>
    public override void AddAmmo()
    {
        SpareAmmo += 6;
    }
}

public class Smg : Gun
{
    protected override void Awake()
    {
        base.Awake();
        Name = "Smg";
        GunType = GUN_TYPE.SMG;
        AmmoInMagazine = 30;
        MagazineCapacity = 30;
        SpareAmmo = 30;
        IsAutomatic = true;
        ProjectileVelocity = 20;
        ProjectileMass = 3;
        ReloadTimeSec = 6f;
        FireDelayInSec = 0.05f;
        MaxRange = 50;
        YBaseDispersion = 5f;
        ZBaseDispersion = 3f;
        DispersionIncreaseByShot = 0.1f;
        DispersionDecayRate = 0.15f;
        ProjectileSize = 5f;
    }

    /// <summary>
    /// Adds default ammo
    /// </summary>
    public override void AddAmmo()
    {
        SpareAmmo += 30;
    }
}