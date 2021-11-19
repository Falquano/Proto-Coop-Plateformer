using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ICharacterController2D : MonoBehaviour
{
    public float HorizontalMovement { get; set; }
    [SerializeField] protected float coyoteTime;
    [SerializeField] protected float jumpBufferTime;

    // Il faut aussi s'occuper des ground checks

    public abstract void Jump();
}