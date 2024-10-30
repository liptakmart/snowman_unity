using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableGun : MonoBehaviour
{
    [Header("Rotation Parameters")]
    [Tooltip("Rotation speed around y-axis in degrees per second.")]
    public float rotationSpeed = 90f; // Add this variable

    [Header("Gun type")]
    [Tooltip("Gun type")]
    public GUN_TYPE GunType;

    void Update()
    {
        RotateObject();
    }

    private void RotateObject()
    {
        float rotationThisFrame = rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotationThisFrame, 0, Space.Self);
    }
}
