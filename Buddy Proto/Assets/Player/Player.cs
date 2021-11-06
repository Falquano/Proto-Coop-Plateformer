using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	[Header("Objects")]
	[Space]
	private Rigidbody2D body;
	[SerializeField]
	private GameObject helpHighlight;
	[SerializeField]
	private ParticleSystem walkParticle;
	[SerializeField]
	private TrailRenderer jumpTrail;
	private static PlayerManager manager;

	[Header("Values")]
	[Space]
	[SerializeField]
	private float runSpeed = 40f;
	[SerializeField]
	private float jumpStrength = 30f;
	[SerializeField]
	private float helpStrength = 20f;
	[SerializeField]
	private float helpLoss = .02f;
	[SerializeField]
	private float helpCooldown = 1f;
	[SerializeField]
	private float helpRadius = 4f;
	[SerializeField]
	private float preJumpTime = .1f;
	[SerializeField]
	private float coyoteTime = .1f;

	[Header("Ground Checks")]
	[Space]
	[SerializeField]
	private Transform[] GroundCheckPositions;
	[SerializeField]
	private LayerMask groundLayer;
	[SerializeField]
	private LayerMask playerLayer;
	[SerializeField]
	private float groundCheckRadius = .03f;
	
	/// SAUT

	/// <summary>
	/// Indique si le personnage est sur le sol ou sur un autre joueur. 
	/// Mise à jour fixe.
	/// </summary>
	public bool Grounded { private set; get; }
	private bool wasGrounded;
	private bool wasWalking;
	private float timeSinceFall = 1f;
	private float lastJumpInput = 1f;

	/// AIDE

	private bool helpAvailable = true;
	private float helpTime = 10f;

	/// <summary>
	/// Indique si le joueur est sur le sol et se déplace.
	/// </summary>
	public bool IsWalking => (Mathf.Abs(body.velocity.x) > .01f && Grounded);

	private float horizontalMove = 0f;
	private bool jump = false;
	private Vector2 helpDirection;
	private float currentHelpStrength = 0f;

	private PlayerState state;
	/// <summary>
	/// L'état du joueur. Voir <see cref="PlayerState"/>.
	/// </summary>
	public PlayerState State { get => state; set => SetPlayerState(value); }

	// MISES A JOUR

	private void Awake()
	{
		if (manager == null)
			manager = GameObject.Find("@Player Manager").GetComponent<PlayerManager>();

		body = GetComponent<Rigidbody2D>();
		Grounded = true;
		helpHighlight.transform.localScale = new Vector3(helpRadius * 2, helpRadius * 2, 1);
		State = PlayerState.Moving;
		helpHighlight.SetActive(false);

		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.color = Random.ColorHSV(0f, 1f, 0.7f, 0.8f, 0.7f, 0.8f);
	}

    private void Update()
    {
		lastJumpInput += Time.deltaTime;
		helpTime += Time.deltaTime;

		//Debug.Log(IsWalking);
		if (wasWalking && !IsWalking)
		{
			walkParticle.Stop();
		}
		else if (!wasWalking && IsWalking)
		{
			walkParticle.Play();
		}
		wasWalking = IsWalking;
	}

    void FixedUpdate()
	{
		timeSinceFall += Time.fixedDeltaTime;

		wasGrounded = Grounded;
		Grounded = false;
		foreach (Transform transform in GroundCheckPositions)
		{
			if (Physics2D.OverlapCircle(transform.position, groundCheckRadius, groundLayer))
			{
				Grounded = true;
				timeSinceFall = 0;
			} else
			{
				Collider2D[] results = new Collider2D[manager.PlayerCount];
				Physics2D.OverlapCircle(transform.position, groundCheckRadius, new ContactFilter2D() { layerMask = playerLayer }, results);
				if (results.Length <= 0)
					break;

				foreach(Collider2D collider in results)
                {
					if (collider == null)
						continue;

					if (!collider.GetComponent<Player>().Equals(this))
                    {
						Grounded = true;
						break;
                    }
                }
			}
		}

		if (Grounded)
			helpAvailable = true;

		switch (State)
        {
			case PlayerState.Moving:
				UpdateMove();
				break;
			case PlayerState.OfferingHelp:
				UpdateHelp();
				break;
        }

		UpdateTrail();
	}

	/// <summary>
	/// Mise à jour du personnage quand il est en état d'aide
	/// </summary>
	private void UpdateHelp()
    {
		foreach (Player otherPlayer in manager.Players)
		{
			if (otherPlayer.Equals(this))
				continue;

			if (Vector2.Distance(transform.position, otherPlayer.transform.position) <= helpRadius)
			{
				otherPlayer.PullUp();
				State = PlayerState.Moving;
				helpTime = 0f;
				helpAvailable = false;
			}
		}
    }

	/// <summary>
	/// Mise à jour du personnage quand il se déplace
	/// </summary>
	private void UpdateMove()
    {
		Vector2 targetVelocity = new Vector2(
			horizontalMove * Time.fixedDeltaTime * runSpeed,
			body.velocity.y);

		if ((Grounded && jump) // On est sur le sol et on saute
			|| (timeSinceFall <= coyoteTime && jump) // On vient de quitter le sol et on saute (il faudrait vérifier que l'on ne vient pas de sauter !)
			|| (Grounded && lastJumpInput <= preJumpTime)) // On vient d'atterir et on a appuyé sur saut pendant la chute
		{
			targetVelocity.y = jumpStrength;
			Grounded = false;
			Debug.Log($"Coyote : {timeSinceFall}/{coyoteTime},\nPrejump : {lastJumpInput}/{preJumpTime}");
		}

		if (currentHelpStrength >= .01f)
		{
			targetVelocity = helpDirection * currentHelpStrength * helpStrength;
			currentHelpStrength -= helpLoss;
		}

		body.velocity = targetVelocity;

		jump = false;
	}

	private void UpdateTrail()
    {
		if (wasGrounded && !Grounded)
        {
			jumpTrail.Clear();
			//jumpTrail.enabled = true;
			jumpTrail.emitting = true;
		} else if (!wasGrounded && Grounded)
        {
			//jumpTrail.enabled = false;
			jumpTrail.emitting = false;
        }			
    }

	// INPUTS

	public void InputHorizontalMovement(InputAction.CallbackContext context)
	{
		horizontalMove = context.ReadValue<float>() * runSpeed;
	}

	public void InputJump(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			jump = true;
			lastJumpInput = 0;
		}
	}

	public void InputHelp(InputAction.CallbackContext context)
	{
		if (context.performed && helpAvailable && helpTime >= helpCooldown)
		{
			State = PlayerState.OfferingHelp;
		} else if (context.canceled)
        {
			State = PlayerState.Moving;
        }
	}

	// ACTIONS

	/// <summary>
	/// Attire ce joueur vers son compagnon.
	/// TODO : tenter de donner une impulsion verticale uniquement ? Pourrait être plus "intuitif"...
	/// </summary>
	public void HelpMe(Player otherPlayer)
	{
		helpDirection = (otherPlayer.transform.position - transform.position).normalized;
		currentHelpStrength = 1f;
		State = PlayerState.Moving;
		helpAvailable = true;
	}

	/// <summary>
	/// Similaire à <see cref="HelpMe"/> mais uniquement vers le haut, pour l'instant ça parrait mieux, faudrait polir HelpMe.
	/// </summary>
	public void PullUp()
    {
		helpDirection = Vector3.up;
		currentHelpStrength = 1f;
		State = PlayerState.Moving;
		helpAvailable = true;
	}

    private void OnDrawGizmosSelected()
    {
		// Dessine les ground checks
		foreach (Transform t in GroundCheckPositions)
			Gizmos.DrawSphere(t.position, groundCheckRadius);
    }

	/// <summary>
	/// Utiliser directement <see cref="State"/>.
	/// </summary>
	/// <param name="value">Nouvelle valeur d'État</param>
	private void SetPlayerState(PlayerState value)
    {
		if (state == PlayerState.Moving && value == PlayerState.OfferingHelp)
        {
			helpHighlight.SetActive(true);
        } else if (state == PlayerState.OfferingHelp && value == PlayerState.Moving)
        {
			helpHighlight.SetActive(false);
		}


		state = value;
    }
}

/// <summary>
/// États possibles du joueur.
/// </summary>
public enum PlayerState
{
	Moving,
	OfferingHelp
}
