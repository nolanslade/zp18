using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainageBehaviour : MonoBehaviour {

    public bool isTargetDrain;
    public GameObject simManager;

    void OnCollisionEnter(Collision col) {
        if (col.collider.gameObject.tag == "Water") {
            if (this.isTargetDrain) {
                simManager.GetComponent<SimManager>().payReward();
            } Destroy(col.collider.gameObject);
        }
    }
}
