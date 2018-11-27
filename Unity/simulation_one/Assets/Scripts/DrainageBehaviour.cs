using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainageBehaviour : MonoBehaviour {

    bool isTargetDrain;
    GameObject simManager;

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.gameObject.tag == "Water")
        {
            if (this.isTargetDrain)
            {
                simManager.payReward();
            }
            Destroy(col.collider.gameObject);
        }
    }
}
