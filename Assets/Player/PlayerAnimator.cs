using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator bodyAnimator;
    private ICharacterController2D characterController;
    private Rigidbody2D rigidBody;
    
    void Start()
    {
        if (bodyAnimator == null)
            throw new System.Exception("Il faut un animateur de corps pour le prefab joueur.");
        characterController = GetComponent<ICharacterController2D>();
        rigidBody = GetComponent<Rigidbody2D>();

        characterController.OnJump.AddListener(OnJump);
    }

    
    void Update()
    {
        bodyAnimator.SetFloat("HorizontalSpeed", rigidBody.velocity.x);
        bodyAnimator.SetBool("InAir", !characterController.IsGrounded);
        Debug.Log($"hs : {rigidBody.velocity.x}\nair : {!characterController.IsGrounded}");
    }

    void OnJump()
    {
        bodyAnimator.SetTrigger("Jump");
    }
}
