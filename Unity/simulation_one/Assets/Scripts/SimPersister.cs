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

    // Logging / Metrics parameters
    private string dbConnection = null;
	private string logFileName  = null;

    private const string LOG_FILE_PREF  = "VR1_log_";
    private const string LOG_FILE_PATT  = "yyyy-MMM-dd_HH-mm-ss";
    private const string LOG_FILE_SUFF  = ".txt";
    private const string DELIMITER 		= ",";

    /*
	* Creates a new persister object - writes to DB or log files
    */
    public SimPersister (string conn) {
    	
    	this.logFileName = LOG_FILE_PREF + System.DateTime.Now.ToString(LOG_FILE_PATT) + LOG_FILE_SUFF;

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
    public void persist () {
    	
    	try {
    		int a = 1; // TODO
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
