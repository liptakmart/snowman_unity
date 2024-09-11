using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowmanCombat : MonoBehaviour
{
    public int snowmanId;
    public bool isAlive;
    public bool isNpc;

    public GameObject snowmanModel;
    public List<Weapon> ownedWeapons;
    public Weapon selectedWeapon;

    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;
        ownedWeapons = new List<Weapon>()
        {
            new Weapon(GUN_NAME.PISTOL, 12, 12, 10000, false, 0, 10, 10, 6)
        };
        selectedWeapon = ownedWeapons[0];
    }

    // Update is called once per frame
    void Update()
    {
        OnFireClick();
    }

    private void OnFireClick()
    {
        if (!isNpc && isAlive && Input.GetKeyDown(KeyCode.Space) && selectedWeapon.CanFire())
        {
            FireProjectile();
            selectedWeapon.HandleShot(selectedWeapon);
        }
    }
    
    private void FireProjectile()
    {
        // Calculate the spawn position using the child object's position and applying the offset in local space
        Vector3 spawnPosition = snowmanModel.transform.TransformPoint(new Vector3(-2.87f, 4.42f, 0.83f));

        // Since the snowmanModel's forward is not aligned as expected, let's use its right axis for the projectile's line of sight
        Quaternion spawnRotation = Quaternion.LookRotation(snowmanModel.transform.right);

        // Instantiate the projectile at the calculated position with the adjusted rotation
        GameObject projectileFired = Instantiate(State._prefabs.ProjectilePrefabRef, spawnPosition, spawnRotation);
        projectileFired.GetComponent<Projectile>().FiredBySnowmanId = snowmanId;

        Rigidbody rb = projectileFired.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply velocity in the new forward direction of the spawned projectile
            rb.velocity = projectileFired.transform.forward * -1 * selectedWeapon.ProjectileVelocity;
        }
    }

    /// <summary>
    /// Kill this snowman instance
    /// </summary>
    public void Die()
    {
        isAlive = false;
        snowmanId = -1;
        State._state.GameManagerScriptObj.SpawnSnowman(isNpc);

        Collider col = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (col != null && rb != null)
        {
            Destroy(col);
            Destroy(rb);
        }
        Destroy(gameObject, 10f);
    }
}

public class Weapon
{
    public GUN_NAME Gun { get; set; }

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
    /// Pause between projectiles in full auto in milisec
    /// </summary>
    public float RateOfFireMilisec { get; set; }
    /// <summary>
    /// Velocity of fired projectile
    /// </summary>
    public float ProjectileVelocity { get; set; }
    /// <summary>
    /// Mass of the fired projectile
    /// </summary>
    public float ProjectileMass { get; set; }
    /// <summary>
    /// Reload time in milisec between magazines
    /// </summary>
    public float ReloadTimeMilisec { get; set; }

    public Weapon(
        GUN_NAME gun, 
        int ammoInMagazine, 
        int magazineCapacity, 
        int spareAmmo, 
        bool isAutomatic, 
        float rateOfFireMilisec, 
        float projectileVelocity, 
        float projectileMass,
        float reloadTimeMilisec)
    {
        Gun = gun;
        AmmoInMagazine = ammoInMagazine;
        MagazineCapacity = magazineCapacity;
        SpareAmmo = spareAmmo;
        IsAutomatic = isAutomatic;
        RateOfFireMilisec= rateOfFireMilisec;
        ProjectileVelocity= projectileVelocity;
        ProjectileMass= projectileMass;
        ReloadTimeMilisec= reloadTimeMilisec;
    }

    public bool CanFire()
    {
        if (AmmoInMagazine > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// After shot has been fired. Handle apropriate action, such as reduce ammo, reload etc.
    /// </summary>
    /// <param name="weaponFired"></param>
    public void HandleShot(Weapon weaponFired)
    {

    }
}

public enum GUN_NAME
{
    PISTOL = 1,
    SHOTGUN = 2,
    SMG = 3,
}
