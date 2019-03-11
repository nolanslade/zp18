using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
* McDSL: VR Simulation One
* Nolan Slade
* Feb 27 2018
*
* Adding sound effects to the scene - call the playSound method
* with the corresponding enum to add effects to the scene.
*/
public class AudioManager : MonoBehaviour {

	public AudioClip 	waterFlowClip, takeMedicineClip, startDayClip, dayCompleteClip,
						simCompleteClip, countNormalClip, countCriticalClip;

	private AudioSource source;
    private bool muted  = false;

	public enum SoundType 
	{
		WATER_FLOW,
		TAKE_MEDICINE,
		START_DAY,
		SIM_COMPLETE,
		NORMAL_TICK,
		CRITICAL_TICK,
		DAY_COMPLETE
	}

	void Start () {
		this.source = GetComponents<AudioSource>()[0];
	}

	public void mute () {
		this.muted = true;
	}

	public void unmute () {
		this.muted = false;
	}

	/*
	* Plays any supported sound effect
	*/
	public void playSound (SoundType s) {

		if (!muted) {
			switch (s) {
				case SoundType.WATER_FLOW: 		source.clip = waterFlowClip; 		source.Play();
					break;
				case SoundType.TAKE_MEDICINE: 	source.clip = takeMedicineClip; 	source.Play();
					break;
				case SoundType.START_DAY: 		source.clip = startDayClip; 		source.Play();
					break;
				case SoundType.SIM_COMPLETE: 	source.clip = simCompleteClip; 		source.Play();
					break;
				case SoundType.DAY_COMPLETE: 	source.clip = dayCompleteClip; 		source.Play();
					break;
				case SoundType.NORMAL_TICK: 	source.clip = countNormalClip; 		source.Play();
					break;
				case SoundType.CRITICAL_TICK: 	source.clip = countCriticalClip; 	source.Play();
					break;
				default:
					Debug.Log("Invalid sound type");
					break;
			}
		}
	}


	public void stopSound () {	
		source.Stop();
	}
}
