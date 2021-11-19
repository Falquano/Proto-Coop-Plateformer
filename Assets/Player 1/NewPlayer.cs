using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayer : MonoBehaviour
{
    //PlayerManager
    private PlayerManager manager;



    // INPUTS


    //Input axis = {xhorizontal, yvertical};
    /// <summary>
    /// Vector2 with the horizontal and vertical axis inputs
    /// </summary>
    public Vector2 InputAxis { get; set; }

    //cb test axis
    public void InputControllerAxis(InputAction.CallbackContext context)
    {
        InputAxis = context.ReadValue<Vector2>();
    }


    /// <summary>
    /// Rigidbody of the player
    /// </summary>
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        JumpingInputUpdate(InputAxis);
        // MoveUpdate();
    }


    [SerializeField] float axisMinJumpDetection;
    private bool isAllowedJumping;
    /// <summary>
    /// Returns weither the controller axis value is detected as a jump or not
    /// </summary>
    /// <param name="ctrlAxis"></param>
    /// <returns></returns>
    bool JumpingInputUpdate(Vector2 ctrlAxis)
    {
        if (ctrlAxis.y >= axisMinJumpDetection)
        {
            return true;
        }
        return false;
    }

    void PositionUpdate()
    {
		
    }




    /// <summary>
    /// Utiliser directement <see cref="State"/>.
    /// </summary>
    /// <param name="value">Nouvelle valeur d'�tat</param>


    public void DeviceLost(PlayerInput playerInput)
    {
        RemovePlayer(playerInput);
    }

    public void RemovePlayer(PlayerInput playerInput)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float h;
        Color.RGBToHSV(spriteRenderer.color, out h, out _, out _);
        manager.AddAvailableHue(h);

        manager.OnPlayerLeft(playerInput);
    }

    // SONS
    public void StepSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/walk");
    }

    public void JumpSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/jump");
    }

    public void HelpSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/capacity");
    }

    public void HelpedSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/capacited");
    }

    public void LandSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/landing");
    }

    public void WallGrabSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/wallgrab");
    }
}

