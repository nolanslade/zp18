using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Oct 2018
 * 
 * Simple script that allows the unity camera object to track the
 * Vive headset in the lab room. A ratio was determined by measuring out the
 * room and figuring out how much a meter of change corresponds to in Unity distance.
 */
public class CameraBehaviour : MonoBehaviour {

    const float UNIT_RATIO = 18.77f;    // Ratio of 1 meter to unity units
    GameObject physicalCamera;

    void Start () {
        physicalCamera = GameObject.FindGameObjectWithTag("physicalCamera");
    }
	
	void FixedUpdate () {
        transform.position = new Vector3 (
            physicalCamera.transform.localPosition.x * UNIT_RATIO,
            physicalCamera.transform.localPosition.y * UNIT_RATIO,
            physicalCamera.transform.localPosition.z * UNIT_RATIO
        );
	}
}
