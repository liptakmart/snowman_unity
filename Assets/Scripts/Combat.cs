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
        SubscribeToEvents();
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
        UnsubscribeFromEvents();
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

    private void SubscribeToEvents()
    {
        snowmanState.OnMyKill += HandleOnMyKill;
    }

    private void UnsubscribeFromEvents()
    {
        snowmanState.OnMyKill -= HandleOnMyKill;
    }

    private void HandleOnMyKill(int enemyId)
    {
        Debug.Log("Killed enemy:" + enemyId);
    }

    private void HandleReloadStarted()
    {
        //Debug.Log($"{selectedGun.Name} reload started.");
        // Play reload sound if applicable
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
        selectedGun.StopAudio();
        // Update UI if necessary
        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    private void HandleFired()
    {
        //Debug.Log($"{selectedGun.Name} fired.");
        // Play firing sound
        
        // Fire the projectile

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
                selectedGun.Fire(snowmanModel, snowmanState);
            }
        }
        else if (selectedGun.IsAutomatic && Input.GetKey(KeyCode.Space))
        {
            clickedFire = true;
            if (!selectedGun.IsEmptyMagazine())
            {
                selectedGun.Fire(snowmanModel, snowmanState);
            }
        }

        if (clickedFire && selectedGun.IsEmptyMagazine())
        {
            selectedGun.PlayEmptyAudio();
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

            selectedGun.StopAudio();
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
        selectedGun.StopAudio();
        Destroy(gameObject);

        //respawn
        GameObject snowman = manager.SpawnSnowman(false, teamId, snowmanId, GUN_TYPE.PISTOL, new GUN_TYPE[] { GUN_TYPE.PISTOL });
        levelState.PlayersSnowmanRef.Add(snowman);
    }
}