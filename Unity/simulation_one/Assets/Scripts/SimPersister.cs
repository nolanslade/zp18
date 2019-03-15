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

    private SimManager simScriptComp;

    private bool debugMode = true;

    // Logging / Metrics parameters
   	private System.DateTime startTime;
    private string dbConnection = null;
	private string logFileName  = null;

    private string participantName = "";

    private const string LOG_FILE_PREF  = "WATERSIM_log_";
    private const string LOG_FILE_PATT  = "yyyy-MMM-dd_HH-mm-ss";
    private const string HEAD_DATE_PATT = "yyyy-MMM-dd HH:mm";
    private const string LOG_FILE_SUFF  = ".txt";
    private const string TXT_OUTPUT_FMT = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}";

    private System.IO.StreamWriter fileWriter;

    /*
	* Creates a new persister object - writes to DB or log files
    */
    public SimPersister (SimManager simScript, string conn) {

        this.simScriptComp = simScript;

        Debug.Log("Setting persistence file name.");
        this.participantName = ParticipantData.name;
		this.startTime = System.DateTime.Now;    	
    	this.logFileName = LOG_FILE_PREF + participantName + startTime.ToString(LOG_FILE_PATT) + LOG_FILE_SUFF;

    	if (conn != null) {
    		// TODO
    		int a = 1;
    	} else {
    		// TODO - log something here?
    		int a = 1;
    	}

        // Check for file here + make sure valid directory
        if(!System.IO.Directory.Exists(Application.dataPath + "/OutputData")) {
            System.IO.Directory.CreateDirectory(Application.dataPath + "/OutputData");
        }

        System.IO.File.CreateText(Application.dataPath + "/OutputData/" + logFileName).Dispose();
        fileWriter = new System.IO.StreamWriter(Application.dataPath + "/OutputData/" + logFileName, true);
        writeIntroduction ();
    }


    /*
	* Writes the file header with general information
	* about the current simulation setup / parameters
    */
    private void writeIntroduction () {
    	
    	try {

            // General Info
            fileWriter.WriteLine("Participant name: "         + ParticipantData.name);
            fileWriter.WriteLine("Nausea Reduction: "         + ParticipantData.nauseaSensitive.ToString());
            fileWriter.WriteLine("Claustrophobia Reduction: " + ParticipantData.claustrophicSensitive.ToString());
            fileWriter.WriteLine("Experiment Date & Time: "   + this.startTime.ToString(HEAD_DATE_PATT));
            fileWriter.WriteLine("Configuration Type: "       + (this.simScriptComp.getSimConfigName() == "--" ? "default" : "Config file"));
            fileWriter.WriteLine("Configuration File Name: "  + this.simScriptComp.getSimConfigName());
            fileWriter.WriteLine("Application Version: "      + SimManager.APPLICATION_VERSION);
            fileWriter.WriteLine("");

            // CSV Headers
            fileWriter.WriteLine(string.Format(
                TXT_OUTPUT_FMT,
                "Global_Time",
                "Current_Day",
                "Day_Time",
                "Current_State",
                "Headset_X",
                "Headset_Y",
                "Headset_Z",
                "Avg_Moving_Speed_Last_Second",
                "Current_Water_In_Bucket",
                "Total_Score",
                "Day_Score",
                "Time_Waited_Treatment_Day",
                "Amount_Payed_Treatment_Day",
                "Time_Waited_Treatment_Total",
                "Amount_Payed_For_Treatment_Total"
            )); closeStreamWriter(); 
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
        int                     currentlyCarrying,               // Water droplets inside of the container
        /*
        float                   tremorImpairmentFactorInitial,
        float                   tremorImpairmentFactorCurrent,
        bool                    dayHasTreatment,
        */
        float                   timeWaitedForTreatmentDay,
        float                   amountPayedForTreatmentDay,
        float                   timeWaitedForTreatmentTotal,
        float                   amountPayedForTreatmentTotal,
        float                   speed                           // Avg speed over last second

    	//float 					speedPenaltyFactorInitial, 	// 0 if no impairment applied
    	//float 					speedPenaltyFactorCurrent	// This will drop if treatment received
    	// .... 
    	) {
    	
    	try {

            fileWriter = new System.IO.StreamWriter(Application.dataPath + "/OutputData/" + logFileName, true);
            fileWriter.WriteLine (
                string.Format (
                    TXT_OUTPUT_FMT,
                    globalTime.ToString(),
                    currentDay.ToString(),
                    dayTime.ToString(),
                    currentState.ToString(),
                    headsetX.ToString(),
                    headsetY.ToString(),
                    headsetZ.ToString(),
                    speed.ToString(),
                    currentlyCarrying.ToString(),
                    totalScore.ToString(),
                    dayScore.ToString(),
                    timeWaitedForTreatmentDay.ToString(),
                    amountPayedForTreatmentDay.ToString(),
                    timeWaitedForTreatmentTotal.ToString(),
                    amountPayedForTreatmentTotal.ToString()
                )
            ); closeStreamWriter();
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

    public void closeStreamWriter () {
        try {
    		fileWriter.Close();
    	} 

    	catch (System.Exception e) {
    		Debug.Log ("StreamWriter exception: " + e.Message + "\n" + e.StackTrace);
    	}
    }


}
