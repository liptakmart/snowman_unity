using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSpawnPoint : MonoBehaviour
{
    public static int IdCounter = 0;
    public Color gizmoColor = Color.green; // Color of the Gizmo
    public float gizmoRadius = 1.0f;     // Radius of the Gizmo

    public int Id;
    public GameObject pickableGun;

    private void Awake()
    {
        Id= IdCounter++;
    }

    // Called when another object enters the trigger
    void OnTriggerStay(Collider other)
    {
        if (pickableGun == null)
        {
            return;
        }

        GUN_TYPE gunType = pickableGun.GetComponent<PickableGun>().GunType;

        if (other.tag == Constants.TAG_PLAYER)
        {
            var script = other.gameObject.GetComponent<Combat>();
            script.OnGunPicked(gunType);
            script.SelectedGun.PlayCockAudio();
            Destroy(pickableGun);
            pickableGun = null;
        }
        else if (other.tag == Constants.TAG_NPC)
        {
            var script = other.gameObject.GetComponent<NpcBehaviour>();
            script.OnGunPicked(gunType);
            script.SelectedGun.PlayCockAudio();
            Destroy(pickableGun);
            pickableGun = null;
        }
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    void OnDrawGizmos()
    {
        // Set the Gizmo color with 50% transparency
        Color transparentColor = gizmoColor;
        transparentColor.a = 0.5f; // Set alpha (transparency) to 0.5

        // Apply the transparent color to the Gizmo
        Gizmos.color = transparentColor;

        // Draw a filled sphere at the object's position with the updated color
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }
}
