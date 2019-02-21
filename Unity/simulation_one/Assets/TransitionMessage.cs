using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionMessage : MonoBehaviour {

    public GameObject simManager;
    private SimManager simScriptComp;
    public Text message;

    // Use this for initialization
    void Awake () {
        message = GetComponent<Text>();
	}

    void Start() {
        this.simScriptComp = simManager.GetComponent<SimManager>();
    }

    // Update is called once per frame
    void Update () {
        message.text = "Transitioning to day:\n" + simScriptComp.getCurrentDay().ToString();
        // If new impairment, append to text that there has been an impiarment

    }
}
