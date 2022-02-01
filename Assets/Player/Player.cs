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
	public static PlayerManager Manager { get; private set; }
	public PlayerSoundEmitter Sound => sound;
	[SerializeField] private PlayerSoundEmitter sound;
	public PlayerFXEmitter FX => fx;
	[SerializeField] private PlayerFXEmitter fx;
	[SerializeField] private PlayerHelp help;
	[SerializeField] private GameObject model;
	[SerializeField] private ICharacterController2D characterController;
	[SerializeField] private PlayerAnimator animator;
	[SerializeField] private GameObject crown;

	[Header("Sound")]
	[Space]
	[SerializeField] private float stepTime = .6f;

	private float stepTimer;

	private Vector2 helpDirection;
	private float currentHelpStrength = 0f;

	private PlayerState state;

	private bool doesIgnoreOtherPlayers;
	public bool DoesIgnoreOtherPlayers { get => doesIgnoreOtherPlayers; set => SetDoesIgnoreOtherPlayers(value); }
	/// <summary>
	/// L'État du joueur. Voir <see cref="PlayerState"/>.
	/// </summary>
	public PlayerState State { get => state; set => SetPlayerState(value); }
	private float ejectionStrength;

	// MISES A JOUR

	private void Awake()
	{
		ID = nextID;
		nextID++;

		if (Manager == null)
			Manager = GameObject.Find("@Player Manager").GetComponent<PlayerManager>();

		transform.position = Manager.transform.position;

		body = GetComponent<Rigidbody2D>();
		fx.SetHelpActive(false, help.HelpMod);
		State = PlayerState.Moving;

		SpriteRenderer spriteRenderer = model.GetComponent<SpriteRenderer>();
		Color color = Manager.RequestColor(); // Nouvelle méthode, couleur unique !
		spriteRenderer.color = color;
		color.a = .2f;
		fx.SetHelpColor(color);

		SetCrown(false);

		characterController.OnLand.AddListener(OnLand);
	}

    void FixedUpdate()
	{
		if (characterController.IsGrounded)
			help.SetAvailable();

		switch (State)
        {
			case PlayerState.Moving:
				characterController.UpdateMove();
				break;
			case PlayerState.OfferingHelp:
				help.UpdateHelp(Manager);
				break;
			case PlayerState.Boost:
				UpdateBoost();
				break;
        }

		UpdateTrail();
	}

    private void Update()
    {
		if (characterController.IsWalking)
		{
			stepTimer += Time.deltaTime;
			if (stepTimer >= stepTime)
			{
				stepTimer = 0f;
				Sound.StepSound();
			}
		}
	}

    private void UpdateBoost()
	{
		if (currentHelpStrength <= .01f)
		{
			DoesIgnoreOtherPlayers = false;
			State = PlayerState.Moving;
			return;
		}

		Vector2 targetVelocity = helpDirection * currentHelpStrength * ejectionStrength;
		currentHelpStrength -= help.Loss;

		body.velocity = targetVelocity;
		//characterController.CanJump = false;
	}

	private void UpdateTrail()
    {
		if (characterController.WasGrounded && !characterController.IsGrounded)
        {
			fx.ResetJumpTrail();
		} else if (!characterController.WasGrounded && characterController.IsGrounded)
        {
			fx.StopJumpTrail();
        }			
    }

    // INPUTS

    public void InputHorizontalMovement(InputAction.CallbackContext context)
	{
		characterController.HorizontalMovement = context.ReadValue<float>();
	}

	public void InputJump(InputAction.CallbackContext context)
	{
		if (context.performed && State != PlayerState.OfferingHelp)
		{
			characterController.Jump();
		}
	}

	public void InputHelp(InputAction.CallbackContext context)
	{
		if (context.performed && help.CanHelp && State != PlayerState.Stun)
		{
			State = PlayerState.OfferingHelp;
			sound.HelpSound();
		} else if (context.canceled)
        {
			State = PlayerState.Moving;
        }
	}

	public void InputPush(InputAction.CallbackContext context)
	{
		if (context.performed && help.CanHelp && State != PlayerState.Stun)
		{
			help.HelpMod = 1f;
			State = PlayerState.OfferingHelp;
			sound.HelpSound();
		}
		else if (context.canceled)
		{
			State = PlayerState.Moving;
		}
	}

	public void InputPull(InputAction.CallbackContext context)
	{
		if (context.performed && help.CanHelp)
		{
			help.HelpMod = -1f;
			State = PlayerState.OfferingHelp;
			sound.HelpSound();
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
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			//Respawn();
		}
	}

	public void InputFastFall(InputAction.CallbackContext context)
	{
		if(context.performed)
		{
			Debug.Log("FAST FALL");
			characterController.FastFall();
		}
	}

	// ACTIONS

	public void PullMe(Player otherPlayer, float strength)
	{
		helpDirection = (otherPlayer.transform.position - transform.position).normalized;
		ActivateBoost(strength);
	}

	public void PushMe(Player otherPlayer, float strength)
	{
		helpDirection = (transform.position - otherPlayer.transform.position).normalized;
		ActivateBoost(strength);
	}

	/// <summary>
	/// Lance le joueur vers le haut.
	/// </summary>
	// public void PullUp()
    // {
	// 	helpDirection = Vector3.up;
	// 	ActivateBoost();
	// }

	private void ActivateBoost(float strength)
	{
		ejectionStrength = strength;
		currentHelpStrength = 1;
		State = PlayerState.Boost;
		help.SetAvailable();
		sound.HelpedSound();
		DoesIgnoreOtherPlayers = true;
	}

	/// <summary>
	/// Renvoie le joueur au dernier endroit "sûr" auquel il était. Si cet endroit n'est plus à l'écran il est téléporté au milieu de l'écran
	/// Un jour faudrait faire ça plus proprement mais on va s'en contenter pour l'instant.
	/// </summary>
	public void Respawn()
    {
		Vector3 spawnWorldLocation = characterController.LastGroundedLocation + Vector3.up;
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


	/// <summary>
	/// Utiliser directement <see cref="State"/>.
	/// </summary>
	/// <param name="value">Nouvelle valeur d'état</param>
	private void SetPlayerState(PlayerState value)
    {
		if (state == PlayerState.Moving && value == PlayerState.OfferingHelp)
        {
			fx.SetHelpActive(true, help.HelpMod);
        } else if (state == PlayerState.OfferingHelp && value == PlayerState.Moving)
        {
			fx.SetHelpActive(false, help.HelpMod);
		}

		if (state == PlayerState.Boost && value != state)
			DoesIgnoreOtherPlayers = false;

		state = value;
    }

	private void SetDoesIgnoreOtherPlayers(bool value)
    {
		doesIgnoreOtherPlayers = value;
		IgnoreOtherPlayers(value);
    }

	private void IgnoreOtherPlayers(bool ignore = true)
	{
		foreach (Player player in Manager.Players)
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
		Manager.AddAvailableHue(spriteRenderer.color);

		Manager.OnPlayerLeft(playerInput);
	}

	public void Win()
	{
		Manager.Win(this);
	}

	public void Hybernate()
	{
		transform.position = new Vector3(10000, 10000, 10000);
		body.bodyType = RigidbodyType2D.Static;
		State = PlayerState.Sleeping;	
	}

	public void WakeUp(Vector3 spawnpoint)
	{
		transform.position = spawnpoint;
		body.bodyType = RigidbodyType2D.Dynamic;
		State = PlayerState.Moving;
	}

	public void SetCrown(bool value)
    {
		crown.SetActive(value);
    }

	// EVENTS
	public void OnLand()
	{
		//Debug.Log("LAND");
		FX.InstantiateImpactParticle();
		Sound.LandSound();
	}

	// public void OnStrongImpact()
	// {
	// 	FX.InstantiateImpactParticle();
	// 	Sound.LandSound();
	// }

	// public void OnWallGrabEnter()
	// {

	// }

	// public void OnWallGrabExit()
	// {

	// }

	// public void OnWallGrabExitJump()
	// {

	// }

	// public void OnWallGrabStay()
	// {

	// }

}

/// <summary>
/// États possibles du joueur.
/// </summary>
public enum PlayerState
{
	Moving,
	OfferingHelp,
	Boost,
	Sleeping,
	Stun
}
