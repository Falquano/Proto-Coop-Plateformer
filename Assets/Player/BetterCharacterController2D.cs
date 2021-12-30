using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterCharacterController2D : ICharacterController2D
{
    private Player player;

    [Header("Jump")]
    [SerializeField] float jumpForce;
    bool jumped = false;
    [SerializeField] float horizontalJumpForce;
    [SerializeField] private float coyoteTimeLeft = -1;
    private float jumpBufferTimeLeft = -1;

    [Header("Movement")]
    [SerializeField] float groundHorizontalSpeed;
    [SerializeField] bool isUsingMaxHorizontalSpeed = false;
    [SerializeField] float maxHorizontalSpeed;

    [Header("Air Movement")]
    public bool canMoveInTheAir;
    [SerializeField] float airHorizontalSpeed;



    [Header("Ground Collision Detection")]
    [SerializeField] float isGroundedMinValue;

    // [Header("Wall Collision Detection")]
    // [SerializeField] private Transform[] WallCheckPositions;
    // [SerializeField] private LayerMask wallLayer;
    // [SerializeField] private float wallCheckRadius = .03f;

    [Header("Velocity cancels")]
    [SerializeField] private bool cancelWhenGrounded = false;



    private void Start()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {

        //Update movement
        UpdateMove();

        //Update jump
        UpdateJump();
    }

    public override void Jump()
    {
        jumpBufferTimeLeft = jumpBufferTime;
    }

    void UpdateJump()
    {
        
        //Update coyoteTimeLeft and jumpBufferTimeLeft if they are in use
        if (!IsGrounded && coyoteTimeLeft >= 0)
            coyoteTimeLeft -= Time.deltaTime;

        if (jumpBufferTimeLeft >= 0)
            jumpBufferTimeLeft -= Time.deltaTime;
        Debug.Log($"Coyote : {coyoteTimeLeft} \n Buffer : {jumpBufferTimeLeft} \n IsGrounded : {IsGrounded}");
        if ((coyoteTimeLeft >= 0 || IsGrounded) && jumpBufferTimeLeft >= 0 && !jumped)
        {
            if (coyoteTimeLeft >= 0)
                body.velocity = new Vector2(body.velocity.x, 0);
            
            body.AddForce(new Vector2(HorizontalMovement * horizontalJumpForce, jumpForce), ForceMode2D.Impulse); //Jump
            jumped = true;
            coyoteTimeLeft = -1; //Set it under 0 so no mistake is made
            jumpBufferTimeLeft = -1;
            IsGrounded = false;
        }
    }

    public override void UpdateMove()
    {

        if (IsGrounded || (!IsGrounded && canMoveInTheAir))
            body.AddForce(new Vector2(HorizontalMovement * (IsGrounded ? groundHorizontalSpeed : airHorizontalSpeed), 0) * Time.deltaTime, ForceMode2D.Force);
        //CLAMP
        if (isUsingMaxHorizontalSpeed)
            body.velocity = new Vector2(Mathf.Clamp(body.velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed), body.velocity.y);


        // Debug.Log(HorizontalMovement * (IsGrounded ? groundHorizontalSpeed : airHorizontalSpeed)* Time.deltaTime);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        bool previousGrounded = IsGrounded;
        IsGrounded = false;

        /* Debug only */
        foreach (ContactPoint2D ContactPoint in other.contacts)
        {
            if (ContactPoint.normal.normalized.y > isGroundedMinValue)
                IsGrounded = true;

            Debug.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y) - (ContactPoint.point - new Vector2(transform.position.x, transform.position.y)).normalized, Color.red, 0.5f);
        }

        if (previousGrounded == true && IsGrounded == false && !jumped)
            coyoteTimeLeft = coyoteTime;
        else
        {
            coyoteTimeLeft = -1;
        }
            
        if(IsGrounded)
            jumped = false;

        if (!previousGrounded && IsGrounded && HorizontalMovement == 0 && cancelWhenGrounded)
            body.velocity = new Vector2(0, body.velocity.y);
    }
    void OnCollisionExit2D(Collision2D other)
    {
        IsGrounded = false;
    }
}
