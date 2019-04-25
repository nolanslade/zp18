using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Nolan Slade
* April 24 2019
*
* Move the bucket to a specific location if it's
* dropped within the bounds of the collider. Or,
* move the bucket back into the room using an 
* adjustment amount.
*/
public class BucketCollector : MonoBehaviour {

    public GameObject containerObj;
    public GameObject simManager;
    private SimManager simManComp;

    public float spawnX, spawnY, spawnZ;            // Absolute spawn point

    public bool relativeAdjustment;                 // The spawn point will be relative to
    public float relativeAdj;                       // current position rather than absolute.
    public bool relativeX, relativeY, relativeZ;    // Apply adjustment over these axes


    private void Start () {
        this.simManComp = simManager.GetComponent<SimManager>();
    }


    private void OnTriggerStay (Collider col) {

        if (col.gameObject == containerObj) {

            // Reset rotational values
            containerObj.transform.eulerAngles = new Vector3 (0.0f, 0.0f, 0.0f);

            // Spawn point depends on what kind of adjustment is specified
            // (absolute or relative)
            if (relativeAdjustment) {
               
               if (relativeX) {
                    containerObj.transform.position = new Vector3 (
                        containerObj.transform.position.x + relativeAdj,
                        containerObj.transform.position.y,
                        containerObj.transform.position.z
                    );
                }

                if (relativeY) {
                    containerObj.transform.position = new Vector3 (
                        containerObj.transform.position.x,
                        containerObj.transform.position.y + relativeAdj,
                        containerObj.transform.position.z
                    );
                }

                if (relativeZ) {
                    containerObj.transform.position = new Vector3 (
                        containerObj.transform.position.x,
                        containerObj.transform.position.y,
                        containerObj.transform.position.z + relativeAdj
                    );
                }
            }

            else {
                containerObj.transform.position = new Vector3 (
                    spawnX,
                    spawnY,
                    spawnZ
                ); 
            }
        }
    }
}
