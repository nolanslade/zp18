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
        } else {
            simScriptComp.togglePayment(false);
        }
    }

    /*
    * If the headset enters the target area, enable scoring
    */
    void OnTriggerEnter(Collider col) {

        Debug.Log("Dest limiter trigger");
        
        // This should only trigger if the colliding object is the headset
        if (enabled  && col.gameObject.CompareTag("MainCamera")) {
            if (simScriptComp.currentState() == SimManager.GameState.RUNNING) {
                Debug.Log("Enabling payment - participant entered destination area.");
                simScriptComp.togglePayment(true);
            }
        }
    }


    /*
    * If the headset exits the target area, disable scoring
    */
    void OnTriggerExit(Collider col) {

        // This should only trigger if the colliding object is the headset
        if (enabled && col.gameObject.CompareTag("MainCamera")) {
            Debug.Log("Disabling payment - participant left destination area.");
            simScriptComp.togglePayment(false);
        }
    }
}
