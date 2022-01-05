using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private Animator faceAnimator;
    [SerializeField] private LittleEventInvoker OnLandFinished;
    private ICharacterController2D characterController;
    private Rigidbody2D rigidBody;

    public int Face;
    [SerializeField] private int faceCount = 6;

    void Start()
    {
        Face = Random.Range(0, faceCount);

        if (bodyAnimator == null)
            throw new System.Exception("Il faut un animateur de corps pour le prefab joueur.");
        if (faceAnimator == null)
            throw new System.Exception("Il faut un animateur de visage pour le prefab joueur.");
        characterController = GetComponent<ICharacterController2D>();
        rigidBody = GetComponent<Rigidbody2D>();

        characterController.OnJump.AddListener(OnJump);
        OnLandFinished.Event.AddListener(FinishedLanding);
    }

    
    void Update()
    {
        bodyAnimator.SetFloat("HorizontalSpeed", rigidBody.velocity.x);
        bodyAnimator.SetBool("InAir", !characterController.IsGrounded);

        faceAnimator.SetFloat("HorizontalSpeed", Mathf.Abs(rigidBody.velocity.x));
        faceAnimator.SetBool("Grounded", characterController.IsGrounded);

        if (rigidBody.velocity.x > 0.05f) // Flip visage
        {
            faceAnimator.transform.localScale = new Vector3(-1, 1, 1);
        } else
        {
            faceAnimator.transform.localScale = Vector3.one;
        }
    }

    void OnJump()
    {
        bodyAnimator.SetTrigger("Jump");
        faceAnimator.SetBool("FinishedLanding", false);
    }

    void FinishedLanding()
    {
        faceAnimator.SetBool("FinishedLanding", true);
    }
}
