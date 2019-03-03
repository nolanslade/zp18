using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracker : MonoBehaviour {

    public GameObject physicalHandObj;
    public float X_ROTATE, Y_ROTATE, Z_ROTATE;
    public float X_TRANSLATE, Y_TRANSLATE, Z_TRANSLATE;

    private int activeImpairmentAmt = 0;
    private bool impaired = false;  // We probably don't need this
    private System.Random random = new System.Random ();
	
	// Update is called once per frame
	void Update () {

        if (impaired) {
            this.transform.position = new Vector3(
                physicalHandObj.transform.localPosition.x * 18.77f + X_TRANSLATE + (((float) random.Next(activeImpairmentAmt)) / 1000.0f),
                physicalHandObj.transform.localPosition.y * 18.77f + Y_TRANSLATE + (((float) random.Next(activeImpairmentAmt)) / 1000.0f),
                physicalHandObj.transform.localPosition.z * 18.77f + Z_TRANSLATE + (((float) random.Next(activeImpairmentAmt)) / 1000.0f)
            );
        } else {
            this.transform.position = new Vector3(
                physicalHandObj.transform.localPosition.x * 18.77f + X_TRANSLATE,
                physicalHandObj.transform.localPosition.y * 18.77f + Y_TRANSLATE,
                physicalHandObj.transform.localPosition.z * 18.77f + Z_TRANSLATE
            );
        }

        this.transform.rotation = physicalHandObj.transform.rotation;
        this.transform.Rotate(X_ROTATE, Y_ROTATE, Z_ROTATE);
	}

    /*
    * Offsets the transform by a random value below or equal to the given value
    */
    public void applyImpairment (float maxOffset) {
        this.activeImpairmentAmt = (int) (1000 * maxOffset);
        this.impaired = true;
    }

    public void clearImpairment () {
        this.activeImpairmentAmt = 0;
        this.impaired = false;
    }


    /*
    * Takes current strength and multiplies it by factor to either
    * decrease or increase the strenght of the impairment
    */
    public void modifyStrength (float factor) {
        Debug.Log ("Modifying shake strength " + activeImpairmentAmt.ToString() + " by factor " + factor.ToString());
        this.activeImpairmentAmt = ((int) (activeImpairmentAmt * factor));
        Debug.Log ("New strength: " + activeImpairmentAmt.ToString());
    }
}
