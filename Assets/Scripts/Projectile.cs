using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    /// <summary>
    /// If projectile can still kill. When it kill someone it should become harmless.
    /// </summary>
    public bool IsLethal;
    /// <summary>
    /// Id of snowman which has fired this projectile
    /// </summary>
    public int FiredBySnowmanId;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        IsLethal = true;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0f;
        rb.angularDrag = 0f;

        int snowmanLayer = LayerMask.NameToLayer("SnowmanEO");
        int projectileLayer = LayerMask.NameToLayer("Projectile");

        // Ignore collisions between the projectile's layer and the ignore layer
        Physics.IgnoreLayerCollision(projectileLayer, snowmanLayer);
        //ignore self
        //Physics.IgnoreCollision(projectileCollider, ignoreCollider);

        Destroy(this.gameObject, 10f);
    }


    // This method will be called when the projectile collides with another object
    private void OnCollisionEnter(Collision collision)
    {
        // Enable gravity so the projectile falls after hitting an object
        rb.useGravity = true;
        //Debug.Log("Collision: " + collision.collider.gameObject.name);

        //get parent
        var hitSnowman = collision.gameObject.GetComponent<SnowmanCombat>();
        //notify snowman and disable projectile
        if (IsLethal && hitSnowman.isAlive && FiredBySnowmanId != hitSnowman.snowmanId)
        
            if (collision.collider.gameObject.name == "Cylinder")
            {
                //TODO disperse cylinder
            }
            else
            {
                hitSnowman.Die();
                //disperse enemy corpse
            }
        IsLethal = false;
    }
}