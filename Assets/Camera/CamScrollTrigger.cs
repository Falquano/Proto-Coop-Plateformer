using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScrollTrigger : MonoBehaviour
{
    [SerializeField] GameObject cam;
    [SerializeField] float timeToNextTarget;
    public GameObject nextTarget;
    SmoothFollow smFl;

    void Start()
    {
        smFl = cam.GetComponent<SmoothFollow>();
        transform.position = new Vector3(transform.position.x, transform.position.y, smFl.offsetPosition.z);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + new Vector3(GetComponent<BoxCollider2D>().offset.x, GetComponent<BoxCollider2D>().offset.y, 0), new Vector3(GetComponent<BoxCollider2D>().size.x, GetComponent<BoxCollider2D>().size.y, 0));
        Gizmos.DrawLine(transform.position, nextTarget.transform.position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        smFl.setCamScroll(nextTarget, timeToNextTarget);
    }
}
