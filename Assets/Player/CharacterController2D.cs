using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : ICharacterController2D
{
	private Player player;

	[Header("Values")]
	[Space]
	[SerializeField] private float runSpeed = 40f;
	[SerializeField] private float jumpStrength = 30f;

	[Header("Ground Checks")]
	[Space]
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask playerLayer;
	[SerializeField] private float groundCheckRadius = .03f;

	private bool wasWalking;
	private float timeSinceFall = 1f;
	private float lastJumpInput = 1f;
	private bool jump = false;

	public CharacterController2D() : base()
    {

    }

	private void Start()
	{
		player = GetComponent<Player>();
	}

	private void Awake()
	{
		IsGrounded = true;

		LastGroundedLocation = transform.position;
	}

	private void Update()
	{
		lastJumpInput += Time.deltaTime;
		timeSinceFall += Time.deltaTime;

		//Debug.Log(IsWalking);
		if (wasWalking && !IsWalking)
		{
			player.FX.StopWalkParticle();
		}
		else if (!wasWalking && IsWalking)
		{
			player.FX.StartWalkParticle();
		}
		wasWalking = IsWalking;
	}

	public override void Jump()
	{
		jump = true;
		lastJumpInput = 0;
		OnJump.Invoke();
	}

	public void UpdateIsGrounded()
	{
		WasGrounded = IsGrounded;
		IsGrounded = false;
		if (Physics2D.OverlapBox(transform.position, new Vector2(.90f, 1f), 0, groundLayer))
		{
			IsGrounded = true;
			timeSinceFall = 0;
		}
		else
		{
			Collider2D[] results = new Collider2D[Player.Manager.Count];
			Physics2D.OverlapBox(transform.position, new Vector2(.90f, 1f), 0, new ContactFilter2D() { layerMask = playerLayer }, results);
			foreach (Collider2D collider in results)
			{
				if (collider == null)
					continue;

				if (!collider.GetComponent<Player>().Equals(player))
				{
					IsGrounded = true;
					break;
				}
			}
		}

		if (IsGrounded)
			LastGroundedLocation = transform.position;
	}

	/// <summary>
	/// Mise à jour du personnage quand il se d�place
	/// </summary>
	public override void UpdateMove()
	{
		UpdateIsGrounded();

		Vector2 targetVelocity = new Vector2(
			HorizontalMovement * Time.fixedDeltaTime * runSpeed,
			body.velocity.y);
		

		if ((IsGrounded && jump) // On est sur le sol et on saute
			|| (timeSinceFall <= coyoteTime && jump) // On vient de quitter le sol et on saute (il faudrait v�rifier que l'on ne vient pas de sauter !)
			|| (IsGrounded && lastJumpInput <= jumpBufferTime)) // On vient d'atterir et on a appuy� sur saut pendant la chute
		{
			targetVelocity.y = jumpStrength;
			IsGrounded = false;
			//Debug.Log($"Coyote : {timeSinceFall}/{coyoteTime},\nPrejump : {lastJumpInput}/{preJumpTime}");
			player.Sound.JumpSound();
		}

		body.velocity = targetVelocity;
		//Debug.Log(string.Format("target : {0}\nvelocity : {1}", targetVelocity, body.velocity));

		jump = false;

		if (IsGrounded && !WasGrounded)
		{
			player.FX.InstantiateImpactParticle();
			player.Sound.LandSound();
		}
	}

	private void OnDrawGizmosSelected()
	{
		// Dessine les ground checks
		/*foreach (Transform t in GroundCheckPositions)
			Gizmos.DrawSphere(t.position, groundCheckRadius);*/
	}
}
