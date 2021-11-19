using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFXEmitter : MonoBehaviour
{

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
}
