using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManagerManager : MonoBehaviour
{
	private FMOD.Studio.EventInstance music;
	bool hasAKing = false;
	PlayerManager playerManager;

	private void Start()
	{
		if (playerManager == null)
			playerManager = GameObject.Find("@Player Manager").GetComponent<PlayerManager>();
		music = FMODUnity.RuntimeManager.CreateInstance("event:/updated SD/Music");
		music.start();
	}

    private void Update()
    {
		//music.setParameterByName("kingPresence", laVariableIsCrowned);

	}

	bool UpdateKing()
	{
		foreach(Player p in playerManager.Players)
		{
			if(p.characterController.IsCrowned)
				return true;
		}

		return false;
	}

}
