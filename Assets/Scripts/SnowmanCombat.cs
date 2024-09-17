using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SnowmanCombat : MonoBehaviour
{
    public int snowmanId;
    public bool isAlive;
    public bool isNpc;

    public GameObject snowmanModel;

    public GameObject pistolGo;
    public GameObject shotgunGo;
    public GameObject smgGo;

    public AudioClip pistolShotAudio;
    public AudioClip pistolReloadAudio;
    public AudioClip pistolEmptyAudio;

    public List<Weapon> allWeapons;
    public List<Weapon> ownedWeapons;
    public Weapon selectedWeapon;

    private AudioSource audioSource;

    /// <summary>
    /// Is reloading in process
    /// </summary>
    private bool isReloading;
    /// <summary>
    /// Count time in seconds for reload purposes
    /// </summary>
    private float reloadTimeCounter;
    /// <summary>
    /// Time to reload, upper bound in seconds
    /// </summary>
    private float reloadFinishTime;
    /// <summary>
    /// Count time in seconds for rate of fire purposes
    /// </summary>
    private float autoShotTimeCounter;

    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;
        isReloading = false;
        reloadTimeCounter = 0;
        autoShotTimeCounter = int.MaxValue;
        audioSource = GetComponent<AudioSource>();

        allWeapons = new List<Weapon>()
        {
            new Weapon("Pistol", GUN_NAME.PISTOL, 12, 12, 10000, false, 0, 10, 10, 6, pistolGo),
            new Weapon("Shotgun", GUN_NAME.SHOTGUN, 6, 6, 6, false, 0, 15, 20, 8, shotgunGo),
            new Weapon("Smg", GUN_NAME.SMG, 30, 30, 30, true, 0.125f, 20, 10, 8, smgGo),
        };

        ownedWeapons = new List<Weapon>()
        {
            allWeapons[0],
            allWeapons[1],
            allWeapons[2],
        };
        selectedWeapon = ownedWeapons[0];

        shotgunGo.SetActive(false);
        smgGo.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        OnFireClick();
        OnWeaponChange();

        if (isReloading)
        {
            reloadTimeCounter += Time.deltaTime;
            if (reloadTimeCounter >= reloadFinishTime)
            {
                //finish reloading
                selectedWeapon.Reload();
                //reset
                isReloading = false;
                reloadTimeCounter = 0;
                reloadFinishTime = 0;
                autoShotTimeCounter = int.MaxValue;
                StopAudio();
                //update UI
                State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
            }
        }
    }

    private void OnFireClick()
    {
        if (isNpc && !isAlive)
        {
            return;
        }

        if (selectedWeapon.IsAutomatic)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (autoShotTimeCounter >= selectedWeapon.RateOfFireInSec)
                {
                    autoShotTimeCounter = 0;
                }
                else
                {
                    autoShotTimeCounter += Time.deltaTime;
                    return;
                }

                if (selectedWeapon.MagazineNotEmpty() && !isReloading)
                {
                    PlayAudioPistolShot();
                    FireProjectile();
                    selectedWeapon.HandleShotAfterward();
                }
                else if (selectedWeapon.CanReload() && !isReloading)
                {
                    //PlayAudioPistolEmpty(); TODO
                    //Start reloading
                    PlayAudioPistolReload();
                    reloadTimeCounter = 0;
                    isReloading = true;
                    reloadFinishTime = selectedWeapon.ReloadTimeSec;
                }
                State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                //reset
                autoShotTimeCounter = int.MaxValue;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (selectedWeapon.MagazineNotEmpty() && !isReloading)
                {
                    PlayAudioPistolShot();
                    FireProjectile();
                    selectedWeapon.HandleShotAfterward();
                }
                else if (selectedWeapon.CanReload() && !isReloading)
                {
                    //PlayAudioPistolEmpty(); TODO
                    //Start reloading
                    PlayAudioPistolReload();
                    reloadTimeCounter = 0;
                    isReloading = true;
                    reloadFinishTime = selectedWeapon.ReloadTimeSec;
                }
                State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
            }
        }
    }

    private void OnWeaponChange()
    {
        if (!isNpc && isAlive && Input.GetKeyUp(KeyCode.Q))
        {
            int selectedWeaponIdx = ownedWeapons.IndexOf(selectedWeapon);
            selectedWeapon.GameObjectRef.SetActive(false);
            if (selectedWeaponIdx + 1 > ownedWeapons.Count - 1)
            {
                selectedWeapon = ownedWeapons[0];
                
            }
            else
            {
                selectedWeapon = ownedWeapons[selectedWeaponIdx + 1];
            }

            if (isReloading)
            {
                //reset
                StopAudio();
                isReloading = false;
                reloadTimeCounter = 0;
                reloadFinishTime = 0;
                autoShotTimeCounter = int.MaxValue;
            }
            selectedWeapon.GameObjectRef.SetActive(true);
            State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
        }
    }

    private void FireProjectile()
    {
        // Calculate the spawn position using the child object's position and applying the offset in local space
        Vector3 spawnPosition = snowmanModel.transform.TransformPoint(new Vector3(-2.87f, 4.42f, 0.83f));

        // Since the snowmanModel's forward is not aligned as expected, let's use its right axis for the projectile's line of sight
        Quaternion spawnRotation = Quaternion.LookRotation(snowmanModel.transform.right);

        switch (selectedWeapon.Gun)
        {
            case GUN_NAME.PISTOL: FirePistolProjectile(spawnPosition, spawnRotation); break;
            case GUN_NAME.SHOTGUN: FireShotgunProjectile(spawnPosition, spawnRotation); break;
            case GUN_NAME.SMG: FireSmgProjectile(spawnPosition, spawnRotation); break;
        }
    }

    private void FirePistolProjectile(Vector3 projectilePos, Quaternion projectileRot)
    {
        var projectile = Instantiate(State._prefabs.ProjectilePrefabRef, projectilePos, projectileRot);
        projectile.GetComponent<Projectile>().FiredBySnowmanId = snowmanId;
        var rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply velocity in the new forward direction of the spawned projectile
            rb.velocity = projectile.transform.forward * -1 * selectedWeapon.ProjectileVelocity;
        }
    }

    private void FireShotgunProjectile(Vector3 projectilePos, Quaternion projectileRot)
    {
        GameObject[] projectiles = new GameObject[7];
        float[] angleOffsets = new float[] { -4.5f, -3f, -1.5f, 0f, 1.5f, 3f, 4.5f };

        for (int i = 0; i < 7; i++)
        {
            // Instantiate the projectile
            projectiles[i] = Instantiate(State._prefabs.ProjectilePrefabRef, projectilePos, projectileRot);
            projectiles[i].GetComponent<Projectile>().FiredBySnowmanId = snowmanId;

            var rb = projectiles[i].GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Original forward direction (negative due to your setup)
                Vector3 forward = projectiles[i].transform.forward * -1;

                // Rotate the forward vector by the angle offset around the Y-axis
                Quaternion rotation = Quaternion.Euler(0, angleOffsets[i], 0);
                Vector3 adjustedDirection = rotation * forward;

                // Apply the adjusted velocity
                rb.velocity = adjustedDirection.normalized * selectedWeapon.ProjectileVelocity;
            }
        }
    }

    private void FireSmgProjectile(Vector3 projectilePos, Quaternion projectileRot)
    {
        var projectile = Instantiate(State._prefabs.ProjectilePrefabRef, projectilePos, projectileRot);
        projectile.GetComponent<Projectile>().FiredBySnowmanId = snowmanId;
        var rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply velocity in the new forward direction of the spawned projectile
            rb.velocity = projectile.transform.forward * -1 * selectedWeapon.ProjectileVelocity;
        }
    }

    /// <summary>
    /// Kill this snowman instance
    /// </summary>
    public void Die()
    {
        isAlive = false;
        snowmanId = -1;
        GameObject snowman = State._state.GameManagerScriptObj.SpawnSnowman(isNpc);

        if (!isNpc)
        {
            State._state.PlayersSnowmanRef.Remove(gameObject);
            State._state.PlayersSnowmanRef.Add(snowman);
        }

        Collider col = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (col != null && rb != null)
        {
            Destroy(col);
            Destroy(rb);
        }
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
        Destroy(gameObject, 10f);
    }

    /// <summary>
    /// Plays audio for pistol shot
    /// </summary>
    private void PlayAudioPistolShot()
    {
        audioSource.PlayOneShot(pistolShotAudio);
    }

    /// <summary>
    /// Plays audio for pistol reload
    /// </summary>
    private void PlayAudioPistolReload()
    {
        audioSource.PlayOneShot(pistolReloadAudio);
    }

    /// <summary>
    /// Plays audio for pistol empty shot
    /// </summary>
    private void PlayAudioPistolEmpty()
    {
        audioSource.PlayOneShot(pistolEmptyAudio);
    }

    /// <summary>
    /// Stops audio
    /// </summary>
    private void StopAudio() 
    {
        audioSource.Stop();
    }
}

