using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManagerManager : MonoBehaviour
{
	private FMOD.Studio.EventInstance music;
	float hasAKing;
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
		hasAKing = UpdateKing() ? 1f : 0f;
		music.setParameterByName("kingPresence", hasAKing);
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
