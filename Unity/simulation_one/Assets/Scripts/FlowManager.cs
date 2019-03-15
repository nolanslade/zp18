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

    private bool canFlow;                   // Sometimes we shouldn't allow the tap to turn on
    private bool flowing;                   // Tap state
    private float elapsed;                  // Since last drop emission

    public GameObject waterDroplet;         // Water drop prefab
    public Vector3 dropletSpawnPoint;       // Should be the nozzle of the tap
    public Vector3 maxDelta;                // Maximum thresholds away from the tap nozzle to spawn
    public float spawnFrequency;            // Wait this amount of time before spawning a new droplet

    public GameObject audioManager;                 // Water sound effects
    private AudioManager audioManagerComponent;

    public GameObject simManager;
    private SimManager simScriptComp;

    void Start () {
        this.audioManagerComponent = audioManager.GetComponent<AudioManager>();
    }

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
        simScriptComp.setCurrentWaterCarry(0);
    }

    public void startFlow () {
        if (canFlow) {
            flowing = true;
            elapsed = 0.0f;
            audioManagerComponent.playSound(AudioManager.SoundType.WATER_FLOW);
        }
    }

    public void stopFlow () {
        flowing = false;
        elapsed = 0.0f;
        audioManagerComponent.stopSound();
    }

    public bool isFlowing () {
        return flowing;
    }

    public bool isFlowable () {
        return this.canFlow;
    }

    public void setFlowable (bool val) {
        this.canFlow = val;
    }
}
