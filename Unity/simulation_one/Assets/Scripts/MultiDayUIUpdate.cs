using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
* Nolan Slade
* April 12 2019
*
* Wall-UI update class; enhanced to support
* multiple day-by-day earnings configurations
* on the wall. 
*
* Currently supports:
* 1-Day
* 2-Day
* 3-Day
* 4 or greater days: will take most recent 4
*/
public class MultiDayUIUpdate : MonoBehaviour {

	// From old UI Update
	public float customRefreshRate;

	// New: different sets of objects for the UI depending on # days
	public GameObject oneDayObjs;
	public GameObject twoDayObjs;
	public GameObject threeDayObjs;
	public GameObject fourDayObjs;

	// The above; broken down into individual objs
	public GameObject oneDayEarnDollars;
	public GameObject oneDayEarnTitle;
	public GameObject twoDayEarnDollars_1;
	public GameObject twoDayEarnDollars_2;
	public GameObject twoDayEarnTitle_1;
	public GameObject twoDayEarnTitle_2;
	public GameObject threeDayEarnDollars_1;
	public GameObject threeDayEarnDollars_2;
	public GameObject threeDayEarnDollars_3;
	public GameObject threeDayEarnTitle_1;
	public GameObject threeDayEarnTitle_2;
	public GameObject threeDayEarnTitle_3;
	public GameObject fourDayEarnDollars_1;
	public GameObject fourDayEarnDollars_2;
	public GameObject fourDayEarnDollars_3;
	public GameObject fourDayEarnDollars_4;
	public GameObject fourDayEarnTitle_1;
	public GameObject fourDayEarnTitle_2;
	public GameObject fourDayEarnTitle_3;
	public GameObject fourDayEarnTitle_4;

	private UnityEngine.UI.Text oneDayEarnDollarsComp;
	private UnityEngine.UI.Text oneDayEarnTitleComp;
	private UnityEngine.UI.Text twoDayEarnDollars_1Comp;
	private UnityEngine.UI.Text twoDayEarnDollars_2Comp;
	private UnityEngine.UI.Text twoDayEarnTitle_1Comp;
	private UnityEngine.UI.Text twoDayEarnTitle_2Comp;
	private UnityEngine.UI.Text threeDayEarnDollars_1Comp;
	private UnityEngine.UI.Text threeDayEarnDollars_2Comp;
	private UnityEngine.UI.Text threeDayEarnDollars_3Comp;
	private UnityEngine.UI.Text threeDayEarnTitle_1Comp;
	private UnityEngine.UI.Text threeDayEarnTitle_2Comp;
	private UnityEngine.UI.Text threeDayEarnTitle_3Comp;
	private UnityEngine.UI.Text fourDayEarnDollars_1Comp;
	private UnityEngine.UI.Text fourDayEarnDollars_2Comp;
	private UnityEngine.UI.Text fourDayEarnDollars_3Comp;
	private UnityEngine.UI.Text fourDayEarnDollars_4Comp;
	private UnityEngine.UI.Text fourDayEarnTitle_1Comp;
	private UnityEngine.UI.Text fourDayEarnTitle_2Comp;
	private UnityEngine.UI.Text fourDayEarnTitle_3Comp;
	private UnityEngine.UI.Text fourDayEarnTitle_4Comp;

	// Also from old UI Update
	private SimManager simManComp;
	public GameObject simManager;
    public GameObject timeRemainingText;
    public GameObject dayText;
    public GameObject wageText;
	public GameObject totalEarnings;
    private UnityEngine.UI.Text timeRemComp;
    private UnityEngine.UI.Text dayTextComp;
    private UnityEngine.UI.Text wageTextComp;
    private UnityEngine.UI.Text totalEarningsComp;
    private float elapsed;
    private bool complete = false;
    private bool displayedComplete = false;
    private bool configured = false;
    private int totalDaysInt;
    private string totalDaysStr;
    private string currentWageStr;
    private string currentDayStr;
    private int currentDayInt;
    private const string PLACE_HOLDER = "--";

