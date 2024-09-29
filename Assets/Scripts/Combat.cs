using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;

public class Combat : MonoBehaviour
{
    private AudioSource audioSource;
    private SnowmanState snowmanState;
    private LevelState levelState;
    private Gun selectedGun;
    private List<Gun> ownedGuns;
    private GameObject snowmanModel;
    private GameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        snowmanState = GetComponent<SnowmanState>();
        selectedGun = snowmanState.SelectedGun;
        ownedGuns = snowmanState.OwnedGuns;
        levelState = State._state;
        snowmanModel = snowmanState.snowmanModel;
        manager = levelState.GameManagerScriptObj;

        SubscribeToGunEvents(snowmanState.SelectedGun);
        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (!snowmanState.IsAlive)
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
        snowmanState.IsAlive = false;
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
        //Debug.Log($"{selectedGun.Name} reload started.");
        // Play reload sound if applicable
        PlayReloadAudio();
        // Update UI if necessary
        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void HandleReloadFinished()
    {
        //Debug.Log($"{selectedGun.Name} reload finished.");
        // Update UI if necessary
        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void HandleReloadCanceled()
    {
        //Debug.Log($"{selectedGun.Name} reload canceled.");
        // Stop reload sound if applicable
        StopAudio();
        // Update UI if necessary
        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void HandleFired()
    {
        //Debug.Log($"{selectedGun.Name} fired.");
        // Play firing sound
        PlayShotAudio();
        // Fire the projectile
        FireProjectile();
        // Update UI if necessary
        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void HandleReadyToFire()
    {
        //Debug.Log($"{selectedGun.Name} is ready to fire again.");
        // Update UI if necessary
    }

    private void HandleAmmoChanged(int ammoInMagazine, int spareAmmo)
    {
        //Debug.Log($"{selectedGun.Name} ammo changed: {ammoInMagazine} / {spareAmmo}");
        // Update UI with new ammo counts
        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
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
                snowmanState.SelectedGun = ownedGuns[0];
            }
            else
            {
                snowmanState.SelectedGun = ownedGuns[selectedWeaponIdx + 1];
            }
            selectedGun = snowmanState.SelectedGun;

            selectedGun.GameObjectRef.SetActive(true);
            SubscribeToGunEvents(selectedGun);

            StopAudio();
            // Update UI
            levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
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
        GameObject projectilePrefab = State._prefabs.ProjectilePrefabRef;
        projectilePrefab.transform.localScale = new Vector3(selectedGun.ProjectileSize, selectedGun.ProjectileSize, selectedGun.ProjectileSize);

        var projectile = Instantiate(projectilePrefab, projectilePos, projectileRot);
        projectile.GetComponent<Projectile>().FiredBySnowmanId = snowmanState.SnowmanId;

        var rb = projectile.GetComponent<Rigidbody>();

        Vector3 direction = projectile.transform.forward * -1;
        float yDispersion = selectedGun.YBaseDispersion + (selectedGun.DynamicDispersion * 3f);
        float zDispersion = selectedGun.ZBaseDispersion + (selectedGun.DynamicDispersion * 3f);
        rb.velocity = ApplyDispersion(direction, yDispersion, zDispersion) * selectedGun.ProjectileVelocity;

        var script = projectile.GetComponent<Projectile>();
        script.MaxRange = selectedGun.MaxRange;
        script.OriginPoint = transform.position;
    }

    private void FireShotgunProjectile(Vector3 projectilePos, Quaternion projectileRot)
    {
        List<float> widthAngleOffsets = new List<float>();
        float yLowerBound = -6.0f;
        float yStep = 0.50f;
        for (float i = yLowerBound; i <= yLowerBound * -1; i+=yStep)
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
                projectilePrefab.transform.localScale = new Vector3(selectedGun.ProjectileSize, selectedGun.ProjectileSize, selectedGun.ProjectileSize);

                // Instantiate the projectile
                var projectile = Instantiate(projectilePrefab, projectilePos, projectileRot);
                projectile.GetComponent<Projectile>().FiredBySnowmanId = snowmanState.SnowmanId;

                var rb = projectile.GetComponent<Rigidbody>();
                // Original forward direction (negative due to your setup)
                Vector3 forward = projectile.transform.forward * -1;

                // Rotate the forward vector by the angle offset around the Y-axis
                Quaternion rotation = Quaternion.Euler(heightAnglesOffsets[i], widthAngleOffsets[j], 0);
                Vector3 adjustedDirection = rotation * forward;

                float yDispersion = selectedGun.YBaseDispersion + (selectedGun.DynamicDispersion * 3f);
                float zDispersion = selectedGun.ZBaseDispersion + (selectedGun.DynamicDispersion * 3f);

                // Apply the adjusted velocity
                rb.velocity = ApplyDispersion(adjustedDirection.normalized, yDispersion, zDispersion) * selectedGun.ProjectileVelocity;

                var script = projectile.GetComponent<Projectile>();
                script.MaxRange = selectedGun.MaxRange;
                script.OriginPoint = transform.position;
            }
        }
    }

