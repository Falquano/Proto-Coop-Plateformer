using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoopCameraTracker : MonoBehaviour
{
    public Transform Player1;
    public Transform Player2;

    // Update is called once per frame
    void Update()
    {
        transform.position = (Player1.position + Player2.position) / 2f + Vector3.forward * -10f;
    }
}