    // Reducing the amount of script calls in update() by getting the
    // non-dynamic day-by-day elements once per day.
    public void setCurrentDay (int d) {
    	this.currentDayStr = d.ToString();
    	this.currentDayInt = d;
    }

    // No need to update once the simulation is complete
    public void setComplete () {
        this.complete = true;
    }

    public void setCurrentWage (float w) {
    	this.currentWageStr = w.ToString("0.00");
    }

    public void setTotalDays (int t) {
    	this.totalDaysInt = t;
    	this.totalDaysStr = t.ToString();
    }

    
    public void configure () {

    	Debug.Log("Configuring advanced UI for days: " + totalDaysStr + ".");

    	// New fields
        if (totalDaysInt == 1) {

        	this.twoDayEarnDollars_1Comp 	= null;
        	this.twoDayEarnDollars_2Comp 	= null;
        	this.threeDayEarnDollars_1Comp 	= null;
        	this.threeDayEarnDollars_2Comp 	= null;
        	this.threeDayEarnDollars_3Comp 	= null;
        	this.fourDayEarnDollars_1Comp	= null;
        	this.fourDayEarnDollars_2Comp	= null;
        	this.fourDayEarnDollars_3Comp	= null;
        	this.fourDayEarnDollars_4Comp	= null;

        	oneDayObjs.SetActive(true);
			Destroy(twoDayObjs);
        	Destroy(threeDayObjs);
        	Destroy(fourDayObjs);
        	Debug.Log("Advanced UI configuration complete for days: " + totalDaysStr + ".");
        	configured = true;
        } 

        else if (totalDaysInt == 2) {

        	this.oneDayEarnDollarsComp 		= null;
        	this.threeDayEarnDollars_1Comp 	= null;
        	this.threeDayEarnDollars_2Comp 	= null;
        	this.threeDayEarnDollars_3Comp 	= null;
        	this.fourDayEarnDollars_1Comp	= null;
        	this.fourDayEarnDollars_2Comp	= null;
        	this.fourDayEarnDollars_3Comp	= null;
        	this.fourDayEarnDollars_4Comp	= null;

        	twoDayObjs.SetActive(true);
        	Destroy(oneDayObjs);
        	Destroy(threeDayObjs);
        	Destroy(fourDayObjs);
        	Debug.Log("Advanced UI configuration complete for days: " + totalDaysStr + ".");
        	configured = true;
        } 

        else if (totalDaysInt == 3) {

        	this.oneDayEarnDollarsComp 		= null;
        	this.twoDayEarnDollars_1Comp 	= null;
        	this.twoDayEarnDollars_2Comp 	= null;
        	this.fourDayEarnDollars_1Comp	= null;
        	this.fourDayEarnDollars_2Comp	= null;
        	this.fourDayEarnDollars_3Comp	= null;
        	this.fourDayEarnDollars_4Comp	= null;

        	threeDayObjs.SetActive(true);
        	Destroy(oneDayObjs);
        	Destroy(twoDayObjs);
        	Destroy(fourDayObjs);
        	Debug.Log("Advanced UI configuration complete for days: " + totalDaysStr + ".");
        	configured = true;
        } 

        else if (totalDaysInt > 3) {

        	this.oneDayEarnDollarsComp 		= null;
        	this.twoDayEarnDollars_1Comp 	= null;
        	this.twoDayEarnDollars_2Comp 	= null;
        	this.threeDayEarnDollars_1Comp 	= null;
        	this.threeDayEarnDollars_2Comp 	= null;
        	this.threeDayEarnDollars_3Comp 	= null;

        	fourDayObjs.SetActive(true);
        	Destroy(oneDayObjs);
        	Destroy(twoDayObjs);
        	Destroy(threeDayObjs);
        	Debug.Log("Advanced UI configuration complete for days: " + totalDaysStr + ".");
        	configured = true;
        } 

        else {
        	Debug.Log("UI STARTUP ERROR: Unexpected number of days.");
        }
    }

