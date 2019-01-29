using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracker : MonoBehaviour {

    public GameObject physicalHandObj;
    public float X_ROTATE, Y_ROTATE, Z_ROTATE;
    public float X_TRANSLATE, Y_TRANSLATE, Z_TRANSLATE;
	
	// Update is called once per frame
	void Update () {

        this.transform.position = new Vector3 (
            physicalHandObj.transform.localPosition.x * 18.77f + X_TRANSLATE,
            physicalHandObj.transform.localPosition.y * 18.77f + Y_TRANSLATE,
            physicalHandObj.transform.localPosition.z * 18.77f + Z_TRANSLATE
        );

        this.transform.rotation = physicalHandObj.transform.rotation;
        this.transform.Rotate(X_ROTATE, Y_ROTATE, Z_ROTATE);
	}
}
