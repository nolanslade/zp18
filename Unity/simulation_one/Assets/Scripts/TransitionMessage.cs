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

    void Start() {
        message = GetComponent<Text>();
        this.countdownText = transitionCountdown.GetComponent<UnityEngine.UI.Text>();
        this.simScriptComp = simManager.GetComponent<SimManager>();
    }

    // Nolan April 2019 - Adding a bit more detail to the transition message
    void Update () {
        countdownText.text = simScriptComp.getRemainingTransitionTime().ToString("0");
        message.text = ((simScriptComp.dayHasImpairment(simScriptComp.getCurrentDay()+1)) 
            ? "You Are Now Impaired!\n" 
            : "You Have Full Health.\n") 
            + "Preparing to Start Day " 
            + (simScriptComp.getCurrentDay()+1).ToString();
    }
}
