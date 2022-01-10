using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pattern : MonoBehaviour
{
    [SerializeField] private Tilemap map;
	[SerializeField] private ExitFlags exitDirections;
	public ExitFlags ExitDirections => exitDirections;
	
	[SerializeField] private Exit[] exits;

	public bool CanComeFrom(ExitFlags entrance)
	{
		return exitDirections.HasFlag(ExitFlag.Opposite(entrance));
	}

	public void NewRound(ExitFlags exit)
	{
		foreach(Exit e in exits)
		{
			if (e.Direction.HasFlag(exit))
				e.MakeExit();
			else
				e.Seal();
		}
	}

	
}

[Flags]
public enum ExitFlags
{
	None = 0,
	Up = 1,
	Down = 2,
	Left = 4,
	Right = 8
}

public static class ExitFlag
{
	public static ExitFlags Opposite(ExitFlags exit)
	{
		return exit switch
		{
			ExitFlags.Up => ExitFlags.Down,
			ExitFlags.Down => ExitFlags.Up,
			ExitFlags.Left => ExitFlags.Right,
			ExitFlags.Right => ExitFlags.Left,
			_ => ExitFlags.None,
		};
	}

	public static ExitFlags RandomExitDirection()
	{
		int r = UnityEngine.Random.Range(0, 4);
		return r switch
		{
			0 => ExitFlags.Right,
			1 => ExitFlags.Down,
			2 => ExitFlags.Left,
			3 => ExitFlags.Up,
			_ => ExitFlags.None
		};
	}
}