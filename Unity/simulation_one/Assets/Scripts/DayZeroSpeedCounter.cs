using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Nolan Slade
 * April 8 2019
 * 
 * Trigger zone used to measure how fast a participant 
 * is moving during the carrying phase of the tutorial 
 * day.
 */
public class DayZeroSpeedCounter : MonoBehaviour {

    public GameObject simManager;
    private SimManager simManComp;

    void Start () {
        this.simManComp = simManager.GetComponent<SimManager>();
    }

	void OnTriggerEnter (Collider col) {
        if (col.gameObject.CompareTag("MainCamera")) {
            Debug.Log("Entered Day 0 Zone");
            this.simManComp.inDay0SpeedCaptureZone = true;
            this.simManComp.dayZeroMovingCount = 0;
        }
    }

    void OnTriggerExit (Collider col) {
        if (col.gameObject.CompareTag("MainCamera")) {
            Debug.Log("Left Day 0 Zone");
            this.simManComp.inDay0SpeedCaptureZone = false;
        }
    }
}
