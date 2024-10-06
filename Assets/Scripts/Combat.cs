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
    private SpawnManager spawnManager;

    public Gun SelectedGun { get { return this.selectedGun;} }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        snowmanState = GetComponent<SnowmanState>();
        selectedGun = snowmanState.SelectedGun;
        ownedGuns = snowmanState.OwnedGuns;
        levelState = State._state;
        snowmanModel = snowmanState.snowmanModel;
        spawnManager = levelState.SpawnManagerScriptObj;

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
        snowmanState.OnIKilledSomeone += HandleOnMyKill;
    }

    private void UnsubscribeFromEvents()
    {
        snowmanState.OnIKilledSomeone -= HandleOnMyKill;
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

    private bool CanThisPlayerFire(bool automaticGun)
    {
        int playerNum = snowmanState.PlayerNumber;
        if (automaticGun)
        {
            if (playerNum == 0 && Input.GetKey(KeyCode.Space) || playerNum == 1 && Input.GetKey(KeyCode.RightControl))
            {
                return true;
            }
        }
        else
        {
            if (playerNum == 0 && Input.GetKeyUp(KeyCode.Space) || playerNum == 1 && Input.GetKeyUp(KeyCode.RightControl))
            {
                return true;
            }
        }

        return false;
    }

    private void OnFireClick()
    {
        bool clickedFire = false;
        
        if (!selectedGun.IsAutomatic && CanThisPlayerFire(false))
        {
            clickedFire = true;
            if (!selectedGun.IsEmptyMagazine())
            {
                selectedGun.Fire(snowmanModel, snowmanState);
            }
        }
        else if (selectedGun.IsAutomatic && CanThisPlayerFire(true))
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

    private bool OnChangeWeapon()
    {
        int playerNum = snowmanState.PlayerNumber;
        if (playerNum == 0 && Input.GetKeyUp(KeyCode.Q))
        {
            return true;
        }
        else if (playerNum == 1 && Input.GetKeyUp(KeyCode.RightShift))
        {
            return true;
        }
        return false;
    }

    private void OnWeaponChange()
    {
        if (OnChangeWeapon())
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

    private bool OnReload()
    {
        int playerNum = snowmanState.PlayerNumber;
        if (playerNum == 0 && Input.GetKeyUp(KeyCode.R))
        {
            return true;
        }
        else if (playerNum == 1 && Input.GetKeyUp(KeyCode.RightAlt))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// On reload key clicked
    /// </summary>
    private void OnGunReload()
    {
        if (!selectedGun.IsReloading() && selectedGun.SpareAmmo > 0 && OnReload())
        {
            if (!selectedGun.IsMagazineFull() && selectedGun.CanReload())
            {
                selectedGun.Reload();
            }
        }
    }

    public void OnGunPicked(GUN_TYPE gunType)
    {
        snowmanState.AddGunOrAmmo(gunType);
        //Debug.Log("gun picked");
    }

    /// <summary>
    /// Kill this snowman instance
    /// </summary>
    public void Die()
    {
        //respawn
        spawnManager.RespawnSnowman(false, snowmanState.SnowmanId, snowmanState.TeamId);

        Collider col = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (col != null && rb != null)
        {
            Destroy(col);
            Destroy(rb);
        }

        snowmanState.IsAlive = false;

        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
        levelState.PlayerList.Remove(gameObject);
        selectedGun.StopAudio();
        Destroy(gameObject);
    }
}