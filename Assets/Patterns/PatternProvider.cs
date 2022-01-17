using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternProvider : MonoBehaviour
{
	public List<GameObject> Patterns { get; private set; }

	private void Start()
	{
		Patterns = new List<GameObject>();
		Patterns.AddRange(Resources.LoadAll<GameObject>("LevelPatterns"));
	}

	public GameObject Find (ExitFlags entrance)
	{
		return Patterns[0];
	}
}
