using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Feb 14 2018
 * 
 * When the counter's trigger is entered / exited by a water droplet
 * we need to decrease and increase the payload amount. Essentially
 * tracks how much the user is carrying at all times. Can also be used
 * to remove the actual water droplets (for example, impairment)
 */
public class WaterDropletCounter : MonoBehaviour {

    public GameObject simManager;
    private SimManager simScriptComp;
    private List <GameObject> drops = new List <GameObject> (); // The actual objects

    void Start () {
        this.simScriptComp = simManager.GetComponent<SimManager>();
    }

    private void OnTriggerEnter (Collider col) {
        if (col.gameObject.tag == "Water") {
            simScriptComp.increasePayload(1);
            drops.Add(col.gameObject);
        }
    }

    private void OnTriggerExit (Collider col) {
        if (col.gameObject.tag == "Water") {
            simScriptComp.decreasePayload(1);
            drops.Remove(col.gameObject);
        }
    }

    /*
    * Remove the specified amount of droplets from the container
    * For use with impairments, or game resets, etc.
    */
    public void removeDropsFromContainer (int amountToRemove) {

        int i = 0;
        foreach (GameObject drop in drops) {
            if (i >= amountToRemove)
                break;
            drops.Remove(drop);
            Destroy(drop);
            simScriptComp.decreasePayload(1);
            i++;
        }
    }
}
