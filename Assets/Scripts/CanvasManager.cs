using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public GameObject ammoText;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateWeaponUI()
    {
        if (State._state.PlayerList.Count == 0)
        {
            return;
        }
        // Get the TextMeshProUGUI component from the GameObject
        TextMeshProUGUI ammoTextComponent = ammoText.GetComponent<TextMeshProUGUI>();

        // Set the text
        //TODO rework, it will only work if first spawned is player, not npc
        SnowmanState sc = State._state.PlayerList[0].GetComponent<SnowmanState>();
        Gun gun = sc.SelectedGun;
        ammoTextComponent.text = $"{gun.Name} {gun.AmmoInMagazine}/{gun.SpareAmmo}";
    }

    public void SetReloadingText()
    {
        TextMeshProUGUI ammoTextComponent = ammoText.GetComponent<TextMeshProUGUI>();
        ammoTextComponent.text = "Reloading";
    }
}
