using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundEmitter : MonoBehaviour
{
	private ICharacterController2D characterController;
	private bool wasCrowned = false;
	private bool IsCrowned => characterController.IsCrowned;

    private void Start()
    {
		characterController = GetComponent<ICharacterController2D>();

		characterController.OnJump.AddListener(JumpSound);
		characterController.OnLand.AddListener(LandSound);
    }

    private void Update()
    {
		if (wasCrowned == false && IsCrowned == true)
			CrownHarvestSound();

		wasCrowned = IsCrowned;
    }

    public void StepSound()
	{
		FMODUnity.RuntimeManager.PlayOneShot("event:/updated SD/betterWalk");
	}

	public void JumpSound()
	{
		FMODUnity.RuntimeManager.PlayOneShot("event:/updated SD/betterJump");
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
		FMODUnity.RuntimeManager.PlayOneShot("event:/updated SD/betterLanding");
	}

	public void WallSlideSound()
	{
		FMODUnity.RuntimeManager.PlayOneShot("event:/updated SD/wallSlide");
		Debug.Log("SCRRRRRRRRR");
	}

	public void CrownHarvestSound()
    {
		FMODUnity.RuntimeManager.PlayOneShot("event:/updated SD/crownHarvest");
		Debug.Log("Choppe ta couronne bg");
	}
}
