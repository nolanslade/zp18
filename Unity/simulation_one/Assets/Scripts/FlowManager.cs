using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Manages behaviour of the tap / provides methods
to start and stop the flow of water. If the tap
is running, then this script will also spawn 
water according to its parameters.
*/

public class FlowManager : MonoBehaviour {

    private bool flowing;                   // Tap state
    private float elapsed;                  // Since last drop emission

    public GameObject waterDroplet;         // Water drop prefab
    public Vector3 dropletSpawnPoint;       // Should be the nozzle of the tap
    public Vector3 maxDelta;                // Maximum thresholds away from the tap nozzle to spawn
    public float spawnFrequency;            // Wait this amount of time before spawning a new droplet

	// Update is called once per frame
	void Update () {
        if (flowing && elapsed > spawnFrequency) {
            Instantiate (waterDroplet, dropletSpawnPoint, Quaternion.identity);
            elapsed = 0.0f;
        } elapsed += Time.deltaTime;
	}

    public void cleanScene () {
        stopFlow();
        GameObject[] allDrops = GameObject.FindGameObjectsWithTag("Water");
        foreach (GameObject drop in allDrops) Destroy (drop);
    }

    public void startFlow () {
        flowing = true;
        elapsed = 0.0f;
    }

    public void stopFlow () {
        flowing = false;
        elapsed = 0.0f;
    }

    public bool isFlowing () {
        return flowing;
    }
}
