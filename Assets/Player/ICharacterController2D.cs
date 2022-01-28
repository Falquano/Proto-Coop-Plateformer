using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ICharacterController2D : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D body;

    /// <summary>
    /// Modifié par l'input manager.
    /// </summary>
    public float HorizontalMovement { get; set; }
    /// <summary>
    /// Indique si le joueur est sur le sol et se d�place.
    /// </summary>
    public bool IsWalking => (Mathf.Abs(body.velocity.x) > .01f && IsGrounded);
    /// <summary>
    /// Indique si le personnage est sur le sol ou sur un autre joueur.
    /// </summary>
    public bool IsGrounded { get; protected set; }
    /// <summary>
    /// Indique si le joueur était sur le sol ou sur un autre joueur lors de la dernière frame.
    /// </summary>
    public bool WasGrounded { get; protected set; }
    [SerializeField] protected float coyoteTime = .1f;
    [SerializeField] protected float jumpBufferTime = .1f;

    /// <summary>
    /// Événement invoqué au début d'un saut.
    /// </summary>
    public UnityEvent OnJump { get; private set; }
    /// <summary>
    /// Événement invoqué quand le joueur atteri.
    /// </summary>
    public UnityEvent OnLand { get; private set; }

    /// <summary>
    /// Événement invoqué quand le joueur collisionne avec un mur ou un autre joueur
    /// Paramètre 1 : Force de la collision (float)
    /// Paramètre 2 : Type de collision (<see cref="CollisionType"/>
    /// Paramètre 3 : Collision forcée (si le joueur a été boosté)
    /// </summary>
    public CollisionEvent OnCollision { get; private set; }

    public ICharacterController2D()
    {
        OnJump = new UnityEvent();
        OnLand = new UnityEvent();
        OnCollision = new CollisionEvent();
    }


    // Il faut aussi s'occuper des ground checks


    /// <summary>
	/// Mise à jour du personnage quand il se déplace
    /// TOUJOURS mettre dans un FixedUpdate, pas dans le Update ! 
	/// </summary>
	public abstract void UpdateMove();
    public Vector3 LastGroundedLocation { get; protected set; }
    public abstract void Jump();


    /// <summary>
    /// Indique si le personnage est sur un mur ou non.
    /// </summary>
    public bool IsOnWall { get; protected set; }
    /// <summary>
    /// Indique si le joueur était sur un mur ou non lors de la dernière frame.
    /// </summary>
    public bool WasOnWall { get; protected set; }
}

public enum CollisionType
{
    Wall,
    Player,
    Other
}

[System.Serializable]
public class CollisionEvent : UnityEvent<float, CollisionType, bool>
{
}