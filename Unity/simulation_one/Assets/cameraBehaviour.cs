using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraBehaviour : MonoBehaviour {

    //private GameObject physicalCamera = GameObject.FindGameObjectWithTag("physicalCamera");
    const float UNIT_RATIO = 2.5f / 65.0f;    // Ratio of Camera object 1M to Unity units. E.g. 1m =

	// Use this for initialization
	void Start () {
		
	}
	
	// Update relative to absolute time
	void FixedUpdate () {

        GameObject physicalCamera = GameObject.FindGameObjectWithTag("physicalCamera");

        transform.position = new Vector3 (
            physicalCamera.transform.localPosition.x / UNIT_RATIO,
            physicalCamera.transform.localPosition.y / UNIT_RATIO,
           physicalCamera.transform.localPosition.z / UNIT_RATIO
        );
    
	}
}