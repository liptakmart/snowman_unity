using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    public Color gizmoColor = Color.blue; // Color of the Gizmo

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        // Set the Gizmo color with 50% transparency
        Color transparentColor = gizmoColor;
        transparentColor.a = 0.5f; // Set alpha (transparency) to 0.5

        // Apply the transparent color to the Gizmo
        Gizmos.color = transparentColor;

        // Draw a filled sphere at the object's position with the updated color
        Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
    }
}
