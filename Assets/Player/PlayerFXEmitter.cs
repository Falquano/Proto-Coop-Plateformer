using Cinemachine;
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
	[SerializeField] private ParticleSystem PushParticle;
	[SerializeField] private ParticleSystem PullParticle;

	[SerializeField] private bool onCollisionParticles = true;

	public void UpdateHelpScale(float radius)
	{
		helpHighlight.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
	}

	public void SetHelpActive(bool active, float helpMod)
	{
		helpHighlight.SetActive(active);
		
		if (!active)
        {
			//PullParticlePrefab.SetActive(false);
			PullParticle.Stop();
			//PushParticlePrefab.SetActive(false);
			PushParticle.Stop();
		} else if (helpMod >= 0)
        {
			//PushParticlePrefab.SetActive(true);
			PushParticle.gameObject.SetActive(true);
			PushParticle.Play();
        } else if (helpMod <= 0)
        {
			//PullParticlePrefab.SetActive(true);
			PullParticle.gameObject.SetActive(true);
			PullParticle.Play();
        }
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

	public void InstantiatePushParticle()
    {
		Instantiate(PushParticle, transform.position, Quaternion.identity);
	}

	public void InstantiatePullParticle()
	{
		Instantiate(PullParticle, transform.position, Quaternion.identity);
	}

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Start()
	{
		player = GetComponent<Player>();
		characterController2D = GetComponent<BetterCharacterController2D>();

		characterController2D.OnCollision.AddListener(OnCollision);

		ResetJumpTrail();
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

	void OnCollision(float force, CollisionType type, bool forced)
    {
		if (!onCollisionParticles)
			return;

		if (force <= .05f)
			return;

		float forcedMod = forced ? 2.5f : 1f;

		// Envoyer des particules ici, mettre un nombre de particules en fonction de la force
		ParticleSystem particleSystem = 
			Instantiate(ImpactParticlePrefab, new Vector3(transform.position.x, transform.position.y, 3), Quaternion.identity)
			.GetComponent<ParticleSystem>();
		particleSystem.Emit((int) (force * 3f * forcedMod));
	}
}
