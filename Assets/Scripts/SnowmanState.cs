using System;
using System.Collections;
using System.Collections.Generic;
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

    public AudioClip pistolShotAudio;
    public AudioClip pistolReloadAudio;
    public AudioClip pistolEmptyAudio;

    public AudioClip smgShotAudio;
    public AudioClip smgReloadAudio;
    public AudioClip smgEmptyAudio;

    public AudioClip shotgunShotAudio;
    public AudioClip shotgunReload;
    public AudioClip shotgunEmptyAudio;
    /**
     * END
     */

    public int GetNewSnowmanId()
    {
        return _snowmanIdCounter++;
    }

    public int SnowmanId
    {
        get; set;
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
            SnowmanId = GetNewSnowmanId();
        }
        else
        {
            SnowmanId = snowmanId.Value;
        }
        IsAlive = true;
        IsNpc = isNpc;
        TeamId= teamId;
        OwnedGuns = new List<Gun>();
        foreach (var gun in ownedGuns)
        {
            if (gun == GUN_TYPE.PISTOL)
            {
                //Pistol go inherits from Gun object that inherit Monobehaviour, so it cannot be instantiated with new()
                GameObject pistolGunObject = new GameObject("PistolGun");
                pistolGunObject.transform.parent = this.transform;
                Pistol pistolObj = pistolGunObject.AddComponent<Pistol>();
                pistolObj.GameObjectRef = pistolGo; // Assign the visual model
                OwnedGuns.Add(pistolObj);

            }
            else if (gun == GUN_TYPE.SHOTGUN)
            {
                //Shotgun go inherits from Gun object that inherit Monobehaviour, so it cannot be instantiated with new()
                GameObject shotgunGunObject = new GameObject("ShotgunGun");
                shotgunGunObject.transform.parent = this.transform;
                Shotgun shotgunObj = shotgunGunObject.AddComponent<Shotgun>();
                shotgunObj.GameObjectRef = shotgunGo;
                OwnedGuns.Add(shotgunObj);
            }
            else if (gun == GUN_TYPE.SMG)
            {
                GameObject smgGunObject = new GameObject("SmgGun");
                smgGunObject.transform.parent = this.transform;
                Smg smgObj = smgGunObject.AddComponent<Smg>();
                smgObj.GameObjectRef = smgGo;
                OwnedGuns.Add(smgObj);
            }
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
        else
        {
            newMaterial.color = Constants.TEAM_02_COLOR;
        }
        renderer.material = newMaterial;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
