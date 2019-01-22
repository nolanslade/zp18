using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdate : MonoBehaviour {

    public GameObject simManager;
    public GameObject timeRemainingText;
    public UnityEngine.UI.Text text;

	// Use this for initialization
	void Start () {
        simManager = GameObject.Find("SimManager");
        timeRemainingText = GameObject.Find("TimeRemainingAmount");
    }

    // Update is called once per frame
    void Update () {
        timeRemainingText.GetComponent<UnityEngine.UI.Text>().text = simManager.GetComponent<SimManager>().getElapsedDayTime().ToString();
    }
}