	// Cache all key components for efficient access and destroy / nullify
	// all unneeded components and game objects
	void Start () {

		// Existing fields
        this.simManComp         = this.simManager.GetComponent<SimManager>();
        this.timeRemComp        = this.timeRemainingText.GetComponent<UnityEngine.UI.Text>();
        this.dayTextComp        = this.dayText.GetComponent<UnityEngine.UI.Text>();
        this.wageTextComp       = this.wageText.GetComponent<UnityEngine.UI.Text>();

        // Each combination of texts
        this.oneDayEarnDollarsComp 		= this.oneDayEarnDollars.GetComponent<UnityEngine.UI.Text>();
        this.oneDayEarnTitleComp		= this.oneDayEarnTitle.GetComponent<UnityEngine.UI.Text>();
        this.twoDayEarnDollars_1Comp 	= this.twoDayEarnDollars_1.GetComponent<UnityEngine.UI.Text>();
        this.twoDayEarnDollars_2Comp 	= this.twoDayEarnDollars_2.GetComponent<UnityEngine.UI.Text>();
        this.twoDayEarnTitle_1Comp 		= this.twoDayEarnTitle_1.GetComponent<UnityEngine.UI.Text>();
        this.twoDayEarnTitle_2Comp 		= this.twoDayEarnTitle_2.GetComponent<UnityEngine.UI.Text>();
        this.threeDayEarnDollars_1Comp 	= this.threeDayEarnDollars_1.GetComponent<UnityEngine.UI.Text>();
        this.threeDayEarnDollars_2Comp 	= this.threeDayEarnDollars_2.GetComponent<UnityEngine.UI.Text>();
        this.threeDayEarnDollars_3Comp 	= this.threeDayEarnDollars_3.GetComponent<UnityEngine.UI.Text>();
        this.threeDayEarnTitle_1Comp 	= this.threeDayEarnTitle_1.GetComponent<UnityEngine.UI.Text>();
        this.threeDayEarnTitle_2Comp 	= this.threeDayEarnTitle_2.GetComponent<UnityEngine.UI.Text>();
        this.threeDayEarnTitle_3Comp 	= this.threeDayEarnTitle_3.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnDollars_1Comp	= this.fourDayEarnDollars_1.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnDollars_2Comp	= this.fourDayEarnDollars_2.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnDollars_3Comp	= this.fourDayEarnDollars_3.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnDollars_4Comp	= this.fourDayEarnDollars_4.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnTitle_1Comp		= this.fourDayEarnTitle_1.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnTitle_2Comp		= this.fourDayEarnTitle_2.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnTitle_3Comp		= this.fourDayEarnTitle_3.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnTitle_4Comp		= this.fourDayEarnTitle_4.GetComponent<UnityEngine.UI.Text>();

        this.currentDayInt = 0;
    }
	

