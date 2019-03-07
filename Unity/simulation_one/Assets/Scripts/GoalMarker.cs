using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* McDSL: VR Simulation One
* Nolan Slade
* March 7 2019
*
* Simple animated markers to go along with instructions.
*/
public class GoalMarker : MonoBehaviour {

    public float moveSpeed;     // Speed between bounds
    public float minHeight;
    public float maxHeight;
	
	// Update is called once per frame - THIS IS ONLY CALLED WHEN ENABLED
	void Update () {
        transform.position = new Vector3(transform.position.x, PingPong(Time.time*moveSpeed, minHeight, maxHeight), transform.position.z);
	}

    float PingPong (float t, float min, float max) {
        return Mathf.PingPong(t, max-min) + min;
    }
}
