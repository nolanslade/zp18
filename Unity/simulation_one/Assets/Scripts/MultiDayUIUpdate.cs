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
	public GameObject oneDayEarnText;
	public GameObject twoDayEarnText_1;
	public GameObject twoDayEarnText_2;
	public GameObject threeDayEarnText_1;
	public GameObject threeDayEarnText_2;
	public GameObject threeDayEarnText_3;
	public GameObject fourDayEarnText_1;
	public GameObject fourDayEarnText_2;
	public GameObject fourDayEarnText_3;
	public GameObject fourDayEarnText_4;
	public GameObject fourDayEarnTitle_1;
	public GameObject fourDayEarnTitle_2;
	public GameObject fourDayEarnTitle_3;
	public GameObject fourDayEarnTitle_4;

	private UnityEngine.UI.Text oneDayEarnTextComp;
	private UnityEngine.UI.Text twoDayEarnText_1Comp;
	private UnityEngine.UI.Text twoDayEarnText_2Comp;
	private UnityEngine.UI.Text threeDayEarnText_1Comp;
	private UnityEngine.UI.Text threeDayEarnText_2Comp;
	private UnityEngine.UI.Text threeDayEarnText_3Comp;
	private UnityEngine.UI.Text fourDayEarnText_1Comp;
	private UnityEngine.UI.Text fourDayEarnText_2Comp;
	private UnityEngine.UI.Text fourDayEarnText_3Comp;
	private UnityEngine.UI.Text fourDayEarnText_4Comp;
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
    private UnityEngine.UI.Text timeRemComp;
    private UnityEngine.UI.Text dayTextComp;
    private UnityEngine.UI.Text wageTextComp;
    private float elapsed;
    private bool complete = false;
    private bool dayZeroComplete = false;
    private bool displayedComplete = false;
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

    public void setCurrentWage (float w) {
    	this.currentWageStr = w.ToString("0.00");
    }

    // No need to update once the simulation is complete
    public void setComplete () {
        this.complete = true;
    }

    // For day 0 we'll always use the 1 day earnings objects
    public void setDayZeroComplete () {

    	this.dayZeroComplete = true;

    	// New fields
        if (totalDaysInt == 1) {

        	this.twoDayEarnText_1Comp 		= null;
        	this.twoDayEarnText_2Comp 		= null;
        	this.threeDayEarnText_1Comp 	= null;
        	this.threeDayEarnText_2Comp 	= null;
        	this.threeDayEarnText_3Comp 	= null;
        	this.fourDayEarnText_1Comp		= null;
        	this.fourDayEarnText_2Comp		= null;
        	this.fourDayEarnText_3Comp		= null;
        	this.fourDayEarnText_4Comp		= null;

			Destroy(twoDayObjs);
        	Destroy(threeDayObjs);
        	Destroy(fourDayObjs);
        	Destroy(twoDayEarnText_1);
        	Destroy(twoDayEarnText_2);
        	Destroy(threeDayEarnText_1);
        	Destroy(threeDayEarnText_2);
        	Destroy(threeDayEarnText_3);
        	Destroy(fourDayEarnText_1);
        	Destroy(fourDayEarnText_2);
        	Destroy(fourDayEarnText_3);
        	Destroy(fourDayEarnText_4); 
        } 

        else if (totalDaysInt == 2) {

        	this.oneDayEarnTextComp 		= null;
        	this.threeDayEarnText_1Comp 	= null;
        	this.threeDayEarnText_2Comp 	= null;
        	this.threeDayEarnText_3Comp 	= null;
        	this.fourDayEarnText_1Comp		= null;
        	this.fourDayEarnText_2Comp		= null;
        	this.fourDayEarnText_3Comp		= null;
        	this.fourDayEarnText_4Comp		= null;

        	this.twoDayObjs.SetActive(true);

        	Destroy(oneDayObjs);
        	Destroy(threeDayObjs);
        	Destroy(fourDayObjs);
        	Destroy(oneDayEarnText);
        	Destroy(threeDayEarnText_1);
        	Destroy(threeDayEarnText_2);
        	Destroy(threeDayEarnText_3);
        	Destroy(fourDayEarnText_1);
        	Destroy(fourDayEarnText_2);
        	Destroy(fourDayEarnText_3);
        	Destroy(fourDayEarnText_4);
        } 

        else if (totalDaysInt == 3) {

        	this.oneDayEarnTextComp 		= null;
        	this.twoDayEarnText_1Comp 		= null;
        	this.twoDayEarnText_2Comp 		= null;
        	this.fourDayEarnText_1Comp		= null;
        	this.fourDayEarnText_2Comp		= null;
        	this.fourDayEarnText_3Comp		= null;
        	this.fourDayEarnText_4Comp		= null;

        	this.threeDayObjs.SetActive(true);

        	Destroy(oneDayObjs);
        	Destroy(twoDayObjs);
        	Destroy(fourDayObjs);
        	Destroy(oneDayEarnText);
        	Destroy(twoDayEarnText_1);
        	Destroy(twoDayEarnText_2);
        	Destroy(fourDayEarnText_1);
        	Destroy(fourDayEarnText_2);
        	Destroy(fourDayEarnText_3);
        	Destroy(fourDayEarnText_4);
        } 

        else if (totalDaysInt > 3) {

        	this.oneDayEarnTextComp 		= null;
        	this.twoDayEarnText_1Comp 		= null;
        	this.twoDayEarnText_2Comp 		= null;
        	this.threeDayEarnText_1Comp 	= null;
        	this.threeDayEarnText_2Comp 	= null;
        	this.threeDayEarnText_3Comp 	= null;

        	this.fourDayObjs.SetActive(true);

        	Destroy(oneDayObjs);
        	Destroy(twoDayObjs);
        	Destroy(threeDayObjs);
        	Destroy(oneDayEarnText);
        	Destroy(twoDayEarnText_1);
        	Destroy(twoDayEarnText_2);
        	Destroy(threeDayEarnText_1);
        	Destroy(threeDayEarnText_2);
        	Destroy(threeDayEarnText_3);
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
        this.totalDaysInt 		= this.simManComp.getTotalDays();
        this.totalDaysStr       = this.totalDaysInt.ToString();
        this.wageTextComp       = this.wageText.GetComponent<UnityEngine.UI.Text>();

        // Each combination of texts
        this.oneDayEarnTextComp 		= this.oneDayEarnText.GetComponent<UnityEngine.UI.Text>();
        this.twoDayEarnText_1Comp 		= this.twoDayEarnText_1.GetComponent<UnityEngine.UI.Text>();
        this.twoDayEarnText_2Comp 		= this.twoDayEarnText_2.GetComponent<UnityEngine.UI.Text>();
        this.threeDayEarnText_1Comp 	= this.threeDayEarnText_1.GetComponent<UnityEngine.UI.Text>();
        this.threeDayEarnText_2Comp 	= this.threeDayEarnText_2.GetComponent<UnityEngine.UI.Text>();
        this.threeDayEarnText_3Comp 	= this.threeDayEarnText_3.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnText_1Comp		= this.fourDayEarnText_1.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnText_2Comp		= this.fourDayEarnText_2.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnText_3Comp		= this.fourDayEarnText_3.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnText_4Comp		= this.fourDayEarnText_4.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnTitle_1Comp		= this.fourDayEarnTitle_1.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnTitle_2Comp		= this.fourDayEarnTitle_2.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnTitle_3Comp		= this.fourDayEarnTitle_3.GetComponent<UnityEngine.UI.Text>();
        this.fourDayEarnTitle_4Comp		= this.fourDayEarnTitle_4.GetComponent<UnityEngine.UI.Text>();

        // We'll only enable them after, if need be.
        this.twoDayObjs.SetActive(false);
        this.threeDayObjs.SetActive(false);
        this.fourDayObjs.SetActive(false);

        this.currentDayInt = 0;
    }
	

	void Update () {


		if (!complete) {

            elapsed += Time.deltaTime;

            // Reducing frame-by-frame operations
            if (elapsed > customRefreshRate) {

            	// This is crappy. We don't have to fetch these every time, except for remaining time
                complete = simManComp.isComplete(); 
                timeRemComp.text = simManComp.getRemainingDayTime().ToString("0.0");
                dayTextComp.text = "Day: " + currentDayStr + " / " + totalDaysStr;
                wageTextComp.text = "Wage: $" + currentWageStr + " / ball";
                
                // Set the corresponding earnings texts, depending on the experiment set up
                // The only non-trivial case is when there are more than four days; then,
                // we'll get the four most recent days (including today) and display those
                try {
	                if (totalDaysInt == 1) {
	                	this.oneDayEarnTextComp.text = "$" + simManComp.getDayScore().ToString("0.00");
	                } 

	                else if (totalDaysInt == 2) {
	                	switch (currentDayInt) {
	                		case 0:
	                		case 1:
	                			this.twoDayEarnText_1Comp.text = simManComp.getDayScore().ToString("0.00");
	                			this.twoDayEarnText_2Comp.text = PLACE_HOLDER;
	                			break;
	                		case 2:
	                			this.twoDayEarnText_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
	                			this.twoDayEarnText_2Comp.text = simManComp.getDayScore().ToString("0.00");
	                			break;
	                		default:
	                			Debug.Log("UI ERROR: Invalid Day Number");
	                			break;
	                	}
	                } 

	                else if (totalDaysInt == 3) {
	                	switch (currentDayInt) {
	                		case 0:
	                		case 1:
	                			this.threeDayEarnText_1Comp.text = simManComp.getDayScore().ToString("0.00");
	                			this.threeDayEarnText_2Comp.text = PLACE_HOLDER;
	                			this.threeDayEarnText_3Comp.text = PLACE_HOLDER;
	                			break;
	                		case 2:
	                			this.threeDayEarnText_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
	                			this.threeDayEarnText_2Comp.text = simManComp.getDayScore().ToString("0.00");
	                			this.threeDayEarnText_3Comp.text = PLACE_HOLDER;
	                			break;
	                		case 3:
	                			this.threeDayEarnText_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
	                			this.threeDayEarnText_2Comp.text = simManComp.earningsByDay[1].ToString("0.00");
	                			this.threeDayEarnText_3Comp.text = simManComp.getDayScore().ToString("0.00");
	                			break;
	                		default:
	                			Debug.Log("UI ERROR: Invalid Day Number");
	                			break;
	                	}
	                } 

	                else if (totalDaysInt == 4) {
	                	switch (currentDayInt) {
	                		case 0:
	                		case 1:
	                			this.fourDayEarnText_1Comp.text = simManComp.getDayScore().ToString("0.00");
	                			this.fourDayEarnText_2Comp.text = PLACE_HOLDER;
	                			this.fourDayEarnText_3Comp.text = PLACE_HOLDER;
	                			this.fourDayEarnText_4Comp.text = PLACE_HOLDER;
	                			break;
	                		case 2:
	                			this.fourDayEarnText_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
	                			this.fourDayEarnText_2Comp.text = simManComp.getDayScore().ToString("0.00");
	                			this.fourDayEarnText_3Comp.text = PLACE_HOLDER;
	                			this.fourDayEarnText_4Comp.text = PLACE_HOLDER;
	                			break;
	                		case 3:
	                			this.fourDayEarnText_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
	                			this.fourDayEarnText_2Comp.text = simManComp.earningsByDay[1].ToString("0.00");
	                			this.fourDayEarnText_3Comp.text = simManComp.getDayScore().ToString("0.00");
	                			this.fourDayEarnText_4Comp.text = PLACE_HOLDER;
	                			break;
	                		case 4:
	                			this.fourDayEarnText_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
	                			this.fourDayEarnText_2Comp.text = simManComp.earningsByDay[1].ToString("0.00");
	                			this.fourDayEarnText_3Comp.text = simManComp.earningsByDay[2].ToString("0.00");
	                			this.fourDayEarnText_4Comp.text = simManComp.getDayScore().ToString("0.00");
	                			break;
	                		default:
	                			Debug.Log("UI ERROR: Invalid Day Number");
	                			break;
	                	}
	                } 

	                else if (totalDaysInt > 4) {
	                	switch (currentDayInt) {
	                		case 0:
	                		case 1:
	                			this.fourDayEarnText_1Comp.text = simManComp.getDayScore().ToString("0.00");
	                			this.fourDayEarnText_2Comp.text = PLACE_HOLDER;
	                			this.fourDayEarnText_3Comp.text = PLACE_HOLDER;
	                			this.fourDayEarnText_4Comp.text = PLACE_HOLDER;
	                			break;
	                		case 2:
	                			this.fourDayEarnText_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
	                			this.fourDayEarnText_2Comp.text = simManComp.getDayScore().ToString("0.00");
	                			this.fourDayEarnText_3Comp.text = PLACE_HOLDER;
	                			this.fourDayEarnText_4Comp.text = PLACE_HOLDER;
	                			break;
	                		case 3:
	                			this.fourDayEarnText_1Comp.text = simManComp.earningsByDay[0].ToString("0.00");
	                			this.fourDayEarnText_2Comp.text = simManComp.earningsByDay[1].ToString("0.00");
	                			this.fourDayEarnText_3Comp.text = simManComp.getDayScore().ToString("0.00");
	                			this.fourDayEarnText_4Comp.text = PLACE_HOLDER;
	                			break;
	                		default:
	                			this.fourDayEarnTitle_1Comp.text = "Day " + (currentDayInt-4).ToString();
	                			this.fourDayEarnTitle_2Comp.text = "Day " + (currentDayInt-3).ToString();
	                			this.fourDayEarnTitle_3Comp.text = "Day " + (currentDayInt-2).ToString();
	                			this.fourDayEarnTitle_4Comp.text = "Day " + currentDayStr;
								this.fourDayEarnText_1Comp.text = simManComp.earningsByDay[currentDayInt-4].ToString("0.00");
	                			this.fourDayEarnText_2Comp.text = simManComp.earningsByDay[currentDayInt-3].ToString("0.00");
	                			this.fourDayEarnText_3Comp.text = simManComp.earningsByDay[currentDayInt-2].ToString("0.00");
	                			this.fourDayEarnText_4Comp.text = simManComp.getDayScore().ToString("0.00");
	                			break;
	                	}
	                }
            	} 

            	catch (System.Exception e) {
            		Debug.Log("** MULTIDAY UI EXCEPTION **");
            	}

                elapsed = 0.0f;
            }
        }

        else if (!displayedComplete) {
            displayedComplete = true;
            dayTextComp.text = "Day: " + totalDaysStr + " / " + totalDaysStr;
            timeRemComp.text = "Complete!";
        }
	}
}
