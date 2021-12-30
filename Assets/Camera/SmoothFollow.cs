using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    bool camMovementEnabled = true;
    [Header("Mode")]
    public CameraMode cameraMode;
    
    public enum CameraMode
    {
        Follow,
        AutoScroll
    }
    public CoopCameraModes CoopCameraMode { get => coopCameraMode; set => coopCameraMode = value; }
    [SerializeField] private CoopCameraModes coopCameraMode;
    public enum CoopCameraModes
    {
        AverageXY,
        AverageX,
        FollowPlayerOne,
        Disabled
    }

    [Header("Positionning")]
    ///<summary>
    /// The target's GameObject
    /// </summary>
    [SerializeField] GameObject target;

    [SerializeField] GameObject mainTarget;

    [Header("Coop positionning")]
    [SerializeField] Vector3 coopTarget;
    [SerializeField] int mainPlayerFocus = 0;
    [SerializeField] PlayerManager players;

    [Header("Boundaries")]
    ///<summary>
    /// The camera boundaries' representation as a Collider2D
    /// </summary>
    public Collider2D cameraBoundaries;

    [Header("Relative positionning")]
    ///<summary>
    /// The camera's offset to the target
    /// </summary>
    public Vector3 offsetPosition;

    [Header("Smoothing")]
    [SerializeField] float camSmoothTime;
    [SerializeField] float camMaxSpeed;

    [Header("Rotation")]
    [SerializeField] bool fixPlayerX = false;
    [SerializeField] bool fixPlayerY = false;
    Vector3 velocityRef = Vector3.zero;
    [SerializeField] bool fixPlayerXOnlyWhenOOB = false; //OOB == Out of bounds
    [SerializeField] bool fixPlayerYOnlyWhenOOB = false;
    [SerializeField] float maxDegreesDelta = 5f;

    //Scrolling
    float scrollingMaxSpeed;

    void Start()
    {
        if(!GetComponent<Collider2D>())
            Debug.LogError("No collider detected, some function might not be able to work properly");
    }

    void Awake()
    {
        players ??= GameObject.Find("@Player Manager").GetComponent<PlayerManager>();
        //If no player component, deactivate coop
        if(players==null) coopCameraMode = CoopCameraModes.Disabled;
    }

    void Update()
    {
        if (coopCameraMode != CoopCameraModes.Disabled && camMovementEnabled && players)
        {
            switch (coopCameraMode)
            {
                case CoopCameraModes.AverageX:
                    UpdateAverageX();
                    break;
                case CoopCameraModes.FollowPlayerOne:
                    UpdateFollowPlayerOne(mainPlayerFocus);
                    break;
                default:
                    UpdateAverageXY();
                    break;  
            }
        }

        // if (!target  && coopCameraMode == CoopCameraModes.Disabled && camMovementEnabled)
        //     camMovementEnabled = false;
        if (camMovementEnabled)
        {
            switch (cameraMode)
            {
                //FOLLOW
                case CameraMode.Follow:
                    UpdateCameraFollow();
                    break;
                case CameraMode.AutoScroll:
                    UpdateCameraScroll();
                    break;
            }
        }

        
    }

    private void UpdateAverageXY()
    {
        coopTarget = new Vector3(GetAveragePlayerPosition().x, GetAveragePlayerPosition().y, offsetPosition.z);
    }

    private void UpdateAverageX()
    {
        coopTarget = new Vector3(GetAveragePlayerPosition().x, transform.position.y, offsetPosition.z);
    }

    private void UpdateFollowPlayerOne(int player)
    {
        // Pas tr�s propre, faudrait pouvoir choisir le joueur. #TODO
        coopTarget = new Vector3(players.Players[player].transform.position.x, players.Players[player].transform.position.y, offsetPosition.z);
    }

    public Vector3 GetAveragePlayerPosition()
    {
        Vector3 average = Vector3.zero;
        foreach(Player player in players.Players)
        {
            average += player.transform.position;
        }
        return average / players.Count;
    }

    void UpdateCameraFollow()
    {
        //Calculate next position
        var targetPosition = ((coopCameraMode != CoopCameraModes.Disabled)?(coopTarget):(target.transform.position + offsetPosition));
        //Make sure it stays inbound
        if (!cameraBoundaries.bounds.Contains(targetPosition))
            targetPosition = cameraBoundaries.bounds.ClosestPoint(targetPosition);
        //Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocityRef, camSmoothTime, camMaxSpeed);
        //Debug
        Debug.DrawLine(transform.position, targetPosition, Color.green, 0.5f);



        var playerTargetPos = new Vector3((coopCameraMode != CoopCameraModes.Disabled)?coopTarget.x:target.transform.position.x, (coopCameraMode != CoopCameraModes.Disabled)?coopTarget.y:target.transform.position.y, cameraBoundaries.transform.position.z); //Z is not 0 so it can detect if in bounds next
        if (fixPlayerXOnlyWhenOOB)
        {
            fixPlayerX = !cameraBoundaries.bounds.Contains(playerTargetPos) && cameraBoundaries.bounds.ClosestPoint(playerTargetPos).x != playerTargetPos.x;
        }

        if (fixPlayerYOnlyWhenOOB)
        {
            fixPlayerY = !cameraBoundaries.bounds.Contains(playerTargetPos) && cameraBoundaries.bounds.ClosestPoint(playerTargetPos).y != playerTargetPos.y;
        }

        //Rotation
        var trgRotPos = new Vector3(fixPlayerX ? ((coopCameraMode != CoopCameraModes.Disabled)?coopTarget.x:target.transform.position.x) : transform.position.x, fixPlayerY ? ((coopCameraMode != CoopCameraModes.Disabled)?coopTarget.y:target.transform.position.y) : transform.position.y, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(trgRotPos - transform.position), maxDegreesDelta);
        // transform.LookAt(trgRotPos);
    }

    void UpdateCameraScroll()
    {
        //Calculate next position
        var targetPosition = new Vector3(target.transform.position.x, target.transform.position.y, offsetPosition.z);
        //Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocityRef, camSmoothTime, scrollingMaxSpeed);
    }

    void StopFollowing()
    {
        target = null;
        camMovementEnabled = false;
        coopCameraMode = CoopCameraModes.Disabled;
    }


    public void setCamBoundaries(Collider2D boundaries)
    {
        cameraMode = SmoothFollow.CameraMode.Follow;
        cameraBoundaries = boundaries;
        if(target && mainTarget) target = mainTarget;
    }

    public void setCamScroll(GameObject nextTarget, float timeToNextTarget)
    {
        cameraMode = SmoothFollow.CameraMode.AutoScroll;
        target = nextTarget;

        scrollingMaxSpeed = (Vector3.Distance(nextTarget.transform.position, transform.position) / timeToNextTarget);
    }

}