public class Weapon
{
    public GUN_NAME Gun { get; set; }

    /// <summary>
    /// Name of the gun
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Current ammo in magazine
    /// </summary>
    public int AmmoInMagazine {get; set;}
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
    /// Pause between projectiles in full auto in sec
    /// </summary>
    public float RateOfFireInSec { get; set; }
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
    /// Reference for instantiated game object Weapon
    /// </summary>
    public GameObject GameObjectRef { get; set; }

    public Weapon(
        string name,
        GUN_NAME gun, 
        int ammoInMagazine, 
        int magazineCapacity, 
        int spareAmmo, 
        bool isAutomatic, 
        float rateOfFireSec, 
        float projectileVelocity, 
        float projectileMass,
        float reloadTimeSec,
        GameObject gameObjectRef)
    {
        Name = name;
        Gun = gun;
        AmmoInMagazine = ammoInMagazine;
        MagazineCapacity = magazineCapacity;
        SpareAmmo = spareAmmo;
        IsAutomatic = isAutomatic;
        RateOfFireInSec= rateOfFireSec;
        ProjectileVelocity= projectileVelocity;
        ProjectileMass= projectileMass;
        ReloadTimeSec= reloadTimeSec;
        GameObjectRef = gameObjectRef;
    }

    public bool MagazineNotEmpty()
    {
        if (AmmoInMagazine > 0)
        {
            return true;
        }
        return false;
    }

    public bool CanReload()
    {
        if (SpareAmmo > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// After shot has been fired. Handle apropriate action, such as reduce ammo, reload etc.
    /// </summary>
    public void HandleShotAfterward()
    {
        AmmoInMagazine--;
        if (Gun == GUN_NAME.PISTOL)
        {
            
        }
        else if (Gun == GUN_NAME.SHOTGUN)
        {

        }
        else if (Gun == GUN_NAME.SMG)
        {

        }
    }

    public void Reload()
    {
        State._state.Canvas.GetComponent<CanvasManager>().SetReloadingText();
        //TODO wait
        int newAmmo = SpareAmmo >= MagazineCapacity ? MagazineCapacity : SpareAmmo;
        AmmoInMagazine = newAmmo;
        SpareAmmo -= newAmmo;
    }
}

public enum GUN_NAME
{
    PISTOL = 1,
    SHOTGUN = 2,
    SMG = 3,
}
