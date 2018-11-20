using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraBehaviour : MonoBehaviour {

    //private GameObject physicalCamera = GameObject.FindGameObjectWithTag("physicalCamera");
    const float UNIT_RATIO = 1 / 25.0f;    // Ratio of Camera object 1M to Unity units. E.g. 1m =

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        GameObject physicalCamera = GameObject.FindGameObjectWithTag("physicalCamera");

        this.gameObject.transform.position = new Vector3 (
            physicalCamera.transform.position.x / UNIT_RATIO,
            physicalCamera.transform.position.y / UNIT_RATIO,
            physicalCamera.transform.position.z / UNIT_RATIO
        );
	}
}
