using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    /// <summary>
    /// If projectile can still kill. When it kill someone it should become harmless.
    /// </summary>
    public bool IsLethal;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        IsLethal = false;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0f;
        rb.angularDrag = 0f;
        Destroy(this.gameObject, 10f);
    }


    // This method will be called when the projectile collides with another object
    private void OnCollisionEnter(Collision collision)
    {
        // Enable gravity so the projectile falls after hitting an object
        rb.useGravity = true;
    }
}
