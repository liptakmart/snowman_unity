using System.Collections;
using System.Collections.Generic;
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
        // Get the TextMeshProUGUI component from the GameObject
        TextMeshProUGUI ammoTextComponent = ammoText.GetComponent<TextMeshProUGUI>();

        // Set the text
        SnowmanCombat sc = State._state.PlayersSnowmanRef[0].GetComponent<SnowmanCombat>();
        Gun gun = sc.selectedGun;
        ammoTextComponent.text = $"{gun.Name} {gun.AmmoInMagazine}/{gun.SpareAmmo}";
    }

    public void SetReloadingText()
    {
        TextMeshProUGUI ammoTextComponent = ammoText.GetComponent<TextMeshProUGUI>();
        ammoTextComponent.text = "Reloading";
    }
}
