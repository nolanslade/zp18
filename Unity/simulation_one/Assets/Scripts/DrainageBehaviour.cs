using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainageBehaviour : MonoBehaviour {

    public bool isTargetDrain, registersSpills;
    public GameObject simManager;
    private SimManager simScriptComp;

    void Start () {
        this.simScriptComp = this.simManager.GetComponent<SimManager>();
    }

    void OnCollisionEnter(Collision col) {
        if (col.collider.gameObject.tag == "Water") {
            if (this.isTargetDrain) {
                simScriptComp.payReward();
            } else if (this.registersSpills) {
                simScriptComp.registerSpill();
            } Destroy(col.collider.gameObject);
        }
    }
}
