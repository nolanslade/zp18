using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionMessage : MonoBehaviour {

    public GameObject simManager;
    public GameObject transitionCountdown;
    private Text countdownText;
    private SimManager simScriptComp;
    private Text message;

    // Use this for initialization
    void Awake () {

	}

    void Start() {
        message = GetComponent<Text>();
        this.countdownText = transitionCountdown.GetComponent<UnityEngine.UI.Text>();
        this.simScriptComp = simManager.GetComponent<SimManager>();
    }

    // Update is called once per frame
    void Update () {
        message.text = "Transitioning to day: " + (simScriptComp.getCurrentDay()+1).ToString();
        countdownText.text = simScriptComp.getRemainingTransitionTime().ToString("0");
        // If new impairment, append to text that there has been an impiarment

    }
}
