using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBoundaries : MonoBehaviour
{
    [SerializeField] GameObject cam;
    SmoothFollow smFl;
    public Collider2D boundaries;
    [SerializeField] bool dontSetIfCamScroll;
    [SerializeField] bool dontSetIfNotCamera;

    void Start()
    {
        smFl = cam.GetComponent<SmoothFollow>();
        boundaries = GetComponent<Collider2D>();
        transform.position = new Vector3(transform.position.x, transform.position.y, smFl.offsetPosition.z);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + new Vector3(GetComponent<BoxCollider2D>().offset.x, GetComponent<BoxCollider2D>().offset.y, 0), new Vector3(GetComponent<BoxCollider2D>().size.x, GetComponent<BoxCollider2D>().size.y, 0));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(((smFl.cameraMode != SmoothFollow.CameraMode.AutoScroll && dontSetIfCamScroll) || !dontSetIfCamScroll) && ((other.gameObject == cam && dontSetIfNotCamera) || !dontSetIfNotCamera))
            smFl.setCamBoundaries(boundaries);
    }
}