using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Jan 24 2019
 * 
 * This object will hold values pertaining
 * to receiving treatment. The participant
 * should, at all times, be able to see how
 * much they need to pay to receive treatment.
 */
public class TreatmentStation : MonoBehaviour {

	public GameObject simManager;
    private SimManager simManagerComponent;
	private Treatment treatment;
	private bool treatmentReceived = false;	
	private float elapsed = 0.0f;			// Only do cost and wait calculations every second rather than every frame
	private float displayCost = 0.0f;		// Displayed at all times
	private float displayWait = 0.0f;		// Displayed at all times

    void Start ()
    {
        this.simManagerComponent = simManager.GetComponent<SimManager>();
    }

	/*
	* Allow for payment to be collected if the 
	* player is standing inside the target area
	*/
	private void OnTriggerStay (Collider col) {
		if (treatment != null && col.gameObject.CompareTag("MainCamera")) {
			if (simManagerComponent.currentState() == SimManager.GameState.RUNNING) {
				// TODO - allow them to pay here by doing some action
			}
		}
	}


	/*
	* Sets the new treatment and reverts all 
	* parameters to defaults. Sim manager should
	* call this on every new day.
	*/
	public void setNewTreatment (Treatment tr) {
		this.treatment 		= tr;
		elapsed 			= 0.0f;
		displayCost 		= 0.0f;
		displayWait 		= 0.0f;
		treatmentReceived 	= false;
	}


	/*
	* Runs once per frame
	*/
	void Update () {
		
		// Only calculate / display costs if the user is 
		// standing within the area around the station 
		// We'll do the calculation once per second for
		// efficiency
		if (this.treatment != null) {
			if (simManagerComponent.currentState() == SimManager.GameState.RUNNING) {
				elapsed += Time.deltaTime;
				if (!treatmentReceived && elapsed > 1.0f) {
					float t = simManagerComponent.getElapsedDayTime();
					this.displayCost = this.treatment.currentCost(t);
					this.displayWait = this.treatment.currentWaitTime(t);
					elapsed = 0.0f;
				}
			}
		}

		// Pump out the current values to the overlay
		// TODO
	}
}
