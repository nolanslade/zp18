using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdate : MonoBehaviour {

    public GameObject simManager;
    public GameObject timeRemainingText;
    public GameObject dayText;
    public GameObject moneyText;
    public UnityEngine.UI.Text text;

	// Use this for initialization
	void Start () {
        simManager = GameObject.Find("SimManager");
        timeRemainingText = GameObject.Find("TimeRemainingAmount");
        dayText = GameObject.Find("DayAmount");
        moneyText = GameObject.Find("Money");
    }

    // Update is called once per frame
    void Update () {
        timeRemainingText.GetComponent<UnityEngine.UI.Text>().text = simManager.GetComponent<SimManager>().getElapsedDayTime().ToString();
        dayText.GetComponent<UnityEngine.UI.Text>().text = "Day: " + simManager.GetComponent<SimManager>().getCurrentDay().ToString();
        moneyText.GetComponent<UnityEngine.UI.Text>().text = "$" + simManager.GetComponent<SimManager>().getCurrentScore().ToString();
    }
}
