using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnowmanState : MonoBehaviour
{
    private static int _snowmanIdCounter = 0;

    /**
     * DRAG AND DROP REFS
     */
    public GameObject snowmanModel;

    public GameObject pistolGo;
    public GameObject shotgunGo;
    public GameObject smgGo;
    /**
     * END
     */

    public int AssignNewSnowmanId()
    {
        return _snowmanIdCounter++;
    }

    private int _snowmanId;
    public int SnowmanId
    {
        get
        {
            return _snowmanId;
        }
    }

    public bool IsAlive
    {
        get; set;
    }

    public bool IsNpc
    {
        get; set;
    }

    public int TeamId
    {
        get; set;
    }

    public List<Gun> OwnedGuns
    {
        get; set;
    }

    /// <summary>
    /// When this snowman kills someone, returns id of killed
    /// </summary>
    public event Action<int> OnIKilledSomeone;
    public void InvokeIKilledSomeone(int snowmanId)
    {
        OnIKilledSomeone?.Invoke(snowmanId);
    }

    public Gun SelectedGun { get; set; }

    /// <summary>
    /// To be called after Snowman has been instantiliazated
    /// </summary>
    /// <param name="snowmanId"></param>
    /// <param name="isNpc"></param>
    /// <param name="teamId"></param>
    /// <param name="selectedGun"></param>
    /// <param name="ownedGuns"></param>
    public void Initialize(int? snowmanId, bool isNpc, int teamId, GUN_TYPE selectedGun, GUN_TYPE[] ownedGuns)
    {
        if (snowmanId == null)
        {
            _snowmanId = AssignNewSnowmanId();
        }
        else
        {
            _snowmanId = snowmanId.Value;
        }
        IsAlive = true;
        IsNpc = isNpc;
        TeamId= teamId;
        OwnedGuns = new List<Gun>();

        //init player state if doesnt exist yet
        if (!IsNpc && State._state.PlayerStats.FirstOrDefault(x => x.Id == SnowmanId) == null)
        {
            State._state.PlayerStats.Add(new PlayerStat(SnowmanId, "TODO", TeamId, false, 0, 0, false));
        }

        foreach (var gun in ownedGuns)
        {
            AddGunOrAmmo(gun);
        }
        SelectedGun = OwnedGuns[Array.IndexOf(ownedGuns, selectedGun)];

        pistolGo.SetActive(false);
        shotgunGo.SetActive(false);
        smgGo.SetActive(false);

        if (selectedGun == GUN_TYPE.PISTOL)
        {
            pistolGo.SetActive(true);
        }
        else if (selectedGun == GUN_TYPE.SHOTGUN)
        {
            shotgunGo.SetActive(true);
        }
        else if (selectedGun == GUN_TYPE.SMG)
        {
            smgGo.SetActive(true);
        }

        GameObject cylinderGo = snowmanModel.transform.Find("Cylinder").gameObject;
        Renderer renderer = cylinderGo.GetComponent<Renderer>();
        Material newMaterial = new Material(renderer.material);
        if (teamId == 1)
        {
            newMaterial.color = Constants.TEAM_01_COLOR;
        }
        else if (teamId == 2 )
        {
            newMaterial.color = Constants.TEAM_02_COLOR;
        }
        else if (teamId == 3)
        {
            newMaterial.color = Constants.TEAM_03_COLOR;
        }
        renderer.material = newMaterial;
    }

    ///// <summary>
    ///// Increments kill leaderboards for this player
    ///// </summary>
    //public void IncKill()
    //{
    //    var player = State._state.PlayerStats.FirstOrDefault(x => x.Id == SnowmanId);
    //    if (player != null)
    //    {
    //        player.KillsCount++;
    //    }
    //    else
    //    {
    //        throw new UnityException("Cannot increment kill!");
    //    }
    //}

    ///// <summary>
    ///// Increments death leaderboards for this player
    ///// </summary>
    //public void IncDeath()
    //{
    //    var player = State._state.PlayerStats.FirstOrDefault(x => x.Id == SnowmanId);
    //    if (player != null)
    //    {
    //        player.DeathCount++;
    //    }
    //    else
    //    {
    //        throw new UnityException("Cannot increment kill!");
    //    }
    //}

    /// <summary>
    /// Adds gun or ammo if player already has this gun
    /// </summary>
    /// <param name="newGun"></param>
    public void AddGunOrAmmo(GUN_TYPE newGun)
    {
        var gun = OwnedGuns.FirstOrDefault(x => x.GunType == newGun);
        if (gun != null)
        {
            //add ammo
            gun.AddAmmo();
        }
        else
        {
            //add gun and ammo
            if (newGun == GUN_TYPE.PISTOL)
            {
                //Pistol go inherits from Gun object that inherit Monobehaviour, so it cannot be instantiated with new()
                GameObject pistolGunObject = new GameObject("PistolGun");
                pistolGunObject.transform.parent = this.transform;
                Pistol pistolObj = pistolGunObject.AddComponent<Pistol>();
                pistolObj.GameObjectRef = pistolGo; // Assign the visual model
                pistolObj.AudioSourceRef = GetComponent<AudioSource>();
                OwnedGuns.Add(pistolObj);

            }
            else if (newGun == GUN_TYPE.SHOTGUN)
            {
                //Shotgun go inherits from Gun object that inherit Monobehaviour, so it cannot be instantiated with new()
                GameObject shotgunGunObject = new GameObject("ShotgunGun");
                shotgunGunObject.transform.parent = this.transform;
                Shotgun shotgunObj = shotgunGunObject.AddComponent<Shotgun>();
                shotgunObj.GameObjectRef = shotgunGo;
                shotgunObj.AudioSourceRef = GetComponent<AudioSource>();
                OwnedGuns.Add(shotgunObj);
            }
            else if (newGun == GUN_TYPE.SMG)
            {
                GameObject smgGunObject = new GameObject("SmgGun");
                smgGunObject.transform.parent = this.transform;
                Smg smgObj = smgGunObject.AddComponent<Smg>();
                smgObj.GameObjectRef = smgGo;
                smgObj.AudioSourceRef = GetComponent<AudioSource>();
                OwnedGuns.Add(smgObj);
            }
        }
    }
}
