using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayerOutsideCameraBounds : MonoBehaviour
{
    private PlayerManager playerManager;
    private Camera mainCamera;

    private float rangeOutsideCamera = 1f;

    private void Start()
    {
        playerManager = GameObject.Find("@Player Manager").GetComponent<PlayerManager>();
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
            throw new System.Exception("\"KillPlayerOutsideCameraBounds\" doit être sur un component caméra !");
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Player player in playerManager.Players)
        {
            Vector3 screenpoint = mainCamera.WorldToScreenPoint(player.transform.position);
            if (screenpoint.x < -rangeOutsideCamera || screenpoint.y < -rangeOutsideCamera
                || screenpoint.x > mainCamera.pixelWidth + rangeOutsideCamera
                || screenpoint.y > mainCamera.pixelHeight + rangeOutsideCamera)
            {
                player.Respawn();
            }
        }
    }
}
