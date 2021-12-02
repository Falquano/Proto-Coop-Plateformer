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
        Debug.Log(InputAxis);
    }


    /// <summary>
    /// Rigidbody of the player
    /// </summary>
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        PositionFUpdate();
    }

    
    [SerializeField] float movementSpeed;

    /// <summary>
    /// Update the movement
    /// </summary>
    void PositionFUpdate()
    {
        Debug.Log(InputAxis*movementSpeed*Time.fixedDeltaTime);
        rb.AddForce(InputAxis*movementSpeed*Time.fixedDeltaTime,ForceMode2D.Force);
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

