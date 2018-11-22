using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraBehaviour : MonoBehaviour {

    //private GameObject physicalCamera = GameObject.FindGameObjectWithTag("physicalCamera");
    const float UNIT_RATIO = 18.77f;    // Ratio of 1 meter to unity units
    GameObject physicalCamera;

    // Use this for initialization
    void Start () {
        physicalCamera = GameObject.FindGameObjectWithTag("physicalCamera");
    }
	
	// Update relative to absolute time
	void FixedUpdate () {
        transform.position = new Vector3 (
            physicalCamera.transform.localPosition.x * UNIT_RATIO,
            physicalCamera.transform.localPosition.y * UNIT_RATIO,
            physicalCamera.transform.localPosition.z * UNIT_RATIO
        );
	}
}