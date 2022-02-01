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
    [SerializeField] bool notUseMaxHorizontalSpeedIfOffering = true;
    [SerializeField] bool disableMovementIfOffering = true;

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
    [SerializeField] float slowDownOnWallsMaxSpeedPlayerOnTop = 0f;

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
    [SerializeField] bool boostOverrideAlwaysStrong = true;
    [SerializeField] bool fastfallOverrideAlwaysNotStrong = true;
    [SerializeField] bool allowFallingStrongCollisions = false;
    [SerializeField] bool loseCrownOnlyIfBoostedSC = true;
    [SerializeField] bool allowTech = true;
    [SerializeField] float techableWindowTimeSinceLastOffering = 0.1f;

    [Header("Wall Grab")]
    [SerializeField] bool wallGrab = true;
    [SerializeField] bool slideIfPlayerIsOnTop = true;
    [SerializeField] float topDetectionMinValue = 0.3f;
    bool isPlayerOnTop = true;
    bool wasPlayerOnTop = true;
    float regularGravityScale = 0;
    [SerializeField] float timeBeforeWallGrabStopInSeconds = 3f;
    float currentTimeBeforeWallGrabStop = -1;
    bool isDirWallSameAsControllerDir;


    [Header("Velocity cancels")]
    [SerializeField] private bool cancelWhenGrounded = false;
    [SerializeField] private bool noCancelWhenGroundedIfFastFall = true;
    [SerializeField] private bool cancelGroundedAndNoInput = false;
    [SerializeField] private bool cancelGroundedAndInvertedInput = true;
    [SerializeField] private bool cancelAirborneAndNoInput = false;
    [SerializeField] private bool cancelAirborneAndInvertedInput = true;
    [SerializeField] private bool cancelWhenWallJump = true;
    [SerializeField] private bool cancelFastFall = true;
    [SerializeField] private bool cancelFastFallWhenOfferingHelp = true;
    [SerializeField] private bool cancelOnTech = true;
    [SerializeField] private bool cancelOnStun = true;
    float previousHorizontalMovement;

    [Header("Fast fall")]
    [SerializeField] bool canFastFall = true;
    bool isFastFalling = false;
    bool fastFallRegistered;
    [SerializeField] float fastFallDownwardForce = 3f;
    [SerializeField] float fastFallLateralForce = 0f;

    [Header("Crown")]
    [SerializeField] float crownGroundSpeedMultiplier = 1f;
    [SerializeField] float crownAirSpeedMultiplier = 1f;
    [SerializeField] float crownMaxSpeedMultiplier = 1f;
    [SerializeField] float crownJumpMultiplier = 1f;
    [SerializeField] float crownHorizontalJumpMultiplier = 1f;
    [SerializeField] float crownWallJumpMultiplier = 1f;
    [SerializeField] float crownHorizontalWallJumpMultiplier = 1f;
    bool wasCrowned = false;
    public GameObject crownGO {set;get;}

    [Header("Stun")]
    [SerializeField] float stunTimeInSecond;
    [SerializeField] float stunSpeedMultiplier;
    bool isStunned;
    float stunTimeLeft = 0f;



    //BOOST
    bool isBoostState = false;
    bool wasBoostState = false;

    //OFFERING
    bool isOfferingState = false;
    bool wasOfferingState = false;
    float timeSinceLastOffering = -1f;


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
        isDirWallSameAsControllerDir = (wallDirection == WallDirection.Left && HorizontalMovement < 0) || (wallDirection == WallDirection.Right && HorizontalMovement > 0);
        isBoostState = (player.State == PlayerState.Boost);
        isOfferingState = (player.State == PlayerState.OfferingHelp);

        timeSinceLastOffering -= Time.deltaTime;
        stunTimeLeft -= Time.deltaTime;

        if (stunTimeLeft <= 0 && isStunned)
            EndStun();

        if (!wasOfferingState && isOfferingState)
            timeSinceLastOffering = techableWindowTimeSinceLastOffering;

        if (wasCrowned != isCrowned)
        {
            Debug.Log(isCrowned ? "Couronné" : "A perdu la couronne");
        }

        List<ContactPoint2D> other = new List<ContactPoint2D>();
        gameObject.GetComponent<Collider2D>().GetContacts(other);
        UpdateCollisions(other);

        //Update movement
        UpdateMove();

        //Update jump
        if (!isStunned) UpdateJump();

        previousHorizontalMovement = HorizontalMovement;
        wasCrowned = isCrowned;
        wasBoostState = isBoostState;
        wasOfferingState = isOfferingState;
    }

    public override void Jump()
    {
        jumpBufferTimeLeft = jumpBufferTime;
    }

    public override void FastFall()
    {
        Debug.Log("Fast fall");
        if (!IsGrounded)
        {
            fastFallRegistered = true;
        }

    }

    void UpdateJump()
    {

        //Update coyoteTimeLeft and jumpBufferTimeLeft if they are in use
        if (!IsGrounded && coyoteTimeLeft >= 0)
            coyoteTimeLeft -= Time.deltaTime;

        if (jumpBufferTimeLeft >= 0)
            jumpBufferTimeLeft -= Time.deltaTime;

        if ((coyoteTimeLeft >= 0 || IsGrounded) && jumpBufferTimeLeft >= 0 && !jumped)
        {
            if (coyoteTimeLeft >= 0)
                body.velocity = new Vector2(body.velocity.x, 0);

            body.AddForce(new Vector2(HorizontalMovement * horizontalJumpForce * (isCrowned ? crownHorizontalJumpMultiplier : 1), jumpForce * (isCrowned ? crownJumpMultiplier : 1)), ForceMode2D.Impulse); //Jump
            OnJump.Invoke();
            jumped = true;
            coyoteTimeLeft = -1; //Set it under 0 so no mistake is made
            jumpBufferTimeLeft = -1;
        }
        else if (!IsGrounded && jumpBufferTimeLeft >= 0 && !jumped && IsOnWall && isDirWallSameAsControllerDir)
        {
            if (cancelWhenWallJump)
                body.velocity = Vector2.zero;

            body.AddForce(new Vector2(((wallDirection == WallDirection.Left) ? 1 : -1) * horizontalWallJumpForce * (isCrowned ? crownHorizontalWallJumpMultiplier : 1), wallJumpForce * (isCrowned ? crownWallJumpMultiplier : 1)), ForceMode2D.Impulse);  //Wall Jump
            OnJump.Invoke();
            jumped = true;
            coyoteTimeLeft = -1; //Set it under 0 so no mistake is made
            jumpBufferTimeLeft = -1;
        }
    }

    public override void UpdateMove()
    {

        //FASTFALL
        if (fastFallRegistered && !isFastFalling && canFastFall)
        {
            isFastFalling = true;
            if (cancelFastFall)
                body.velocity = new Vector2(body.velocity.x, 0);

            body.AddForce(new Vector2(HorizontalMovement * fastFallLateralForce, -fastFallDownwardForce), ForceMode2D.Impulse);
        }

        fastFallRegistered = false;

        //FASTFALL CANCEL
        if (isFastFalling && isOfferingState && !wasOfferingState && cancelFastFallWhenOfferingHelp)
        {
            body.velocity = new Vector2(body.velocity.x, 0);
            isFastFalling = false;
        }


        if (!(!isDirWallSameAsControllerDir && currentTimeBeforeWallGrabStop > 0 && IsOnWall))
            currentTimeBeforeWallGrabStop -= Time.deltaTime;


        //CANCEL IF INVERTED MVMNT // IF 0 MVMNT // AIRBORNE/GROUNDED
        if ((((((previousHorizontalMovement > 0 && HorizontalMovement < 0) || (previousHorizontalMovement < 0 && HorizontalMovement > 0)) && cancelGroundedAndInvertedInput) || (HorizontalMovement == 0 && cancelGroundedAndNoInput)) && IsGrounded) || (((((previousHorizontalMovement > 0 && HorizontalMovement < 0) || (previousHorizontalMovement < 0 && HorizontalMovement > 0)) && cancelAirborneAndInvertedInput) || (HorizontalMovement == 0 && cancelAirborneAndNoInput)) && !IsGrounded))
        {
            body.velocity = new Vector2(0, body.velocity.y);
        }

        if ((IsGrounded || (!IsGrounded && canMoveInTheAir)) && (!(disableMovementIfOffering) || !(disableMovementIfOffering && isOfferingState)))
            body.AddForce(new Vector2(HorizontalMovement * (IsGrounded ? groundHorizontalSpeed * (isCrowned ? crownGroundSpeedMultiplier : 1) : airHorizontalSpeed * (isCrowned ? crownAirSpeedMultiplier : 1)) * (isStunned ? stunSpeedMultiplier : 1), 0) * Time.deltaTime, ForceMode2D.Force);
        //CLAMP
        if (isUsingMaxHorizontalSpeed && (!(notUseMaxHorizontalSpeedIfOffering) || (notUseMaxHorizontalSpeedIfOffering && !isOfferingState)))
            body.velocity = new Vector2(Mathf.Clamp(body.velocity.x, -maxHorizontalSpeed * (isCrowned ? crownMaxSpeedMultiplier : 1), maxHorizontalSpeed * (isCrowned ? crownMaxSpeedMultiplier : 1)), body.velocity.y);



        //WALL GRAB SETUP
        if (!IsOnWall || ((!slideIfPlayerIsOnTop) || (isPlayerOnTop && slideIfPlayerIsOnTop)))
            currentTimeBeforeWallGrabStop = -1;

        //WALL GRAB START
        if ((body.velocity.y < 0 && IsOnWall && wallGrab && (!WasOnWall || !wasGoingDown)) && isDirWallSameAsControllerDir && !IsGrounded)
        {
            currentTimeBeforeWallGrabStop = timeBeforeWallGrabStopInSeconds;
        }

        //WALL GRAB
        if ((body.velocity.y <= 0.01 && IsOnWall && wallGrab && currentTimeBeforeWallGrabStop >= 0) && isDirWallSameAsControllerDir && !IsGrounded)
        {
            //STOP VELOCITY
            body.velocity = new Vector2(body.velocity.x, 0);
            body.gravityScale = 0;
        }
        else
        {
            body.gravityScale = regularGravityScale;
        }

        //WALL SLIDE
        if ((body.velocity.y < 0 && IsOnWall && slowDownOnWalls && currentTimeBeforeWallGrabStop < 0) && isDirWallSameAsControllerDir && !IsGrounded)
        {
            body.velocity = new Vector2(body.velocity.x, Mathf.Clamp(body.velocity.y, (isPlayerOnTop) ? slowDownOnWallsMaxSpeedPlayerOnTop : slowDownOnWallsMaxSpeed, 0f));
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
            bool hasAnalysed = false;
            //GROUND
            if (ContactPoint.normal.normalized.y > isGroundedMinValue)
            {
                IsGrounded = true;

                if (!WasGrounded && allowFallingStrongCollisions)
                {
                    collisionAnalysis(ContactPoint, ((ContactPoint.otherCollider.gameObject.tag == gameObject.tag) ? CollisionType.Player : CollisionType.Other));
                }
                hasAnalysed = true; //Outside so it doesn't analyse ground unless it gets grounded
            }
            //WALLS
            if (Mathf.Abs(ContactPoint.normal.y) < isWallMinValue && ContactPoint.rigidbody.gameObject.tag != gameObject.tag)
            {
                IsOnWall = true;
                wallDirection = (ContactPoint.point.x > transform.position.x) ? WallDirection.Right : WallDirection.Left;

                if (!WasOnWall)
                {
                    hasAnalysed = true;
                    collisionAnalysis(ContactPoint, CollisionType.Wall);
                }
            }
            //OTHER PLAYER ON TOP
            if (ContactPoint.normal.y < -topDetectionMinValue && ContactPoint.rigidbody.gameObject.tag == gameObject.tag) //If other player is on top
            {
                isPlayerOnTop = true;

                if (!WasGrounded)
                {
                    collisionAnalysis(ContactPoint, CollisionType.Player);
                    hasAnalysed = true;
                }
            }

            if (!hasAnalysed && isBoostState)
                collisionAnalysis(ContactPoint, CollisionType.Other);

            Debug.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y) - (ContactPoint.point - new Vector2(transform.position.x, transform.position.y)).normalized, (ContactPoint.normal.normalized.y > isGroundedMinValue) ? Color.green : Color.red, 0.5f);
        }

        if (!WasGrounded && IsGrounded)
        {
            OnLand.Invoke();
        }

        //GROUNDED CANCEL / CANCELED IF FAST FALLING (Allows jump cancelable dashes lol)
        if ((!WasGrounded && IsGrounded && HorizontalMovement == 0 && cancelWhenGrounded) && ((!noCancelWhenGroundedIfFastFall) || !(noCancelWhenGroundedIfFastFall && isFastFalling)))
        {
            body.velocity = new Vector2(0, body.velocity.y);
        }

        if ((!WasGrounded && IsGrounded) || (!WasOnWall && IsOnWall))
        {
            jumped = false;
            isFastFalling = false;
        }




        if (WasGrounded && !IsGrounded && !jumped)
        {
            coyoteTimeLeft = coyoteTime;
        }

    }


    float impactStrength(ContactPoint2D cp)
    {
        if (cp.normalImpulse > 0.01)
        {
            // Debug.Log(cp.normalImpulse);
        }
        return cp.normalImpulse;
    }

    void collisionAnalysis(ContactPoint2D contactPoint, CollisionType ct)
    {
        if (!useStrongCollisions)
            return;

        if (fastfallOverrideAlwaysNotStrong && isFastFalling)
            return;

        if (isBoostState && boostOverrideAlwaysStrong)
        {
            OnCollision.Invoke(impactStrength(contactPoint), ct, isBoostState);
            strongCollision();
            return;
        }


        if (!(impactStrength(contactPoint) > minForceStrongCollision))
            return;


        OnCollision.Invoke(impactStrength(contactPoint), ct, isBoostState);

        if (loseCrownOnlyIfBoostedSC && !isBoostState)
            return;


        strongCollision();

    }


    void strongCollision()
    {
        isCrowned = false;
        player.SetCrown(false);
        Destroy(crownGO);
        //Ignore strong collision on tech
        if (timeSinceLastOffering > 0 && allowTech)
        {
            if (cancelOnTech)
                body.velocity = Vector2.zero;
            return;
        }
        Stun();
    }


    void Stun()
    {
        if (cancelOnStun)
            body.velocity = Vector2.zero;

        player.State = PlayerState.Stun;
        isStunned = true;
        stunTimeLeft = stunTimeInSecond;
    }
    void EndStun()
    {
        isStunned = false;
        player.State = PlayerState.Moving;
    }
}
