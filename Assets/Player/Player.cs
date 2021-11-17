using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	public int ID { get; private set; }
	private static int nextID;

	[Header("Objects")]
	[Space]
	private Rigidbody2D body;
	[SerializeField] private GameObject helpHighlight;
	[SerializeField] private ParticleSystem walkParticle;
	[SerializeField] private TrailRenderer jumpTrail;
	private static PlayerManager manager;
	[SerializeField] private GameObject ImpactParticlePrefab;
	[SerializeField] private HelpOrienter helpOrienter;

	[Header("Values")]
	[Space]
	[SerializeField] private float runSpeed = 40f;
	[SerializeField] private float jumpStrength = 30f;
	[SerializeField] private float helpStrength = 20f;
	[SerializeField] private float helpVerticalPortion = .2f;
	[SerializeField] private float helpLoss = .02f;
	[SerializeField] private float helpCooldown = 1f;
	[SerializeField] private float helpRadius = 4f;
	[SerializeField] private float preJumpTime = .1f;
	[SerializeField] private float coyoteTime = .1f;

	[Header("Ground Checks")]
	[Space]
	[SerializeField] private Transform[] GroundCheckPositions;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask playerLayer;
	[SerializeField] private float groundCheckRadius = .03f;

	[Header("Sound")]
	[Space]
	[SerializeField] private float stepTime = .6f;

	private float stepTimer;

	/// SAUT

	/// <summary>
	/// Indique si le personnage est sur le sol ou sur un autre joueur. 
	/// Mise à jour fixe.
	/// </summary>
	public bool IsGrounded { private set; get; }
	private Vector3 lastGroundedLocation;
	private bool wasGrounded;
	private bool wasWalking;
	private float timeSinceFall = 1f;
	private float lastJumpInput = 1f;

	/// AIDE

	private bool helpAvailable = true;
	private float helpTime = 10f;
	public Vector2 AimDirection { get; private set; }

	/// <summary>
	/// Indique si le joueur est sur le sol et se déplace.
	/// </summary>
	public bool IsWalking => (Mathf.Abs(body.velocity.x) > .01f && IsGrounded);

	private float horizontalMove = 0f;
	private bool jump = false;
	private Vector2 helpDirection;
	private float currentHelpStrength = 0f;

	private PlayerState state;

	private bool doesIgnoreOtherPlayers;
	public bool DoesIgnoreOtherPlayers { get => doesIgnoreOtherPlayers; set => SetDoesIgnoreOtherPlayers(value); }
	/// <summary>
	/// L'état du joueur. Voir <see cref="PlayerState"/>.
	/// </summary>
	public PlayerState State { get => state; set => SetPlayerState(value); }

	// MISES A JOUR

	private void Awake()
	{
		ID = nextID;
		nextID++;

		if (manager == null)
			manager = GameObject.Find("@Player Manager").GetComponent<PlayerManager>();

		transform.position = manager.transform.position;

		body = GetComponent<Rigidbody2D>();
		IsGrounded = true;
		helpHighlight.transform.localScale = new Vector3(helpRadius * 2, helpRadius * 2, 1);
		State = PlayerState.Moving;
		helpHighlight.SetActive(false);

		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.color = Random.ColorHSV(0f, 1f, 0.7f, 0.8f, 0.7f, 0.8f);

		lastGroundedLocation = transform.position;
	}

    private void Update()
	{
		UpdateIsGrounded();

		if (IsGrounded && !wasGrounded)
		{
			//Debug.Log(IsGrounded);
			LandSound();
		}

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

		if (IsWalking)
        {
			stepTimer += Time.deltaTime;
			if (stepTimer >= stepTime)
            {
				stepTimer = 0f;
				StepSound();
            }
        }
	}

	private void UpdateIsGrounded()
    {
		wasGrounded = IsGrounded;
		IsGrounded = false;
		foreach (Transform transform in GroundCheckPositions)
		{
			if (Physics2D.OverlapCircle(transform.position, groundCheckRadius, groundLayer))
			{
				IsGrounded = true;
				timeSinceFall = 0;
			}
			else
			{
				Collider2D[] results = new Collider2D[manager.Count];
				Physics2D.OverlapCircle(transform.position, groundCheckRadius, new ContactFilter2D() { layerMask = playerLayer }, results);
				if (results.Length <= 0)
					break;

				foreach (Collider2D collider in results)
				{
					if (collider == null)
						continue;

					if (!collider.GetComponent<Player>().Equals(this))
					{
						IsGrounded = true;
						break;
					}
				}
			}
		}
		if (IsGrounded)
			lastGroundedLocation = transform.position;
	}

    void FixedUpdate()
	{
		timeSinceFall += Time.fixedDeltaTime;
		UpdateIsGrounded();

		if (IsGrounded)
			helpAvailable = true;

		switch (State)
        {
			case PlayerState.Moving:
				UpdateMove();
				break;
			case PlayerState.OfferingHelp:
				UpdateHelp();
				break;
			case PlayerState.Boost:
				UpdateBoost();
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
				//otherPlayer.PullUp();
				otherPlayer.HelpMe(this);
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

		if ((IsGrounded && jump) // On est sur le sol et on saute
			|| (timeSinceFall <= coyoteTime && jump) // On vient de quitter le sol et on saute (il faudrait vérifier que l'on ne vient pas de sauter !)
			|| (IsGrounded && lastJumpInput <= preJumpTime)) // On vient d'atterir et on a appuyé sur saut pendant la chute
		{
			targetVelocity.y = jumpStrength;
			IsGrounded = false;
			//Debug.Log($"Coyote : {timeSinceFall}/{coyoteTime},\nPrejump : {lastJumpInput}/{preJumpTime}");
			JumpSound();
		}

		body.velocity = targetVelocity;

		jump = false;
	}

	private void UpdateBoost()
	{
		if (currentHelpStrength <= .01f)
		{
			DoesIgnoreOtherPlayers = false;
			State = PlayerState.Moving;
			return;
		}

		Vector2 targetVelocity = helpDirection * currentHelpStrength * helpStrength;
		currentHelpStrength -= helpLoss;

		body.velocity = targetVelocity;
		jump = false;
	}

	private void UpdateTrail()
    {
		if (wasGrounded && !IsGrounded)
        {
			jumpTrail.Clear();
			//jumpTrail.enabled = true;
			jumpTrail.emitting = true;
		} else if (!wasGrounded && IsGrounded)
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
		if (context.performed && State != PlayerState.OfferingHelp)
		{
			jump = true;
			lastJumpInput = 0;
		}
	}

	public void InputHelp(InputAction.CallbackContext context)
	{
		if (context.performed && helpAvailable && helpTime >= helpCooldown)
		{
			helpOrienter.ResetDirection();
			State = PlayerState.OfferingHelp;
			HelpSound();
		} else if (context.canceled)
        {
			State = PlayerState.Moving;
        }
	}

	public void InputRespawn(InputAction.CallbackContext context)
    {
		if (context.performed)
			Respawn();
	}

	public void InputAim(InputAction.CallbackContext context)
	{
		Vector2 aimInput = context.ReadValue<Vector2>();
		if (aimInput.magnitude == 0)
			return;
		
		AimDirection = aimInput.normalized;
	}

	// ACTIONS

	/// <summary>
	/// Similaire à <see cref="PullUp"/>, mais dans la direction vers laquelle vise le joueur aidant.
	/// En plus de cette direction, on ajoute un peu de boost vers le haut !
	/// </summary>
	/// <param name="otherPlayer"></param>
	public void HelpMe(Player otherPlayer)
	{
		helpDirection =
			(Vector3)otherPlayer.AimDirection.normalized * (1f - otherPlayer.helpVerticalPortion) // Visée
			+ otherPlayer.transform.up * otherPlayer.helpVerticalPortion; // Poussée vers le haut
		ActivateBoost();
	}

	/// <summary>
	/// Lance le joueur vers le haut.
	/// </summary>
	public void PullUp()
    {
		helpDirection = Vector3.up;
		ActivateBoost();
	}

	private void ActivateBoost()
	{
		currentHelpStrength = 1f;
		State = PlayerState.Boost;
		helpAvailable = true;
		HelpedSound();
		DoesIgnoreOtherPlayers = true;
	}

	/// <summary>
	/// Renvoie le joueur au dernier endroit "sûr" auquel il était. Si cet endroit n'est plus à l'écran il est téléporté au milieu de l'écran
	/// Un jour faudrait faire ça plus proprement mais on va s'en contenter pour l'instant.
	/// </summary>
	public void Respawn()
    {
		Vector3 spawnWorldLocation = lastGroundedLocation + Vector3.up;
		Vector3 screenSpawnLocation = Camera.main.WorldToScreenPoint(spawnWorldLocation);
		if (screenSpawnLocation.x < 0 || screenSpawnLocation.y < 0
				|| screenSpawnLocation.x > Camera.main.pixelWidth
				|| screenSpawnLocation.y > Camera.main.pixelHeight)
			transform.position = Camera.main.ScreenToWorldPoint(
				new Vector3(Camera.main.pixelWidth / 2, spawnWorldLocation.y, 
							Camera.main.pixelHeight / 2));
		else
			transform.position = spawnWorldLocation;
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

	private void SetDoesIgnoreOtherPlayers(bool value)
    {
		doesIgnoreOtherPlayers = value;
		IgnoreOtherPlayers(value);
    }
	private void IgnoreOtherPlayers(bool ignore = true)
	{
		foreach (Player player in manager.Players)
		{
			Physics2D.IgnoreCollision(this.GetComponent<Collider2D>(), player.GetComponent<Collider2D>(), ignore);
		}
	}

	public void DeviceLost(PlayerInput playerInput)
    {
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
		Instantiate(ImpactParticlePrefab, new Vector3(transform.position.x, transform.position.y, 3), Quaternion.identity);
		FMODUnity.RuntimeManager.PlayOneShot("event:/landing");
	}

	public void WallGrabSound()
    {
		FMODUnity.RuntimeManager.PlayOneShot("event:/wallgrab");
	}
}

/// <summary>
/// États possibles du joueur.
/// </summary>
public enum PlayerState
{
	Moving,
	OfferingHelp,
	Boost
}
