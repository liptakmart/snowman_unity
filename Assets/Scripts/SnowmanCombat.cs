using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SnowmanCombat : MonoBehaviour
{
    public int snowmanId;
    public bool isAlive;
    public bool isNpc;
    public Color cylinderColor;

    public GameObject snowmanModel;

    public GameObject pistolGo;
    public GameObject shotgunGo;
    public GameObject smgGo;

    public AudioClip pistolShotAudio;
    public AudioClip pistolReloadAudio;
    public AudioClip pistolEmptyAudio;

    public AudioClip smgShotAudio;
    public AudioClip smgReloadAudio;
    public AudioClip smgEmptyAudio;

    public AudioClip shotgunShotAudio;
    public AudioClip shotgunReload;
    public AudioClip shotgunEmptyAudio;

    public List<Gun> ownedGuns;
    public Gun selectedGun;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;
        audioSource = GetComponent<AudioSource>();
        GameObject cylinderGo = snowmanModel.transform.Find("Cylinder").gameObject;
        Renderer renderer = cylinderGo.GetComponent<Renderer>();
        Material newMaterial = new Material(renderer.material);
        newMaterial.color = cylinderColor;
        renderer.material = newMaterial;

        ownedGuns = new List<Gun>();

        // Create separate GameObjects for gun components
        GameObject pistolGunObject = new GameObject("PistolGun");
        pistolGunObject.transform.parent = this.transform;
        Pistol pistolObj = pistolGunObject.AddComponent<Pistol>();
        pistolObj.GameObjectRef = pistolGo; // Assign the visual model
        ownedGuns.Add(pistolObj);

        GameObject shotgunGunObject = new GameObject("ShotgunGun");
        shotgunGunObject.transform.parent = this.transform;
        Shotgun shotgunObj = shotgunGunObject.AddComponent<Shotgun>();
        shotgunObj.GameObjectRef = shotgunGo;
        ownedGuns.Add(shotgunObj);

        GameObject smgGunObject = new GameObject("SmgGun");
        smgGunObject.transform.parent = this.transform;
        Smg smgObj = smgGunObject.AddComponent<Smg>();
        smgObj.GameObjectRef = smgGo;
        ownedGuns.Add(smgObj);

        selectedGun = ownedGuns[0];

        pistolGo.SetActive(true);
        shotgunGo.SetActive(false);
        smgGo.SetActive(false);

        SubscribeToGunEvents(selectedGun);
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (isNpc || !isAlive)
        {
            return;
        }

        OnFireClick();
        OnWeaponChange();
        OnGunReload();
    }

    private void OnDestroy()
    {
        if (selectedGun != null)
        {
            UnsubscribeFromGunEvents(selectedGun);
        }
        isAlive = false;
    }

    private void SubscribeToGunEvents(Gun gun)
    {
        gun.OnReloadStarted += HandleReloadStarted;
        gun.OnReloadFinished += HandleReloadFinished;
        gun.OnReloadCanceled += HandleReloadCanceled;
        gun.OnFired += HandleFired;
        gun.OnReadyToFire += HandleReadyToFire; // Subscribe to new event
        gun.OnAmmoChanged += HandleAmmoChanged;
    }

    private void UnsubscribeFromGunEvents(Gun gun)
    {
        gun.OnReloadStarted -= HandleReloadStarted;
        gun.OnReloadFinished -= HandleReloadFinished;
        gun.OnReloadCanceled -= HandleReloadCanceled;
        gun.OnFired -= HandleFired;
        gun.OnReadyToFire -= HandleReadyToFire; // Unsubscribe from new event
        gun.OnAmmoChanged -= HandleAmmoChanged;
    }

    private void HandleReloadStarted()
    {
        Debug.Log($"{selectedGun.Name} reload started.");
        // Play reload sound if applicable
        PlayReloadAudio();
        // Update UI if necessary
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void HandleReloadFinished()
    {
        Debug.Log($"{selectedGun.Name} reload finished.");
        // Update UI if necessary
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void HandleReloadCanceled()
    {
        Debug.Log($"{selectedGun.Name} reload canceled.");
        // Stop reload sound if applicable
        StopAudio();
        // Update UI if necessary
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void HandleFired()
    {
        Debug.Log($"{selectedGun.Name} fired.");
        // Play firing sound
        PlayShotAudio();
        // Fire the projectile
        FireProjectile();
        // Update UI if necessary
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void HandleReadyToFire()
    {
        Debug.Log($"{selectedGun.Name} is ready to fire again.");
        // Update UI if necessary
    }

    private void HandleAmmoChanged(int ammoInMagazine, int spareAmmo)
    {
        Debug.Log($"{selectedGun.Name} ammo changed: {ammoInMagazine} / {spareAmmo}");
        // Update UI with new ammo counts
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void OnFireClick()
    {
        bool clickedFire = false;
        if (!selectedGun.IsAutomatic && Input.GetKeyUp(KeyCode.Space))
        {
            clickedFire = true;
            if (!selectedGun.IsEmptyMagazine())
            {
                selectedGun.Fire();
            }
        }
        else if (selectedGun.IsAutomatic && Input.GetKey(KeyCode.Space))
        {
            clickedFire = true;
            if (!selectedGun.IsEmptyMagazine())
            {
                selectedGun.Fire();
            }
        }

        if (clickedFire && selectedGun.IsEmptyMagazine())
        {
            PlayEmptyAudio();
        }
    }

    private void OnWeaponChange()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (ownedGuns.Count == 1)
            {
                return;
            }

            int selectedWeaponIdx = ownedGuns.IndexOf(selectedGun);
            UnsubscribeFromGunEvents(selectedGun);
            bool isReloading = selectedGun.IsReloading();
            selectedGun.GameObjectRef.SetActive(false);

            if (isReloading)
            {
                //reset
                selectedGun.CancelReload();
                selectedGun.CancelFireDelay();
            }

            if (selectedWeaponIdx + 1 > ownedGuns.Count - 1)
            {
                selectedGun = ownedGuns[0];
            }
            else
            {
                selectedGun = ownedGuns[selectedWeaponIdx + 1];
            }

            selectedGun.GameObjectRef.SetActive(true);
            SubscribeToGunEvents(selectedGun);

            StopAudio();
            // Update UI
            State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
        }
    }

    /// <summary>
    /// On reload key clicked
    /// </summary>
    private void OnGunReload()
    {
        if (!selectedGun.IsReloading() && selectedGun.SpareAmmo > 0 && Input.GetKeyUp(KeyCode.R))
        {
            if (!selectedGun.IsMagazineFull() && selectedGun.CanReload())
            {
                selectedGun.Reload();
            }
        }
    }

    private void FireProjectile()
    {
        // Calculate the spawn position using the child object's position and applying the offset in local space
        Vector3 spawnPosition = snowmanModel.transform.TransformPoint(new Vector3(-2.87f, 4.42f, 0.83f));

        // Since the snowmanModel's forward is not aligned as expected, let's use its right axis for the projectile's line of sight
        Quaternion spawnRotation = Quaternion.LookRotation(snowmanModel.transform.right);

        if (selectedGun is Pistol)
        {
            FirePistolProjectile(spawnPosition, spawnRotation);
        }
        else if (selectedGun is Shotgun)
        {
            FireShotgunProjectile(spawnPosition, spawnRotation);
        }
        else if (selectedGun is Smg)
        {
            FireSmgProjectile(spawnPosition, spawnRotation);
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
            rb.velocity = projectile.transform.forward * -1 * selectedGun.ProjectileVelocity;
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
                rb.velocity = adjustedDirection.normalized * selectedGun.ProjectileVelocity;
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
            rb.velocity = projectile.transform.forward * -1 * selectedGun.ProjectileVelocity;
        }
    }

    /// <summary>
    /// Kill this snowman instance
    /// </summary>
    public void Die()
    {
        Collider col = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (col != null && rb != null)
        {
            Destroy(col);
            Destroy(rb);
        }

        State._state.GameManagerRef.GetComponent<GameManager>().RemoveDeadSnowmanIdFromSpawnPoints(snowmanId);
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
        isAlive = false;
        snowmanId = -1;
        if (!isNpc)
        {
            State._state.PlayersSnowmanRef.Remove(gameObject);
        }
        Destroy(gameObject);
        //TODO
        //snowmanModel.transform.parent = null;
        //Destroy(snowmanModel, 10f);
        //Destroy(gameObject);

        GameObject snowman = State._state.GameManagerScriptObj.SpawnSnowman(isNpc, cylinderColor);
        if (!isNpc)
        {
            State._state.PlayersSnowmanRef.Add(snowman);
        }
    }

    /// <summary>
    /// Stops audio
    /// </summary>
    private void StopAudio() 
    {
        audioSource.Stop();
    }

    /// <summary>
    /// Plays reload audio clip for selected gun
    /// </summary>
    private void PlayReloadAudio()
    {
        AudioClip clipToPlay = null;
        float reloadTime = selectedGun.ReloadTimeSec;
        float clipLength = 0f;

        if (selectedGun is Pistol)
        {
            clipToPlay = pistolReloadAudio;
        }
        else if (selectedGun is Shotgun)
        {
            clipToPlay = shotgunReload;
        }
        else if (selectedGun is Smg)
        {
            clipToPlay = smgReloadAudio;
        }

        if (clipToPlay != null)
        {
            clipLength = clipToPlay.length;

            // Calculate the required pitch adjustment
            float pitch = clipLength / reloadTime;

            // Clamp the pitch to prevent extreme adjustments
            pitch = Mathf.Clamp(pitch, 0.5f, 2f);

            // Set the pitch on the audio source
            audioSource.pitch = pitch;

            // Play the audio clip
            audioSource.PlayOneShot(clipToPlay);

            // Start a coroutine to reset the pitch after the adjusted playback duration
            StartCoroutine(ResetAudioPitchAfterDelay(clipLength / pitch));
        }
    }

    private IEnumerator ResetAudioPitchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.pitch = 1f;
    }


    /// <summary>
    /// Plays shot audio clip for selected gun
    /// </summary>
    private void PlayShotAudio()
    {
        if (selectedGun is Pistol)
        {
            audioSource.PlayOneShot(pistolShotAudio);
        }
        else if (selectedGun is Shotgun)
        {
            float clipLength = shotgunShotAudio.length;
            // Calculate the required pitch adjustment
            float pitch = clipLength / selectedGun.FireDelayInSec;

            // Clamp the pitch to prevent extreme adjustments
            pitch = Mathf.Clamp(pitch, 0.5f, 2f);

            // Set the pitch on the audio source
            audioSource.pitch = pitch;

            // Play the audio clip
            audioSource.PlayOneShot(shotgunShotAudio);

            // Start a coroutine to reset the pitch after the adjusted playback duration
            StartCoroutine(ResetAudioPitchAfterDelay(clipLength / pitch));
        }
        else if (selectedGun is Smg)
        {
            audioSource.PlayOneShot(smgShotAudio);
        }
    }

    /// <summary>
    /// Play empty click audio clip for selected gun
    /// </summary>
    private void PlayEmptyAudio()
    {
        //prevent duplical plays
        if (audioSource.isPlaying)
        {
            return;
        }

        if (selectedGun is Pistol)
        {
            audioSource.PlayOneShot(pistolEmptyAudio);
        }
        else if (selectedGun is Shotgun)
        {
            audioSource.PlayOneShot(shotgunEmptyAudio);
        }        
        else if (selectedGun is Smg)
        {
            audioSource.PlayOneShot(smgEmptyAudio);
        } 
    }
}

public abstract class Gun : MonoBehaviour
{
    /// <summary>
    /// Name of the gun
    /// </summary>
    public string Name { get; set; }

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
            Debug.Log(Name + " fired. Ammo left: " + AmmoInMagazine);

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
            Debug.Log(Name + " is out of ammo. Reload needed.");
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
        Debug.Log(Name + " is reloading...");

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

        Debug.Log(Name + " reload complete.");
    }

    public virtual void CancelReload()
    {
        if (isReloading)
        {
            // Stop the reload coroutine
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
            isReloading = false;
            Debug.Log(Name + " reload canceled.");

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
            Debug.Log(Name + " firing delay canceled.");
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
        AmmoInMagazine = 12;
        MagazineCapacity = 12;
        SpareAmmo = 9999;
        IsAutomatic = false;
        ProjectileVelocity = 10;
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
        AmmoInMagazine = 6;
        MagazineCapacity = 6;
        SpareAmmo = 6;
        IsAutomatic = false;
        ProjectileVelocity = 10;
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
        AmmoInMagazine = 30000;
        MagazineCapacity = 30;
        SpareAmmo = 30;
        IsAutomatic = true;
        ProjectileVelocity = 10;
        ProjectileMass = 3;
        ReloadTimeSec = 6f;
        FireDelayInSec = 0.05f;
    }
}