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

    GameObject physicalCamera;

    void Start () {
        physicalCamera = GameObject.FindGameObjectWithTag("physicalCamera");
    }
	
	void Update () {
        transform.position = new Vector3 (
            physicalCamera.transform.localPosition.x * SimManager.UNITY_VIVE_SCALE,
            physicalCamera.transform.localPosition.y * SimManager.UNITY_VIVE_SCALE,
            physicalCamera.transform.localPosition.z * SimManager.UNITY_VIVE_SCALE
        );
	}
}
