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
    private const string TXT_OUTPUT_FMT = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39}";

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
        if (!System.IO.Directory.Exists(Application.dataPath + "/OutputData")) {
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
                "Global_Time_Seconds",
                "Current_Day",
                "Day_Time_Seconds",
                "Current_State",
                "Head_X_Position_Meters",
                "Head_Y_Position_Meters",
                "Head_Z_Position_Meters",
                "Head_X_Rotation_Degrees",
                "Head_Y_Rotation_Degrees",
                "Head_Z_Rotation_Degrees",
                "L_Hand_X_Position_Meters",
                "L_Hand_Y_Position_Meters",
                "L_Hand_Z_Position_Meters",
                "R_Hand_X_Position_Meters",
                "R_Hand_Y_Position_Meters",
                "R_Hand_Z_Position_Meters",
                "Bucket_X_Position_Meters",
                "Bucket_Y_Position_Meters",
                "Bucket_Z_Position_Meters",
                "Avg_Moving_Speed_Last_Second_M/sec",
                "Current_Water_In_Bucket",
                "Cumulative_Water_Carried",
                "Today_Water_Carried",
                "Cumulative_Water_Spilled",
                "Today_Water_Spilled",
                "Cumulative_Water_Delivered",
                "Today_Water_Delivered",
                "Total_Score",
                "Today_Score",
                "Current_Pay_Per_Drop",
                "Current_Day_Has_Pay_Treatment",    // Bool
                "Current_Day_Has_Wait_Treatment",   // Bool
                "Current_Treatment_Pay_Cost",       // Dollars
                "Current_Treatment_Wait_Cost",      // Seconds
                "Shake_Impairment_Day_Current_Strength_Pcnt",
                "Shake_Impairment_Day_Initial_Strength_Pcnt",
                "Seconds_Waited_Treatment_Day",
                "Amount_Payed_Treatment_Day",
                "Seconds_Waited_Treatment_Total",
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
        int                     headsetXRotate,
        int                     headsetYRotate,
        int                     headsetZRotate,
        float                   controllerLX,
        float                   controllerLY,
        float                   controllerLZ,
        float                   controllerRX,
        float                   controllerRY,
        float                   controllerRZ,
        float                   bucketX,
        float                   bucketY,
        float                   bucketZ,
        float 					dayTime, 				         // Total day time (running state only)
    	float 					totalScore,                      // Includes deductions for payment
    	float 					dayScore,                        // Includes deductions for payment
        float                   payRate,                         // How much 1 droplet of water is worth today
        int                     currentlyCarrying,               // Water droplets inside of the container
        int                     cumulativeCarrying,              // Amount of water carried total
        int                     dailyCumulativeCarrying,         // Total amount of water carried on this day
        int                     cumulativeSpilled,               // Total amount of water spilled
        int                     dailyCumulativeSpilled,          // Total amount of water spilled on this day
        int                     cumulativeDelivered,             // All drops that have reached the destination 
        int                     todayDelivered,                  // Above, except for today
        bool                    currentDayOffersPayTreatment,
        bool                    currentDayOffersWaitTreatment,
        float                   currentTreatmentPayCost,            // Dollars
        float                   currentTreatmentWaitCost,           // Seconds
        float                   tremorImpairmentCurrentStrength,
        float                   tremorImpairmentInitialStrength,
        float                   timeWaitedForTreatmentDay,
        float                   amountPayedForTreatmentDay,
        float                   timeWaitedForTreatmentTotal,
        float                   amountPayedForTreatmentTotal,
        float                   speed                           // Avg speed over last second
    	
        ) {
    	
    	try {

            fileWriter = new System.IO.StreamWriter(Application.dataPath + "/OutputData/" + logFileName, true);
            fileWriter.WriteLine(
                string.Format(
                    TXT_OUTPUT_FMT,
                    globalTime.ToString(),
                    currentDay.ToString(),
                    dayTime.ToString(),
                    currentState.ToString(),
                    headsetX.ToString(),
                    headsetY.ToString(),
                    headsetZ.ToString(),
                    headsetXRotate.ToString(),
                    headsetYRotate.ToString(),
                    headsetZRotate.ToString(),
                    controllerLX.ToString(),
                    controllerLY.ToString(),
                    controllerLZ.ToString(),
                    controllerRX.ToString(),
                    controllerRY.ToString(),
                    controllerRZ.ToString(),
                    bucketX.ToString(),
                    bucketY.ToString(),
                    bucketZ.ToString(),
                    speed.ToString(),
                    currentlyCarrying.ToString(),
                    cumulativeCarrying.ToString(),
                    dailyCumulativeCarrying.ToString(),
                    cumulativeSpilled.ToString(),
                    dailyCumulativeSpilled.ToString(),
                    cumulativeDelivered.ToString(),
                    todayDelivered.ToString(),
                    totalScore.ToString(),
                    dayScore.ToString(),
                    payRate.ToString(),
                    currentDayOffersPayTreatment.ToString(),
                    currentDayOffersWaitTreatment.ToString(),
                    currentTreatmentPayCost.ToString(),
                    currentTreatmentWaitCost.ToString(),
                    tremorImpairmentCurrentStrength.ToString(),
                    tremorImpairmentInitialStrength.ToString(),
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
