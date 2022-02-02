using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManagerManager : MonoBehaviour
{
	private FMOD.Studio.EventInstance music;
	

	private void Start()
	{
		music = FMODUnity.RuntimeManager.CreateInstance("event:/updated SD/Music");
		music.start();
	}

    private void Update()
    {
		//music.setParameterByName("kingPresence", laVariableIsCrowned);

	}

}
