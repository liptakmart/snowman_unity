using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        Physics.IgnoreLayerCollision(projectileLayer, projectileLayer);

        Destroy(this.gameObject, 10f);
    }


    // This method will be called when the projectile collides with another object
    private void OnCollisionEnter(Collision collision)
    {
        // Enable gravity so the projectile falls after hitting an object
        rb.useGravity = true;
        //Debug.Log("Collision: " + collision.collider.gameObject.name);

        if (collision.gameObject.tag != Constants.TAG_PLAYER && collision.gameObject.tag != Constants.TAG_NPC)
        {
            IsLethal = false;
            return;
        }

        //get parent
        var snowmanState = collision.gameObject.GetComponent<SnowmanState>();
        //notify snowman and disable projectile
        if (snowmanState != null && IsLethal && snowmanState.IsAlive && FiredBySnowmanId != snowmanState.SnowmanId)
        {
            if (collision.collider.gameObject.name == "Cylinder")
            {
                var cylinderGo = collision.collider.gameObject;
                cylinderGo.transform.parent = null;
                cylinderGo.AddComponent<Rigidbody>();
                Destroy(cylinderGo, 10f);
            }
            else
            {
                //disperse enemy corpse
                var modelParent = snowmanState.snowmanModel;
                var cylinderGo = modelParent.transform.Find("Cylinder").gameObject;
                var legsGo = modelParent.transform.Find("Legs").gameObject;
                var bodyGo = modelParent.transform.Find("Body").gameObject;
                var headGo = modelParent.transform.Find("Head").gameObject;
                var gunsGo = modelParent.transform.Find("Guns").gameObject;

                cylinderGo.transform.parent = null;
                legsGo.transform.parent = null;
                bodyGo.transform.parent = null;
                headGo.transform.parent = null;
                gunsGo.transform.parent = null;

                cylinderGo.AddComponent<Rigidbody>();
                legsGo.AddComponent<Rigidbody>();
                bodyGo.AddComponent<Rigidbody>();
                headGo.AddComponent<Rigidbody>();
                gunsGo.AddComponent<Rigidbody>();

                Destroy(cylinderGo, 10f);
                Destroy(legsGo, 10f);
                Destroy(bodyGo, 10f);
                Destroy(headGo, 10f);
                Destroy(gunsGo, 0f);

                if (snowmanState.IsNpc)
                {
                    var npcBehaviour = collision.gameObject.GetComponent<NpcBehaviour>();
                    npcBehaviour.Die();
                }
                else
                {
                    var snowmanCombat = collision.gameObject.GetComponent<Combat>();
                    snowmanCombat.Die();
                }
            }
        }
        IsLethal = false;
    }
}