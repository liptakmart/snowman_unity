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

    public AudioClip PistolShotAudio;
    public AudioClip PistolReloadAudio;
    public AudioClip PistolEmptyAudio;

    public AudioClip SmgShotAudio;
    public AudioClip SmgReloadAudio;
    public AudioClip SmgEmptyAudio;

    public AudioClip ShotgunShotAudio;
    public AudioClip ShotgunReload;
    public AudioClip ShotgunEmptyAudio;

    public List<GameObject> SpawnPoints;
    public List<GameObject> PatrolPoints;

    void Start()
    {
        InitState();
        GameObject[] snowmenArr = SpawnSnowmen(
            new int? [] { null, null/*, null, null*/ },
            new bool[] { false, true, /*true, true*/ },
            new int[] { 0, 1, /*1, 1 */},
            new GUN_TYPE[] { GUN_TYPE.PISTOL, GUN_TYPE.PISTOL/*, GUN_TYPE.PISTOL, GUN_TYPE.PISTOL*/ },
            new List<GUN_TYPE[]>()
            {
                new GUN_TYPE[] { GUN_TYPE.PISTOL, GUN_TYPE.SHOTGUN, GUN_TYPE.SMG},
                new GUN_TYPE[] { GUN_TYPE.PISTOL},
                //new GUN_TYPE[] { GUN_TYPE.PISTOL},
                //new GUN_TYPE[] { GUN_TYPE.PISTOL},
            });
            
        State._state.PlayersSnowmanRef.Add(snowmenArr[0]);
        State._state.NpcSnowmanRef.Add(snowmenArr[1]);
        Invoke("InitAfterStart", 0.5f); //TODO solve that spawn is made after everything is loaded etc, is this neccesarry?
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitAfterStart()
    {
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    /// <summary>
    /// Initialzies global state of level
    /// </summary>
    private void InitState()
    {
        State._state.GameManagerRef = GameManagerRef;
        State._state.GameManagerScriptObj = this;
        State._state.SpawnPoints = SpawnPoints;
        State._state.PatrolPoints = PatrolPoints;
        State._state.Canvas = CanvasRef;

        State._prefabs.PlayerSnowmanPrefabRef = PlayerSnowmanPrefabRef;
        State._prefabs.NpcSnowmanPrefabRef = NpcSnowmanPrefabRef;
        State._prefabs.ProjectilePrefabRef = ProjectilePrefabRef;

        State._prefabs.AudioPrefabs.PistolEmptyAudio = PistolEmptyAudio;
        State._prefabs.AudioPrefabs.PistolReloadAudio = PistolReloadAudio;
        State._prefabs.AudioPrefabs.PistolShotAudio = PistolShotAudio;

        State._prefabs.AudioPrefabs.ShotgunEmptyAudio = ShotgunEmptyAudio;
        State._prefabs.AudioPrefabs.ShotgunReload = ShotgunReload;
        State._prefabs.AudioPrefabs.ShotgunShotAudio = ShotgunShotAudio;

        State._prefabs.AudioPrefabs.SmgEmptyAudio = SmgEmptyAudio;
        State._prefabs.AudioPrefabs.SmgReloadAudio = SmgReloadAudio;
        State._prefabs.AudioPrefabs.SmgShotAudio = SmgShotAudio;
    }

    public GameObject[] SpawnSnowmen(int?[] snowmanId, bool[] isNpcArr, int[] teamIds, GUN_TYPE[] gunType, List<GUN_TYPE[]> ownedGuns)
    {
        List<GameObject> snowmenList = new List<GameObject>();
        Vector3[] spawnPositions = GetSpawnPositions(isNpcArr.Length);
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            GameObject snowman = Instantiate(isNpcArr[i] ? NpcSnowmanPrefabRef : PlayerSnowmanPrefabRef, spawnPositions[i], Quaternion.identity);
            SnowmanState snowmanState = snowman.GetComponent<SnowmanState>();
            snowmanState.Initialize(snowmanId[i], isNpcArr[i], teamIds[i], gunType[i], ownedGuns[i]);
            snowmenList.Add(snowman);
        }
        return snowmenList.ToArray();
    }

    public GameObject SpawnSnowman(bool isNpc, int teamId, int? snowmanId, GUN_TYPE gunType, GUN_TYPE[] ownedGuns)
    {
        GameObject snowman = Instantiate(isNpc ? NpcSnowmanPrefabRef : PlayerSnowmanPrefabRef, GetSpawnPosition(), Quaternion.identity);
        SnowmanState snowmanState = snowman.GetComponent<SnowmanState>();
        snowmanState.Initialize(snowmanId, isNpc, teamId, gunType, ownedGuns);
        return snowman;
    }

    private Vector3 GetSpawnPosition()
    {
        if (SpawnPoints != null && SpawnPoints.Count > 0)
        {
            // Create a copy of the spawn points list
            List<GameObject> shuffledSpawnPoints = new List<GameObject>(SpawnPoints);
            // Shuffle the list
            ShuffleList(shuffledSpawnPoints);

            // Iterate over the shuffled list
            foreach (var spawnPoint in shuffledSpawnPoints)
            {
                var sp = spawnPoint.GetComponent<SpawnPoint>();
                if (sp.NoSnowmanInside())
                {
                    return sp.transform.position;
                }
            }
            // All spawn points are occupied
            // Handle as needed: wait and retry, throw exception, or return a default position
            throw new Exception("No empty spawn points available");
        }
        throw new UnityException("Spawn points not initialized properly!");
    }

    private Vector3[] GetSpawnPositions(int numOfPos)
    {
        if (SpawnPoints != null && SpawnPoints.Count > 0)
        {
            List<Vector3> positions = new List<Vector3>();
            // Create a copy of the spawn points list
            List<GameObject> shuffledSpawnPoints = new List<GameObject>(SpawnPoints);
            // Shuffle the list
            ShuffleList(shuffledSpawnPoints);

            // Iterate over the shuffled list
            foreach (var spawnPoint in shuffledSpawnPoints)
            {
                var sp = spawnPoint.GetComponent<SpawnPoint>();
                if (sp.NoSnowmanInside())
                {
                    positions.Add(sp.transform.position);
                }

                if (positions.Count == numOfPos)
                {
                    return positions.ToArray();
                }
            }

            throw new Exception("No empty spawn points available");
        }
        throw new UnityException("Spawn points not initialized properly!");
    }

    // Helper method to shuffle a list
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    /// <summary>
    /// Removes dead snowman id from spawn points where he entered. Because he is no longer there, cause he is dead.
    /// </summary>
    /// <param name="deadSnowmanId"></param>
    public void RemoveDeadSnowmanIdFromSpawnPoints(int deadSnowmanId)
    {
        foreach (GameObject item in SpawnPoints)
        {
            SpawnPoint script = item.GetComponent<SpawnPoint>();
            script.TryRemoveSnowman(deadSnowmanId);
        }
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
}

public class LevelState
{
    public GameObject GameManagerRef;
    public GameManager GameManagerScriptObj;
    public List<GameObject> SpawnPoints = new List<GameObject>();
    public List<GameObject> PatrolPoints = new List<GameObject>();
    public List<GameObject> PlayersSnowmanRef = new List<GameObject>();
    public List<GameObject> NpcSnowmanRef = new List<GameObject>();
    public GameObject Canvas;
}

public static class Constants
{
    public static readonly string TAG_PLAYER = "PlayerTag";
    public static readonly string TAG_NPC = "NpcTag";
    public static readonly Color TEAM_01_COLOR = Color.red;
    public static readonly Color TEAM_02_COLOR = Color.blue;
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
        YBaseDispersion = 7f;
        ZBaseDispersion = 5f;
        DispersionIncreaseByShot = 0.1f;
        DispersionDecayRate= 0.2f;
        ProjectileSize = 5f; 
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
        YBaseDispersion = 7f;
        ZBaseDispersion = 15f;
        DispersionIncreaseByShot = 1f;
        DispersionDecayRate = 0.2f;
        ProjectileSize = 1f;
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
        YBaseDispersion = 7f;
        ZBaseDispersion = 5f;
        DispersionIncreaseByShot = 0.1f;
        DispersionDecayRate = 0.15f;
        ProjectileSize = 5f;
    }
}