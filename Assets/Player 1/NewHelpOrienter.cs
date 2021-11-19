using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewHelpOrienter : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = .4f;

    private float currentVelocity;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
            player = GetComponentInParent<Player>();
        if (target == null)
            throw new System.Exception("HelpOrienter a besoin d'un empty gameobject \"cible\"");
    }

    // Update is called once per frame
    void Update()
    {
        target.up = player.AimDirection;

        transform.rotation = Quaternion.Euler(0, 0, Mathf.SmoothDampAngle(transform.eulerAngles.z, target.eulerAngles.z, ref currentVelocity, smoothTime));
    }

    public void ResetDirection()
    {
        transform.rotation = Quaternion.identity;
        target.rotation = Quaternion.identity;
        currentVelocity = 0f;
    }
}
