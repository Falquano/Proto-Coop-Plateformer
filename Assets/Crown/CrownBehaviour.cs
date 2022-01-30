using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownBehaviour : MonoBehaviour
{
    [SerializeField] float radius = 1.5f;
    [SerializeField] LayerMask layer;
    Collider2D body;
    List<ContactPoint2D> ctpts = new List<ContactPoint2D>();
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        body = GetComponent<Collider2D>();
    }
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        var ctbodies = Physics2D.OverlapCircleAll(transform.position, radius, layer);
        if(ctbodies.Length < 1) return;

        getCatcher(ctbodies);
    }

    void getCatcher(Collider2D[] cs2D)
    {
        GameObject currentCatcher = new GameObject();
        float currMaxVelocity = 0;
        foreach (Collider2D c2D in cs2D)
        {
            if(currentCatcher != c2D.gameObject && c2D.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude > currMaxVelocity)
            {
                currentCatcher = c2D.gameObject;
                currMaxVelocity = c2D.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude;
            }
                
        }


        Catch(currentCatcher);

    }

    void Catch(GameObject C)
    {
        C.GetComponent<BetterCharacterController2D>().isCrowned = true;
        C.GetComponent<Player>().SetCrown(true);
        Destroy(gameObject);
    }

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
