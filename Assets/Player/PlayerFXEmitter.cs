using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFXEmitter : MonoBehaviour
{
	[SerializeField] Player player;
	[SerializeField] BetterCharacterController2D characterController2D;
	[SerializeField] private GameObject helpHighlight;
	[SerializeField] private ParticleSystem walkParticle;
	
	[SerializeField] private TrailRenderer jumpTrail;
	[SerializeField] private GameObject ImpactParticlePrefab;

	public void UpdateHelpScale(float radius)
	{
		helpHighlight.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
	}

	public void SetHelpActive(bool active)
	{
		helpHighlight.SetActive(active);
	}

	public void InstantiateImpactParticle()
	{
		Instantiate(ImpactParticlePrefab, new Vector3(transform.position.x, transform.position.y, 3), Quaternion.identity);
	}

	public void StartWalkParticle()
	{
		walkParticle.Play();
	}

	public void StopWalkParticle()
	{
		walkParticle.Stop();
	}

	public void ResetJumpTrail()
	{
		jumpTrail.Clear();
		jumpTrail.emitting = true;
	}

	public void StopJumpTrail()
	{
		jumpTrail.emitting = false;
	}

	public void SetHelpColor(Color color)
    {
		helpHighlight.GetComponent<SpriteRenderer>().color = color;
    }

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Start()
	{
		player = GetComponent<Player>();
		characterController2D = GetComponent<BetterCharacterController2D>();
	}
	private bool wasWalking;
	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
		if (wasWalking && !characterController2D.IsWalking)
		{
			player.FX.StopWalkParticle();
		}
		else if (!wasWalking && characterController2D.IsWalking)
		{
			player.FX.StartWalkParticle();
		}
		wasWalking = characterController2D.IsWalking;
	}
}
