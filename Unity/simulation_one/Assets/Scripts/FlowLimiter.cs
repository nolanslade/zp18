using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Jan 19 2019
 * 
 * Limits the tap to only flowing
 * if the participant is within the target area - 
 * less object spawning -> potentially more performance
 */
public class FlowLimiter : MonoBehaviour {

	public GameObject flowManager;
	public GameObject simManager;
    private FlowManager flowManComponent;
    private SimManager simManComponent;
    public bool flowLimitEnabled;

    void Start ()
    {
        if (!flowLimitEnabled)
            flowManager.GetComponent<FlowManager>().startFlow();

        this.simManComponent = simManager.GetComponent<SimManager>();
        this.flowManComponent = flowManager.GetComponent<FlowManager>();
    }

    /*
    * If the headset enters the target area, turn the tap on
    */
	private void OnTriggerEnter (Collider col) {
        if (flowLimitEnabled) {
            // This should only trigger if the colliding object is the headset
            if (col.gameObject.CompareTag("MainCamera")) {
                if (simManComponent.currentState() == SimManager.GameState.RUNNING) {
                    flowManComponent.startFlow();
                }
            }
        }
	}


    /*
    * If the headset exits the target area, turn the tap off
    */
	private void OnTriggerExit (Collider col) {
        if (flowLimitEnabled) {
            // This should only trigger if the colliding object is the headset
            if (col.gameObject.CompareTag("MainCamera")) {
                flowManComponent.stopFlow();
            }
        }
	}
}
