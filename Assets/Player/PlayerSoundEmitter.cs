using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundEmitter : MonoBehaviour
{
	public void StepSound()
	{
		FMODUnity.RuntimeManager.PlayOneShot("event:/updated SD/betterWalk");
	}

	public void JumpSound()
	{
		FMODUnity.RuntimeManager.PlayOneShot("event:/updated SD/betterJump");
		Debug.Log("SautSon");
	}

	public void HelpSound()
	{
		FMODUnity.RuntimeManager.PlayOneShot("event:/capacity");
	}

	public void HelpedSound()
	{
		FMODUnity.RuntimeManager.PlayOneShot("event:/capacited");
	}

	public void LandSound()
	{
		FMODUnity.RuntimeManager.PlayOneShot("event:/landing");
	}

	public void WallGrabSound()
	{
		FMODUnity.RuntimeManager.PlayOneShot("event:/wallgrab");
	}
}
