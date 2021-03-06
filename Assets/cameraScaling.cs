using System.Collections;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEngine;
using UnityEngine.Tilemaps;
[ExecuteInEditMode]
public class cameraScaling : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] PixelPerfectCamera ppc;
    [SerializeField] Vector2 boundarySize;
    [SerializeField] bool activated = true;
    [SerializeField] Tilemap tm;
    [SerializeField] float zoom;


    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void LateUpdate()
    {
        if (!activated) return;

        ppc.assetsPPU = (int)(Screen.width / tm.size.x * zoom);

        //Debug.Log($"Screen width : {Screen.width}, Tilemap size : {tm.size.x}, assets ppu : {ppc.assetsPPU}");
        //Debug.Log((Screen.width > Screen.height) ? 1 : 2);
    }


    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(boundarySize.x, boundarySize.y, 0));
    }
}
