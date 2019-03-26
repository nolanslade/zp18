using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class HandTracker : MonoBehaviour {

    public enum HandSide
    {
        RIGHT,
        LEFT,
        NONE
    }

    public GameObject physicalHandObj;
    private Valve.VR.InteractionSystem.Hand handScr;
    public float X_ROTATE, Y_ROTATE, Z_ROTATE;
    public float X_TRANSLATE, Y_TRANSLATE, Z_TRANSLATE;
    public float compareThreshold;      // How close do the transforms need to be to be 'equal'
    public float baseApproachSpeed;     // Scale this by the impairment factor
    public float maximumShakeOffset;    // Shake strength relative to this value (0.8 recommended)
    public bool useSmoothedJitter;      // Instead of randomly picking a location on every frame,
                                        // make it more smooth by picking a random destination, then 
                                        // moving towards it at a constant rate, and then repeat.
                                        
    private const float IMPAIRMENT_CAST_PRECISION   = 1000.0f;

    private System.Random random = new System.Random ();
    private int activeImpairmentAmt = 0;
    private bool impaired = false;                                          // We probably don't need this
    private float scaledMoveSpeed;                                          // Will be ase * impairment factor
    private Vector3 approachDestination;
    private ushort ImpairmentStr;
    private const ushort MAX_SHAKE_STR = (ushort) 3999;

    public float customRefreshRate;
    private float elapsed;

    void Start ()
    {
        this.handScr = this.gameObject.GetComponent<Valve.VR.InteractionSystem.Hand>();
        this.elapsed = 0.0f;
        this.ImpairmentStr = (ushort)0;
    }

    // Update is called once per frame
    void Update () {

        if (impaired) {
            elapsed += Time.deltaTime;
            if (elapsed > customRefreshRate)
            {
                handScr.TriggerHapticPulse((ushort)(ImpairmentStr * MAX_SHAKE_STR));
                elapsed = 0.0f;
            }
            if (!useSmoothedJitter) {
                this.transform.position = generateRandomDestination ();
            } else {
                this.transform.position = Vector3.MoveTowards (transform.position, approachDestination, scaledMoveSpeed);
                if (positionMatch(this.transform.position, approachDestination)) approachDestination = generateRandomDestination ();
            }
        } else {
            this.transform.position = new Vector3 (
                physicalHandObj.transform.localPosition.x * SimManager.UNITY_VIVE_SCALE + X_TRANSLATE,
                physicalHandObj.transform.localPosition.y * SimManager.UNITY_VIVE_SCALE + Y_TRANSLATE,
                physicalHandObj.transform.localPosition.z * SimManager.UNITY_VIVE_SCALE + Z_TRANSLATE
            );
        }

        this.transform.rotation = physicalHandObj.transform.rotation;
        this.transform.Rotate(X_ROTATE, Y_ROTATE, Z_ROTATE);
            
    }

    /*
    * Stephanie's feedback was that the jitter was unnatural / too random 
    * which is fair, considering we randomly pick a spot on every frame.
    * This is an alternative way, where we pick a random destination, then
    * move to it, and repeat. It should be a bit smoother.
    *
    * This function generates that destination.
    */
    private Vector3 generateRandomDestination () {
        return new Vector3 (
            physicalHandObj.transform.localPosition.x * SimManager.UNITY_VIVE_SCALE + X_TRANSLATE + (((float) random.Next(activeImpairmentAmt)) / IMPAIRMENT_CAST_PRECISION),
            physicalHandObj.transform.localPosition.y * SimManager.UNITY_VIVE_SCALE + Y_TRANSLATE + (((float) random.Next(activeImpairmentAmt)) / IMPAIRMENT_CAST_PRECISION),
            physicalHandObj.transform.localPosition.z * SimManager.UNITY_VIVE_SCALE + Z_TRANSLATE + (((float) random.Next(activeImpairmentAmt)) / IMPAIRMENT_CAST_PRECISION)
        );
    }


    /*
    * See if the movement towards the point has been completed
    */
    private bool positionMatch (Vector3 v1, Vector3 v2) {
        return (
            System.Math.Abs(v1.x-v2.x) < compareThreshold &&
            System.Math.Abs(v1.y-v2.y) < compareThreshold && 
            System.Math.Abs(v1.z-v2.z) < compareThreshold
        );
    }


    /*
    * Offsets the transform by a random value below or equal to the given value
    */
    public void applyImpairment (float impairmentStrength) {

        if (useSmoothedJitter) { 
            approachDestination = generateRandomDestination (); 
            scaledMoveSpeed = impairmentStrength * baseApproachSpeed;
        }
        ImpairmentStr = (ushort)impairmentStrength;
        this.activeImpairmentAmt = (int) (1000 * impairmentStrength * maximumShakeOffset);
        this.impaired = true;
    }


    public void clearImpairment () {
        this.activeImpairmentAmt = 0;
        this.ImpairmentStr = (ushort)0;
        this.impaired = false;
    }


    /*
    * Takes current strength and multiplies it by factor to either
    * decrease or increase the strenght of the impairment
    */
    public void modifyStrength (float factor) {
        Debug.Log ("Modifying shake strength " + activeImpairmentAmt.ToString() + " by factor " + factor.ToString());
        this.activeImpairmentAmt = ((int) (activeImpairmentAmt - (activeImpairmentAmt * factor)));
        Debug.Log ("New strength: " + activeImpairmentAmt.ToString());
        ImpairmentStr = (ushort)factor;
    }
}
