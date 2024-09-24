using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject GameManagerRef;
    public GameObject PlayerSnowmanPrefabRef;
    public GameObject NpcSnowmanPrefabRef;
    public GameObject ProjectilePrefabRef;
    public GameObject CanvasRef;

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
    /// Reference for instantiated game object Weapon
    /// </summary>
    public GameObject GameObjectRef { get; set; }

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
    }

    public bool IsAmmoInMagazine()
    {
        return AmmoInMagazine > 0;
    }

    public bool CanReload()
    {
        return SpareAmmo > 0 && AmmoInMagazine < MagazineCapacity;
    }

    public virtual void Fire()
    {
        if (isReloading)
            return;

        if (!canFire)
            return;

        if (AmmoInMagazine > 0)
        {
            AmmoInMagazine--;

            // Fire logic here
            //Debug.Log(Name + " fired. Ammo left: " + AmmoInMagazine);

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
}

public class Pistol : Gun
{
    protected override void Awake()
    {
        Name = "Pistol";
        GunType = GUN_TYPE.PISTOL;
        AmmoInMagazine = 12;
        MagazineCapacity = 12;
        SpareAmmo = 48;
        IsAutomatic = false;
        ProjectileVelocity = 20;
        ProjectileMass = 2;
        ReloadTimeSec = 1.5f;
        FireDelayInSec = 0.01f;
    }
}

public class Shotgun : Gun
{
    protected override void Awake()
    {
        Name = "Shotgun";
        GunType = GUN_TYPE.SHOTGUN;
        AmmoInMagazine = 6;
        MagazineCapacity = 6;
        SpareAmmo = 6;
        IsAutomatic = false;
        ProjectileVelocity = 20;
        ProjectileMass = 3;
        ReloadTimeSec = 7f;
        FireDelayInSec = 1.5f;
    }
}

public class Smg : Gun
{
    protected override void Awake()
    {
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
    }
}
