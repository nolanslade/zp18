using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainageBehaviour : MonoBehaviour {

    public bool isTargetDrain, registersSpills;
    public GameObject simManager;
    public GameObject audioManager;                 // Water sound effects
    private SimManager simScriptComp;
    private AudioManager audioManagerComponent;

    private float elapsed;                  // Since last drop emission
    private bool soundOn;


    void Start () {
        this.simScriptComp = this.simManager.GetComponent<SimManager>();
        this.audioManagerComponent = audioManager.GetComponent<AudioManager>();
        soundOn = false;
        elapsed = 0.0f;
    }

    void Update()
    {
        if (soundOn)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= 0.5f)
            {
                audioManagerComponent.stopSound();
                soundOn = false;
                elapsed = 0.0f;
            }
        }
    }

    void OnCollisionEnter(Collision col) {
        Debug.Log("collides");
        if (col.collider.gameObject.tag == "Water") {
            if (this.isTargetDrain) {
                elapsed = 0.0f;
                if (!soundOn)
                {
                    audioManagerComponent.playSound(AudioManager.SoundType.WATER_FLOW);
                    soundOn = true;
                }
                simScriptComp.payReward();
            }
            else if (this.registersSpills) {
                simScriptComp.registerSpill();
            } Destroy(col.collider.gameObject);            
        }
    }
}
