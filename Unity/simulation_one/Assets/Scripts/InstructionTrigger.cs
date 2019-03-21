using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* McDSL: VR Simulation One
* Nolan Slade
* March 7 2019
*
* Removes markers once the participant enters the appropriate area
* Also self destructs after doing so
*/
public class InstructionTrigger : MonoBehaviour {

    public GameObject simManager;
    public GameObject detectCollisionsWith;
    private SimManager simManagerComponent;
    public GameObject destroyOnTrigger; // The attached object will be removed once the user is inside the target area

    public SimManager.TutorialStep advanceToThisStep;

    void Start () {
        this.simManagerComponent = simManager.GetComponent<SimManager>();
    }

    private void OnTriggerEnter(Collider col)
    {
        // This should only trigger if the colliding object is the headset
        if (col.gameObject == detectCollisionsWith) {
            if (destroyOnTrigger??false) Destroy(destroyOnTrigger); // ?? is a null check, but it's destruction safe
            if (advanceToThisStep != SimManager.TutorialStep.NULL) simManagerComponent.advanceTutorialStep(advanceToThisStep);
            else simManagerComponent.advanceTutorialStep();
            Destroy(gameObject);
        }
    }
}
