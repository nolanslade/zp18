using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Jan 18 2019
 * 
 * Persists simulation data to log files and, if enabled,
 * a SQL database. Comma separated, timestamped log files.
 */
public class SimPersister {

    private bool debugMode = true;

    // Logging / Metrics parameters
   	private System.DateTime startTime;
    private string dbConnection = null;
	private string logFileName  = null;

    private string participantName = "";

    private const string LOG_FILE_PREF  = "WATERSIM_log_";
    private const string LOG_FILE_PATT  = "yyyy-MMM-dd_HH-mm-ss";
    private const string LOG_FILE_SUFF  = ".txt";

    /*
	* Creates a new persister object - writes to DB or log files
    */
    public SimPersister (string conn) {

        this.participantName = ParticipantData.name;
		this.startTime = System.DateTime.Now;    	
    	this.logFileName = LOG_FILE_PREF + startTime.ToString(LOG_FILE_PATT) + LOG_FILE_SUFF;

    	if (conn != null) {
    		// TODO
    		int a = 1;
    	} else {
    		// TODO - log something here?
    		int a = 1;
    	}

    	// TODO check for file here + make sure valid directory
    	// .....
    	writeIntroduction ();
    }


    /*
	* Writes the file header with general information
	* about the current simulation setup / parameters
    */
    private void writeIntroduction () {
    	
    	try {
    		int a = 1; // TODO
    	} 

    	catch (System.Exception e) {
    		Debug.Log ("Intro persistance exception: " + e.Message + "\n" + e.StackTrace);
    	}
    }


    /*
	* General CSV entry into log file
    */
    public void persist (

    	float 					globalTime,				         // Total simulation runtime (any state)
    	int 					currentDay,				
    	SimManager.GameState 	currentState,
        float                   headsetX,
        float                   headsetY,
        float                   headsetZ,
    	float 					dayTime, 				         // Total day time (running state only)
    	float 					totalScore,                      // Includes deductions for payment
    	float 					dayScore,                        // Includes deductions for payment
    	/*
        int                     currentlyCarrying,               // Water droplets inside of the container
        float                   tremorImpairmentFactorInitial,
        float                   tremorImpairmentFactorCurrent,
        bool                    dayHasTreatment,
        */
        float                   timeWaitedForTreatmentDay,
        float                   amountPayedForTreatmentDay,
        float                   timeWaitedForTreatmentTotal,
        float                   amountPayedForTreatmentTotal

    	//float 					speedPenaltyFactorInitial, 	// 0 if no impairment applied
    	//float 					speedPenaltyFactorCurrent	// This will drop if treatment received
    	// .... 
    	) {

        string s = "";
    	//const string fmt = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}"; // TODO 
        const string fmt = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}";
    	
    	try {
    		s = string.Format (
    			fmt,
    			globalTime.ToString(),
                currentDay.ToString(),
                currentState.ToString(),
                headsetX.ToString(),
                headsetY.ToString(),
                headsetZ.ToString(),
                dayTime.ToString(),
                totalScore.ToString(),
                dayScore.ToString(),
                timeWaitedForTreatmentDay.ToString(),
                amountPayedForTreatmentDay.ToString(),
                timeWaitedForTreatmentTotal.ToString(),
                amountPayedForTreatmentTotal.ToString()
    		);

            if (debugMode)
                Debug.Log(s);
    	} 

    	catch (System.Exception e) {
    		Debug.Log ("Persistance exception: " + e.Message + "\n" + e.StackTrace);
    	}
    }


    /*
    * Copy the log file CSV contents into the DB
    */
    public void persistToDB () {
    	
    	try {
    		int a = 1; // TODO
    	} 

    	catch (System.Exception e) {
    		Debug.Log ("DB Persistance exception: " + e.Message + "\n" + e.StackTrace);
    	}
    }
}