    private void FireSmgProjectile(Vector3 projectilePos, Quaternion projectileRot)
    {
        GameObject projectilePrefab = State._prefabs.ProjectilePrefabRef;
        projectilePrefab.transform.localScale = new Vector3(selectedGun.ProjectileSize, selectedGun.ProjectileSize, selectedGun.ProjectileSize);

        var projectile = Instantiate(projectilePrefab, projectilePos, projectileRot);
        projectile.GetComponent<Projectile>().FiredBySnowmanId = snowmanState.SnowmanId;
        var rb = projectile.GetComponent<Rigidbody>();

        Vector3 direction = projectile.transform.forward * -1;
        float yDispersion = selectedGun.YBaseDispersion + (selectedGun.DynamicDispersion * 3f);
        float zDispersion = selectedGun.ZBaseDispersion + (selectedGun.DynamicDispersion * 3f);
        rb.velocity = ApplyDispersion(direction, yDispersion, zDispersion) * selectedGun.ProjectileVelocity;

        var script = projectile.GetComponent<Projectile>();
        script.MaxRange = selectedGun.MaxRange;
        script.OriginPoint = transform.position;
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
        float randomAngleY = Random.Range(-maxAngleY, maxAngleY);
        float randomAngleZ = Random.Range(-maxAngleZ, maxAngleZ);

        // Create a rotation based on the random angles
        //Quaternion dispersionRotation = Quaternion.Euler(randomAngleY, 0f, randomAngleZ);
        Quaternion dispersionRotation = Quaternion.Euler(0f, randomAngleY, randomAngleZ);

        // Apply the rotation to the original direction
        Vector3 dispersedDirection = dispersionRotation * originalDirection;

        return dispersedDirection;
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

        int snowmanId = snowmanState.SnowmanId;
        int teamId = snowmanState.TeamId;
        snowmanState.IsAlive = false;

        levelState.GameManagerRef.GetComponent<GameManager>().RemoveDeadSnowmanIdFromSpawnPoints(snowmanState.SnowmanId);
        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
        levelState.PlayersSnowmanRef.Remove(gameObject);
        StopAudio();
        Destroy(gameObject);

        //respawn
        GameObject snowman = manager.SpawnSnowman(false, teamId, snowmanId, GUN_TYPE.PISTOL, new GUN_TYPE[] { GUN_TYPE.PISTOL });
        levelState.PlayersSnowmanRef.Add(snowman);
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
            clipToPlay = snowmanState.pistolReloadAudio;
        }
        else if (selectedGun is Shotgun)
        {
            clipToPlay = snowmanState.shotgunReload;
        }
        else if (selectedGun is Smg)
        {
            clipToPlay = snowmanState.smgReloadAudio;
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
            audioSource.PlayOneShot(snowmanState.pistolShotAudio);
        }
        else if (selectedGun is Shotgun)
        {
            float clipLength = snowmanState.shotgunShotAudio.length;
            // Calculate the required pitch adjustment
            float pitch = clipLength / selectedGun.FireDelayInSec;

            // Clamp the pitch to prevent extreme adjustments
            pitch = Mathf.Clamp(pitch, 0.5f, 2f);

            // Set the pitch on the audio source
            audioSource.pitch = pitch;

            // Play the audio clip
            audioSource.PlayOneShot(snowmanState.shotgunShotAudio);

            // Start a coroutine to reset the pitch after the adjusted playback duration
            StartCoroutine(ResetAudioPitchAfterDelay(clipLength / pitch));
        }
        else if (selectedGun is Smg)
        {
            audioSource.PlayOneShot(snowmanState.smgShotAudio);
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
            audioSource.PlayOneShot(snowmanState.pistolEmptyAudio);
        }
        else if (selectedGun is Shotgun)
        {
            audioSource.PlayOneShot(snowmanState.shotgunEmptyAudio);
        }
        else if (selectedGun is Smg)
        {
            audioSource.PlayOneShot(snowmanState.smgEmptyAudio);
        }
    }
}
