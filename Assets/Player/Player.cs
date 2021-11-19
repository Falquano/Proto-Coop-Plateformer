using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	public int ID { get; private set; }
	private static int nextID;

	[Header("Objects")]
	[Space]
	private Rigidbody2D body;
	private static PlayerManager manager;
	[SerializeField] private PlayerSoundEmitter Sound;
	[SerializeField] private PlayerFXEmitter FX;
	[SerializeField] private PlayerHelp Help;

	[Header("Values")]
	[Space]
	[SerializeField] private float runSpeed = 40f;
	[SerializeField] private float jumpStrength = 30f;
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
	/// Mise � jour fixe.
	/// </summary>
	public bool IsGrounded { private set; get; }
	private Vector3 lastGroundedLocation;
	private bool wasGrounded;
	private bool wasWalking;
	private float timeSinceFall = 1f;
	private float lastJumpInput = 1f;


	/// <summary>
	/// Indique si le joueur est sur le sol et se d�place.
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
	/// L'�tat du joueur. Voir <see cref="PlayerState"/>.
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
		FX.SetHelpActive(false);
		State = PlayerState.Moving;

		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		//spriteRenderer.color = Random.ColorHSV(0f, 1f, 0.7f, 0.8f, 0.7f, 0.8f); // Ancienne m�thode d'assignation de couleur
		spriteRenderer.color = Color.HSVToRGB(manager.RequestHue(), .75f, .75f); // Nouvelle m�thode, couleur unique !

		lastGroundedLocation = transform.position;
	}

    private void Update()
	{
		UpdateIsGrounded();

		if (IsGrounded && !wasGrounded)
		{
			FX.InstantiateImpactParticle();
			Sound.LandSound();
		}

		lastJumpInput += Time.deltaTime;

		//Debug.Log(IsWalking);
		if (wasWalking && !IsWalking)
		{
			FX.StopWalkParticle();
		}
		else if (!wasWalking && IsWalking)
		{
			FX.StartWalkParticle();
		}
		wasWalking = IsWalking;

		if (IsWalking)
        {
			stepTimer += Time.deltaTime;
			if (stepTimer >= stepTime)
            {
				stepTimer = 0f;
				Sound.StepSound();
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
			Help.SetAvailable();

		switch (State)
        {
			case PlayerState.Moving:
				UpdateMove();
				break;
			case PlayerState.OfferingHelp:
				Help.UpdateHelp(manager);
				break;
			case PlayerState.Boost:
				UpdateBoost();
				break;
        }

		UpdateTrail();
	}

	

	/// <summary>
	/// Mise � jour du personnage quand il se d�place
	/// </summary>
	private void UpdateMove()
    {
		Vector2 targetVelocity = new Vector2(
			horizontalMove * Time.fixedDeltaTime * runSpeed,
			body.velocity.y);

		if ((IsGrounded && jump) // On est sur le sol et on saute
			|| (timeSinceFall <= coyoteTime && jump) // On vient de quitter le sol et on saute (il faudrait v�rifier que l'on ne vient pas de sauter !)
			|| (IsGrounded && lastJumpInput <= preJumpTime)) // On vient d'atterir et on a appuy� sur saut pendant la chute
		{
			targetVelocity.y = jumpStrength;
			IsGrounded = false;
			//Debug.Log($"Coyote : {timeSinceFall}/{coyoteTime},\nPrejump : {lastJumpInput}/{preJumpTime}");
			Sound.JumpSound();
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

		Vector2 targetVelocity = helpDirection * currentHelpStrength * Help.Strength;
		currentHelpStrength -= Help.Loss;

		body.velocity = targetVelocity;
		jump = false;
	}

	private void UpdateTrail()
    {
		if (wasGrounded && !IsGrounded)
        {
			FX.ResetJumpTrail();
		} else if (!wasGrounded && IsGrounded)
        {
			FX.StopJumpTrail();
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
		if (context.performed && Help.CanHelp)
		{
			State = PlayerState.OfferingHelp;
			Sound.HelpSound();
		} else if (context.canceled)
        {
			State = PlayerState.Moving;
        }
	}

	public void InputPush(InputAction.CallbackContext context)
	{
		if (context.performed && Help.CanHelp)
		{
			Help.HelpMod = 1f;
			State = PlayerState.OfferingHelp;
			Sound.HelpSound();
		}
		else if (context.canceled)
		{
			State = PlayerState.Moving;
		}
	}

	public void InputPull(InputAction.CallbackContext context)
	{
		if (context.performed && Help.CanHelp)
		{
			Help.HelpMod = -1f;
			State = PlayerState.OfferingHelp;
			Sound.HelpSound();
		}
		else if (context.canceled)
		{
			State = PlayerState.Moving;
		}
	}

	public void InputRespawn(InputAction.CallbackContext context)
    {
		if (context.performed)
		{
			Debug.LogWarning("RELOADING");
			SceneManager.LoadScene("SampleScene");
			//Respawn();
		}
	}

	// ACTIONS

	public void PullMe(Player otherPlayer)
	{
		Debug.Log("ZPI");
		helpDirection = (otherPlayer.transform.position - transform.position).normalized;
		ActivateBoost();
	}

	public void PushMe(Player otherPlayer)
	{
		helpDirection = (transform.position - otherPlayer.transform.position).normalized;
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
		Help.SetAvailable();
		Sound.HelpedSound();
		DoesIgnoreOtherPlayers = true;
	}

	/// <summary>
	/// Renvoie le joueur au dernier endroit "s�r" auquel il �tait. Si cet endroit n'est plus � l'�cran il est t�l�port� au milieu de l'�cran
	/// Un jour faudrait faire �a plus proprement mais on va s'en contenter pour l'instant.
	/// </summary>
	public void Respawn()
    {
		Vector3 spawnWorldLocation = lastGroundedLocation + Vector3.up;
		Vector3 screenSpawnLocation = Camera.main.WorldToScreenPoint(spawnWorldLocation);
		if (screenSpawnLocation.x < 0 || screenSpawnLocation.y < 0
				|| screenSpawnLocation.x > Camera.main.pixelWidth
				|| screenSpawnLocation.y > Camera.main.pixelHeight)
		{
			Vector3 newpos = Camera.main.ScreenToWorldPoint(
				new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));
			newpos.z = 0;
			transform.position = newpos;
		}
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
	/// <param name="value">Nouvelle valeur d'�tat</param>
	private void SetPlayerState(PlayerState value)
    {
		if (state == PlayerState.Moving && value == PlayerState.OfferingHelp)
        {
			FX.SetHelpActive(true);
        } else if (state == PlayerState.OfferingHelp && value == PlayerState.Moving)
        {
			FX.SetHelpActive(false);
		}

		state = value;
    }

	private void SetDoesIgnoreOtherPlayers(bool value)
    {
		Debug.Log(value);
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
}

/// <summary>
/// �tats possibles du joueur.
/// </summary>
public enum PlayerState
{
	Moving,
	OfferingHelp,
	Boost
}
