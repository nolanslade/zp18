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

    // Use these components inside the update - don't use GetComponent
    private SimManager simManComp;
    private UnityEngine.UI.Text timeRemComp;
    private UnityEngine.UI.Text dayTextComp;
    private UnityEngine.UI.Text moneyTextComp;

    void Start ()
    {
        this.simManComp     = simManager.GetComponent<SimManager>();
        this.timeRemComp    = timeRemainingText.GetComponent<UnityEngine.UI.Text>();
        this.dayTextComp    = dayText.GetComponent<UnityEngine.UI.Text>();
        this.moneyTextComp  = moneyText.GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update () {
        timeRemComp.text = simManComp.ToString();
        dayTextComp.text = "Day: " + simManComp.getCurrentDay().ToString();
        moneyTextComp.text = "$" + simManComp.getCurrentScore().ToString();
    }
}
