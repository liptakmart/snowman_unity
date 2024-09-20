using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Color gizmoColor = Color.red; // Color of the Gizmo
    public float gizmoRadius = 1.0f;     // Radius of the Gizmo

    // List of objects currently inside the trigger
    private HashSet<int> snowmanIdsInside = new HashSet<int>();

    // This method is called by Unity in the editor to draw gizmos in the Scene view
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

    // Called when another object enters the trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == Constants.TAG_SNOWMAN && !snowmanIdsInside.Contains(other.gameObject.GetComponent<SnowmanCombat>().snowmanId))
        {
            snowmanIdsInside.Add(other.gameObject.GetComponent<SnowmanCombat>().snowmanId);
        }
    }

    // Called when another object exits the trigger
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == Constants.TAG_SNOWMAN && snowmanIdsInside.Contains(other.gameObject.GetComponent<SnowmanCombat>().snowmanId))
        {
            snowmanIdsInside.Remove(other.gameObject.GetComponent<SnowmanCombat>().snowmanId);
        }
    }

    /// <summary>
    /// Tries to remove snowman id from list of inside. For example if snowman is dead.
    /// </summary>
    /// <param name="snowmanId"></param>
    public void TryRemoveSnowman(int snowmanId)
    {
        if (snowmanIdsInside.Contains(snowmanId))
        {
            snowmanIdsInside.Remove(snowmanId);
        }
        
    }

    // Optional: Method to check if the spawn point is free
    public bool NoSnowmanInside()
    {
        return snowmanIdsInside.Count == 0;
    }
}
