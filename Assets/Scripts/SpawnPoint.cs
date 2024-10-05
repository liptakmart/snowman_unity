using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Color gizmoColor = Color.red; // Color of the Gizmo
    public float gizmoRadius = 1.0f;     // Radius of the Gizmo

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

    private bool IsSnowman(GameObject go)
    {
        if (go.tag == Constants.TAG_PLAYER || go.tag == Constants.TAG_NPC)
        {
            return true;
        }
        return false;
    }

    private List<GameObject> objectsInside = new List<GameObject>();
    public void AddObjectInside(GameObject go)
    {
        if (!objectsInside.Contains(go))
        {
            objectsInside.Add(go);
        }
    }

    public GameObject GetIfSomeoneInside()
    {
        // Clean up destroyed objects before returning
        objectsInside.RemoveAll(obj => obj == null);
        if (objectsInside.Count > 0)
        {
            return objectsInside[0];
        }
        return null;
    }

    public List<GameObject> GetAllSnowmenInside()
    {
        // Clean up destroyed objects before returning
        objectsInside.RemoveAll(obj => obj == null);
        return new List<GameObject>(objectsInside);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsSnowman(other.gameObject) && !objectsInside.Contains(other.gameObject))
        {
            objectsInside.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectsInside.Contains(other.gameObject))
        {
            objectsInside.Remove(other.gameObject);
        }
    }
}
