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
    bool wasGoingDown = true;

    [Header("Ground Collision Detection")]
    [SerializeField] float isGroundedMinValue;

    [Header("Wall Collision Detection")]
    [SerializeField] float isWallMinValue = 0.3f;
    [SerializeField] bool slowDownOnWalls = true;
    [SerializeField] float slowDownOnWallsMaxSpeed = 0f;

    [Header("Wall Jump")]
    [SerializeField] float wallJumpForce;
    [SerializeField] float horizontalWallJumpForce;

    WallDirection wallDirection;
    enum WallDirection
    {
        Left,
        Right
    }

    [Header("Strong Collisions")] //WIP
    [SerializeField] bool useStrongCollisions;
    [SerializeField] float minForceStrongCollision = 4f;

    [Header("Wall Grab")]
    [SerializeField] bool wallGrab = true;
    [SerializeField] bool slideIfPlayerIsOnTop = true;
    [SerializeField] float topDetectionMinValue = 0.3f;
    bool isPlayerOnTop = true;
    bool wasPlayerOnTop = true;
    float regularGravityScale = 0;
    [SerializeField] float timeBeforeWallGrabStopInSeconds = 3f;
    float currentTimeBeforeWallGrabStop = -1;


    [Header("Velocity cancels")]
    [SerializeField] private bool cancelWhenGrounded = false;
    [SerializeField] private bool cancelGroundedAndNoInput = false;
    [SerializeField] private bool cancelGroundedAndInvertedInput = true;
    [SerializeField] private bool cancelAirborneAndNoInput = false;
    [SerializeField] private bool cancelAirborneAndInvertedInput = true;
    [SerializeField] private bool cancelWhenWallJump = true;
    float previousHorizontalMovement;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        regularGravityScale = body.gravityScale;
    }
    private void Start()
    {
        player = GetComponent<Player>();
        
    }

    private void Update()
    {
        List<ContactPoint2D> other = new List<ContactPoint2D>();
        gameObject.GetComponent<Collider2D>().GetContacts(other);
        UpdateCollisions(other);

        //Update movement
        UpdateMove();

        //Update jump
        UpdateJump();

        previousHorizontalMovement = HorizontalMovement;
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

        //Debug.Log($"Coyote : {coyoteTimeLeft} \n Buffer : {jumpBufferTimeLeft} \n IsGrounded : {IsGrounded}");
        if ((coyoteTimeLeft >= 0 || IsGrounded) && jumpBufferTimeLeft >= 0 && !jumped)
        {
            if (coyoteTimeLeft >= 0)
                body.velocity = new Vector2(body.velocity.x, 0);

            body.AddForce(new Vector2(HorizontalMovement * horizontalJumpForce, jumpForce), ForceMode2D.Impulse); //Jump
            OnJump.Invoke();
            jumped = true;
            coyoteTimeLeft = -1; //Set it under 0 so no mistake is made
            jumpBufferTimeLeft = -1;
        }
        else if (!IsGrounded && jumpBufferTimeLeft >= 0 && !jumped && IsOnWall)
        {
            if (cancelWhenWallJump)
                body.velocity = Vector2.zero;

            body.AddForce(new Vector2(((wallDirection == WallDirection.Left) ? 1 : -1) * horizontalWallJumpForce, wallJumpForce), ForceMode2D.Impulse);
            jumped = true;
            coyoteTimeLeft = -1; //Set it under 0 so no mistake is made
            jumpBufferTimeLeft = -1;
        }

    }

    public override void UpdateMove()
    {
        var isDirWallSameAsControllerDir = (wallDirection == WallDirection.Left && HorizontalMovement < 0) || (wallDirection == WallDirection.Right && HorizontalMovement > 0);
        
        if(!(!isDirWallSameAsControllerDir && currentTimeBeforeWallGrabStop>0 && IsOnWall))
        currentTimeBeforeWallGrabStop -= Time.deltaTime;


        //CANCEL IF INVERTED MVMNT // IF 0 MVMNT // AIRBORNE/GROUNDED
        if ((((((previousHorizontalMovement > 0 && HorizontalMovement < 0) || (previousHorizontalMovement < 0 && HorizontalMovement > 0)) && cancelGroundedAndInvertedInput) || (HorizontalMovement == 0 && cancelGroundedAndNoInput)) && IsGrounded) || (((((previousHorizontalMovement > 0 && HorizontalMovement < 0) || (previousHorizontalMovement < 0 && HorizontalMovement > 0)) && cancelAirborneAndInvertedInput) || (HorizontalMovement == 0 && cancelAirborneAndNoInput)) && !IsGrounded))
        {
            body.velocity = new Vector2(0, body.velocity.y);
        }

        if (IsGrounded || (!IsGrounded && canMoveInTheAir))
            body.AddForce(new Vector2(HorizontalMovement * (IsGrounded ? groundHorizontalSpeed : airHorizontalSpeed), 0) * Time.deltaTime, ForceMode2D.Force);
        //CLAMP
        if (isUsingMaxHorizontalSpeed)
            body.velocity = new Vector2(Mathf.Clamp(body.velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed), body.velocity.y);

        

        //WALL GRAB SETUP
        if (!IsOnWall || isPlayerOnTop)
            currentTimeBeforeWallGrabStop = -1;

        //WALL GRAB START
        if ((body.velocity.y < 0 && IsOnWall && wallGrab && (!WasOnWall || !wasGoingDown)) && isDirWallSameAsControllerDir)
        {
            currentTimeBeforeWallGrabStop = timeBeforeWallGrabStopInSeconds;
        }

        //WALL GRAB
        if ((body.velocity.y <= 0.01 && IsOnWall && wallGrab && currentTimeBeforeWallGrabStop >= 0) && isDirWallSameAsControllerDir)
        {
            //STOP VELOCITY
            body.velocity = new Vector2(body.velocity.x, 0);
            body.gravityScale = 0;
            // body.AddForce(-Physics.gravity * body.mass);
        }
        else
        {
            body.gravityScale = regularGravityScale;
        }

        //WALL SLIDE
        if ((body.velocity.y < 0 && IsOnWall && slowDownOnWalls && currentTimeBeforeWallGrabStop < 0) && isDirWallSameAsControllerDir)
        {
            body.velocity = new Vector2(body.velocity.x, Mathf.Clamp(body.velocity.y, slowDownOnWallsMaxSpeed, 0f));
        }


        wasGoingDown = (body.velocity.y <= 0);
    }

    void UpdateCollisions(List<ContactPoint2D> contacts)
    {
        // GROUNDED
        WasGrounded = IsGrounded;
        IsGrounded = false;

        //WALL
        WasOnWall = IsOnWall;
        IsOnWall = false;

        //PLAYER ON TOP
        wasPlayerOnTop = isPlayerOnTop;
        isPlayerOnTop = false;

        foreach (ContactPoint2D ContactPoint in contacts)
        {
            //GROUND
            if (ContactPoint.normal.normalized.y > isGroundedMinValue)
                IsGrounded = true;
            //WALLS
            if (Mathf.Abs(ContactPoint.normal.y) < isWallMinValue && ContactPoint.rigidbody.gameObject.tag != gameObject.tag)
            {
                IsOnWall = true;
                wallDirection = (ContactPoint.point.x > transform.position.x) ? WallDirection.Right : WallDirection.Left;
            }
            //OTHER PLAYER ON TOP
            if (ContactPoint.normal.y < -topDetectionMinValue && ContactPoint.rigidbody.gameObject.tag == gameObject.tag) //If other player is on top
            {
                isPlayerOnTop = true;
            }

            Debug.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y) - (ContactPoint.point - new Vector2(transform.position.x, transform.position.y)).normalized, (ContactPoint.normal.normalized.y > isGroundedMinValue) ? Color.green : Color.red, 0.5f);
        }

        // Debug.Log(gameObject.name);
        // Debug.Log(IsGrounded);

        if (WasGrounded == true && IsGrounded == false && !jumped)
            coyoteTimeLeft = coyoteTime;

        else
        {
            coyoteTimeLeft = -1;
        }

        if (!WasGrounded && IsGrounded)
            OnLand.Invoke();


        if (IsGrounded || IsOnWall)
            jumped = false;

        if (!WasGrounded && IsGrounded && HorizontalMovement == 0 && cancelWhenGrounded)
            body.velocity = new Vector2(0, body.velocity.y);



    }
}
