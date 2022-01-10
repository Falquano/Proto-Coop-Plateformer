using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
	[SerializeField] private ExitFlags direction;
	public ExitFlags Direction => direction;

	private bool isExit;

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
		spriteRenderer.color = Color.white; // � changer
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!isExit || !collision.gameObject.CompareTag("Player"))
			return;

		Player player = collision.gameObject.GetComponent<Player>();
		player.Win();
	}
}
