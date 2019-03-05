using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Jan 19 2019
 * 
 * An attempt to prevent people from cheating in some way,
 * i.e. throwing water from the bucket to the other side of 
 * the room, etc. The participant will need to be standing
 * within the target area in order to receive payment.
 */
public class DestinationLimiter : MonoBehaviour {

    public GameObject simManager;
    public bool enabled;
    private SimManager simScriptComp;

    void Start() {
        
        this.simScriptComp = simManager.GetComponent<SimManager>();

        if (!enabled) {
            simScriptComp.togglePayment(true);
        }
    }

    /*
    * If the headset enters the target area, enable scoring
    */
    private void OnTriggerEnter(Collider col) {
        
        // This should only trigger if the colliding object is the headset
        if (enabled && col.gameObject.CompareTag("physicalCamera")) {
            Debug.Log("Enabling payment - participant in destination area.");
            if (simScriptComp.currentState() == SimManager.GameState.RUNNING)
            {
                simScriptComp.togglePayment(true);
            }
        }
    }


    /*
    * If the headset exits the target area, disable scoring
    */
    private void OnTriggerExit(Collider col) {

        // This should only trigger if the colliding object is the headset
        if (enabled) {
            Debug.Log("Disabling payment - participant left destination area.");
            if (col.gameObject.CompareTag("physicalCamera")) {
                simScriptComp.togglePayment(false);
            }
        }
    }
}