	void Update () {

		if (configured) {
			if (!complete) {

	            elapsed += Time.deltaTime;

	            // Reducing frame-by-frame operations
	            if (elapsed > customRefreshRate) {

	            	// This is crappy. We don't have to fetch these every time, except for remaining time
	                complete = simManComp.isComplete(); 
	                timeRemComp.text = simManComp.getRemainingDayTime().ToString("0.0");
	                dayTextComp.text = "Day: " + currentDayStr + " / " + totalDaysStr;
	                wageTextComp.text = "Wage: $" + currentWageStr + " / ball";
	                totalEarningsComp.text = "Earnings: $" + simManComp.getCurrentScore().ToString("0.00");
	                
	                // Set the corresponding earnings texts, depending on the experiment set up
	                // The only non-trivial case is when there are more than four days; then,
	                // we'll get the four most recent days (including today) and display those
	                try {
		                if (totalDaysInt == 1) {
		                	this.oneDayEarnTitleComp.text = (currentDayInt == 0) ? "Day 0" : "Day 1" ;
		                	this.oneDayEarnDollarsComp.text = "$" + simManComp.getDayScore().ToString("0.00");
		                } 

		                else if (totalDaysInt == 2) {
		                	switch (currentDayInt) {
		                		case 0:
		                			this.twoDayEarnTitle_1Comp.text = "Day 0";
		                			this.twoDayEarnTitle_2Comp.text = "Day 1";
		                			this.twoDayEarnDollars_1Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.twoDayEarnDollars_2Comp.text = PLACE_HOLDER;
		                			break;
		                		case 1:
		                			this.twoDayEarnTitle_1Comp.text = "Day 1";
		                			this.twoDayEarnTitle_2Comp.text = "Day 2";
		                			this.twoDayEarnDollars_1Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.twoDayEarnDollars_2Comp.text = PLACE_HOLDER;
		                			break;
		                		case 2:
		                			this.twoDayEarnDollars_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
		                			this.twoDayEarnDollars_2Comp.text = simManComp.getDayScore().ToString("0.00");
		                			break;
		                		default:
		                			Debug.Log("UI ERROR: Invalid Day Number");
		                			break;
		                	}
		                } 

		                else if (totalDaysInt == 3) {
		                	switch (currentDayInt) {
		                		case 0:
		                			this.threeDayEarnTitle_1Comp.text = "Day 0";
		                			this.threeDayEarnTitle_2Comp.text = "Day 1";
		                			this.threeDayEarnTitle_3Comp.text = "Day 2";
		                			this.threeDayEarnDollars_1Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.threeDayEarnDollars_2Comp.text = PLACE_HOLDER;
		                			this.threeDayEarnDollars_3Comp.text = PLACE_HOLDER;
		                			break;
		                		case 1:
		                			this.threeDayEarnTitle_1Comp.text = "Day 1";
		                			this.threeDayEarnTitle_2Comp.text = "Day 2";
		                			this.threeDayEarnTitle_3Comp.text = "Day 3";
		                			this.threeDayEarnDollars_1Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.threeDayEarnDollars_2Comp.text = PLACE_HOLDER;
		                			this.threeDayEarnDollars_3Comp.text = PLACE_HOLDER;
		                			break;
		                		case 2:
		                			this.threeDayEarnDollars_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
		                			this.threeDayEarnDollars_2Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.threeDayEarnDollars_3Comp.text = PLACE_HOLDER;
		                			break;
		                		case 3:
		                			this.threeDayEarnDollars_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
		                			this.threeDayEarnDollars_2Comp.text = simManComp.earningsByDay[1].ToString("0.00");
		                			this.threeDayEarnDollars_3Comp.text = simManComp.getDayScore().ToString("0.00");
		                			break;
		                		default:
		                			Debug.Log("UI ERROR: Invalid Day Number");
		                			break;
		                	}
		                } 

		                else if (totalDaysInt == 4) {
		                	switch (currentDayInt) {
		                		case 0:
		                			this.fourDayEarnTitle_1Comp.text = "Day 0";
		                			this.fourDayEarnTitle_2Comp.text = "Day 1";
		                			this.fourDayEarnTitle_3Comp.text = "Day 2";
		                			this.fourDayEarnTitle_4Comp.text = "Day 3";
		                			this.fourDayEarnDollars_1Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_3Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_4Comp.text = PLACE_HOLDER;
		                			break;
		                		case 1:
		                			this.fourDayEarnTitle_1Comp.text = "Day 1";
		                			this.fourDayEarnTitle_2Comp.text = "Day 2";
		                			this.fourDayEarnTitle_3Comp.text = "Day 3";
		                			this.fourDayEarnTitle_4Comp.text = "Day 4";
		                			this.fourDayEarnDollars_1Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_3Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_4Comp.text = PLACE_HOLDER;
		                			break;
		                		case 2:
		                			this.fourDayEarnDollars_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.fourDayEarnDollars_3Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_4Comp.text = PLACE_HOLDER;
		                			break;
		                		case 3:
		                			this.fourDayEarnDollars_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = simManComp.earningsByDay[1].ToString("0.00");
		                			this.fourDayEarnDollars_3Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.fourDayEarnDollars_4Comp.text = PLACE_HOLDER;
		                			break;
		                		case 4:
		                			this.fourDayEarnDollars_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = simManComp.earningsByDay[1].ToString("0.00");
		                			this.fourDayEarnDollars_3Comp.text = simManComp.earningsByDay[2].ToString("0.00");
		                			this.fourDayEarnDollars_4Comp.text = simManComp.getDayScore().ToString("0.00");
		                			break;
		                		default:
		                			Debug.Log("UI ERROR: Invalid Day Number");
		                			break;
		                	}
		                } 

		                else if (totalDaysInt > 4) {
		                	switch (currentDayInt) {
		                		case 0:
		                			this.fourDayEarnTitle_1Comp.text = "Day 0";
		                			this.fourDayEarnTitle_2Comp.text = "Day 1";
		                			this.fourDayEarnTitle_3Comp.text = "Day 2";
		                			this.fourDayEarnTitle_4Comp.text = "Day 3";
		                			this.fourDayEarnDollars_1Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_3Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_4Comp.text = PLACE_HOLDER;
		                			break;
		                		case 1:
		                			this.fourDayEarnTitle_1Comp.text = "Day 1";
		                			this.fourDayEarnTitle_2Comp.text = "Day 2";
		                			this.fourDayEarnTitle_3Comp.text = "Day 3";
		                			this.fourDayEarnTitle_4Comp.text = "Day 4";
		                			this.fourDayEarnDollars_1Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_3Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_4Comp.text = PLACE_HOLDER;
		                			break;
		                		case 2:
		                			this.fourDayEarnDollars_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.fourDayEarnDollars_3Comp.text = PLACE_HOLDER;
		                			this.fourDayEarnDollars_4Comp.text = PLACE_HOLDER;
		                			break;
		                		case 3:
		                			this.fourDayEarnDollars_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = simManComp.earningsByDay[1].ToString("0.00");
		                			this.fourDayEarnDollars_3Comp.text = simManComp.getDayScore().ToString("0.00");
		                			this.fourDayEarnDollars_4Comp.text = PLACE_HOLDER;
		                			break;
		                		default:
		                			this.fourDayEarnTitle_1Comp.text = "Day " + (currentDayInt-4).ToString();
		                			this.fourDayEarnTitle_2Comp.text = "Day " + (currentDayInt-3).ToString();
		                			this.fourDayEarnTitle_3Comp.text = "Day " + (currentDayInt-2).ToString();
		                			this.fourDayEarnTitle_4Comp.text = "Day " + currentDayStr;
									this.fourDayEarnDollars_1Comp.text = simManComp.earningsByDay[currentDayInt-4].ToString("0.00");
		                			this.fourDayEarnDollars_2Comp.text = simManComp.earningsByDay[currentDayInt-3].ToString("0.00");
		                			this.fourDayEarnDollars_3Comp.text = simManComp.earningsByDay[currentDayInt-2].ToString("0.00");
		                			this.fourDayEarnDollars_4Comp.text = simManComp.getDayScore().ToString("0.00");
		                			break;
		                	}
		                }
	            	} 

	            	catch (System.Exception e) {
	            		Debug.Log("** MULTIDAY UI EXCEPTION **");
	            		Debug.Log(e);
	            	}

	                elapsed = 0.0f;
	            }
	        }
    	}

        else if (!displayedComplete) {
            displayedComplete = true;
            dayTextComp.text = "Day: " + totalDaysStr + " / " + totalDaysStr;
            timeRemComp.text = "Complete!";
        }
	}
}
