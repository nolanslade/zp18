using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdate : MonoBehaviour {

    public GameObject simManager;
    public GameObject timeRemainingText;
    public GameObject dayText;
    public GameObject moneyText;
    public GameObject todayMoneyText;
    public GameObject wageText;

    private string totalDays;

    // Use these components inside the update - don't use GetComponent outside of start()
    private SimManager simManComp;
    private UnityEngine.UI.Text timeRemComp;
    private UnityEngine.UI.Text dayTextComp;
    private UnityEngine.UI.Text moneyTextComp;          // Total score (includes today)
    private UnityEngine.UI.Text todayMoneyTextComp;     // Score today
    private UnityEngine.UI.Text wageTextComp;

    // Only update every set duration, instead of every frame.
    public float customRefreshRate;
    private float elapsed;

    // Different display on simulation completion
    private bool complete = false;
    private bool displayedComplete = false;

    void Start ()
    {
        this.simManComp         = simManager.GetComponent<SimManager>();
        this.timeRemComp        = timeRemainingText.GetComponent<UnityEngine.UI.Text>();
        this.dayTextComp        = dayText.GetComponent<UnityEngine.UI.Text>();
        this.moneyTextComp      = moneyText.GetComponent<UnityEngine.UI.Text>();
        this.todayMoneyTextComp = todayMoneyText.GetComponent<UnityEngine.UI.Text>();
        this.totalDays          = simManComp.getTotalDays().ToString();
        this.wageTextComp       = wageText.GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update () {

        if (!complete) {

            elapsed += Time.deltaTime;

            // Reducing frame-by-frame operations
            if (elapsed > customRefreshRate) {
                complete = simManComp.isComplete(); // this sucks
                timeRemComp.text = simManComp.getRemainingDayTime().ToString("0.0");
                dayTextComp.text = "Day: " + simManComp.getCurrentDay().ToString() + " / " + totalDays;
                moneyTextComp.text = "$" + simManComp.getCurrentScore().ToString("0.00");
                todayMoneyTextComp.text = "$" + simManComp.getDayScore().ToString("0.00");
                wageTextComp.text = "Wage: $" + simManComp.getCurrentDayConfiguration().getRewardMultiplier().ToString("0.00") + "/droplet";
                elapsed = 0.0f;
            }
        }

        else if (!displayedComplete) {
            displayedComplete = true;
            dayTextComp.text = "Day: " + totalDays + " / " + totalDays;
            timeRemComp.text = "Complete!";
        }
    }


    public void setComplete () {
        this.complete = true;
    }
}
