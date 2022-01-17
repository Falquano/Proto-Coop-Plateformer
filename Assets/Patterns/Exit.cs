using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
	[SerializeField] private ExitFlags direction;
	public ExitFlags Direction => direction;

	private bool isExit;

	[SerializeField] private Vector3 spawnPointOffset = Vector3.zero;

	[SerializeField] private SpriteRenderer spriteRenderer;

	private void Start()
	{
		if (spriteRenderer == null)
			spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Seal()
	{
		gameObject.SetActive(true);
	}

	public void MakeExit()
	{
		gameObject.SetActive(true);
		isExit = true;
		spriteRenderer.color = Color.white; // à changer
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!isExit || !collision.gameObject.CompareTag("Player"))
			return;

		Player player = collision.gameObject.GetComponent<Player>();
		player.Win();
	}

	public Vector3 SpawnPoint => transform.position + spawnPointOffset;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(SpawnPoint, .333f);
	}
}
