using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Color gizmoColor = Color.red; // Color of the Gizmo
    public float gizmoRadius = 1.0f;     // Radius of the Gizmo

    // List of objects currently inside the trigger
    private HashSet<GameObject> objectsInside = new HashSet<GameObject>();

    // This method is called by Unity in the editor to draw gizmos in the Scene view
    void OnDrawGizmos()
    {
        // Set the Gizmo color
        Gizmos.color = gizmoColor;

        // Draw a wireframe sphere at the object's position
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }

    // Called when another object enters the trigger
    void OnTriggerEnter(Collider other)
    {
        if (!objectsInside.Contains(other.gameObject))
        {
            objectsInside.Add(other.gameObject);
        }
    }

    // Called when another object exits the trigger
    void OnTriggerExit(Collider other)
    {
        if (objectsInside.Contains(other.gameObject))
        {
            objectsInside.Remove(other.gameObject);
        }
    }

    // Optional: Method to check if the spawn point is free
    public bool IsEmpty()
    {
        return objectsInside.Count == 0;
    }
}