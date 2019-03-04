using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * March 4 2019
 * 
 * Populates participant data static class
 * once the user presses the confirm button
 * on the UI screen, and then loads the 
 * actual simulation scene.
 */
public class PopulateParticipantData : MonoBehaviour {

	public GameObject nameBox;
	public GameObject nausTog;
	public GameObject clausTog;

	private InputField name;
	private Toggle nausea;
	private Toggle claustrophobic;

	void Start () {
		this.name = nameBox.GetComponent<InputField>();
		this.nausea = nausTog.GetComponent<Toggle>();
		this.claustrophobic = clausTog.GetComponent<Toggle>();
	}

	/*
	* Sets values in the static class so that they
	* can be used throughout the simulation for 
	* persistance
	*/
	public void populateStaticData () {
		ParticipantData.name = this.name.text;
		ParticipantData.claustrophicSensitive = this.nausea.isOn;
		ParticipantData.nauseaSensitive = this.claustrophobic.isOn;
	}

	/*
	* Loads a given scene
	*/
	public void LoadNewScene (string sceneName) {
		Application.LoadLevel(sceneName);
	}
}
