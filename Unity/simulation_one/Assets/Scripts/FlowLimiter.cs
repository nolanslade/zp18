using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowLimiter : MonoBehaviour {

	public GameObject flowManager;
	public GameObject simManager;

	private void OnTriggerEnter (Collider col) {
		if (simManager.GetComponent<SimManager>().currentState() == SimManager.GameState.RUNNING) {
			flowManager.GetComponent<FlowManager>().startFlow();
		}
	}

	private void OnTriggerExit (Collider col) {
		flowManager.GetComponent<FlowManager>().stopFlow();
	}
}
