using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionMessage : MonoBehaviour {

    public GameObject simManager;
    public Text message;

    // Use this for initialization
    void Awake () {
        message = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        message.text = "Transitioning to day:\n" + simManager.GetComponent<SimManager>().getCurrentDay().ToString();
        // If new impairment, append to text that there has been an impiarment

    }
}
