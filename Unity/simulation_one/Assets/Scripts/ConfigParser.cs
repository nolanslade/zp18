using System;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Dec 22 2018
 * 
 * Configuration file parser. Config
 * file is indent-based, with keywords,
 * values, and attributes. This parsing
 * is done at execution-time.
 * 
 * If parsing is successful, then the SimManager
 * can access the needed parameters here, as well.
 */
public class ConfigParser {

    // Parsing
    private string configFilePath;
    private string dbConnection;

    // Simulation parameters
    private DayConfiguration [] dayConfigs = null;
    
    /*
    * If not using a config file - use these default values (for testing)
    *
    * Structure (3 days):
    * - Day one: no impairment, 1 minute, 2* multiplier
    * - Day two: speed penalty (pay 50 / wait 15s), 2 minutes
    * - Day three: speed penalty, fog (fog @ 80%, pay 100 / wait 25s), 2 minutes
    */
    public ConfigParser () {

        // Metrics
        // TODO ... db stuff here
        dbConnection = null;

        // Day One
        Impairment [] dayOneImps = new Impairment [0];
        Treatment dayOneTreatment = null;   // TODO
        float dayOneDur = 30.0f;
        float dayOneMult = 2.0f;
        int dayOne = 1;

        // Day Two
        float dayTwoDur = 120.0f;
        Impairment [] dayTwoImps = new Impairment [1];
        Treatment dayTwoTreatment = new Treatment (
            100.0f,
            0.15f,
            2.0f,
            25.0f,
            1.0f,
            1.0f,
            0.0f,
            0.0f,
            0.0f
        );   // TODO
        dayTwoImps [0] = new Impairment (Impairment.ImpairmentType.PHYSICAL_SPEED_PENALTY, 0.5f);
        int dayTwo = 2;

        // Day Three
        float dayThreeDur = 120.0f;
        Impairment [] dayThreeImps = new Impairment [2];
        Treatment dayThreeTreatment = new Treatment (
            100.0f,
            0.15f,
            3.0f,
            30.0f,
            1.0f,
            1.0f,
            0.0f,
            0.0f,
            0.0f
        );
        dayThreeImps[0] = new Impairment (Impairment.ImpairmentType.PHYSICAL_SPEED_PENALTY, 0.5f);
        dayThreeImps[1] = new Impairment (Impairment.ImpairmentType.VISUAL_FOG, 0.8f);
        int dayThree = 3;


        // Set each day configuration with the above
        this.dayConfigs = new DayConfiguration [3];
        this.dayConfigs [0] = new DayConfiguration (dayOne, dayOneDur, dayOneImps, dayOneTreatment, dayOneMult);
        this.dayConfigs [1] = new DayConfiguration (dayTwo, dayTwoDur, dayTwoImps, dayTwoTreatment);
        this.dayConfigs [2] = new DayConfiguration (dayThree, dayThreeDur, dayThreeImps, dayThreeTreatment);
    }


    /*
    * If using a configuration file, then load in day configurations
    * by parsing in the file if the path argument is valid.
    */
    public ConfigParser (string path) {
        
        this.configFilePath = path;  
        dbConnection = null;

        // TODO - parse the file (if it exists) and load in day configurations
    }


    /*
	* Parse the file and create objects for each parsed item
    *
    * *** TODO ***
    *
    */
    private bool parseConfig () {
        
        try {
        	return true;
        } 

        catch (UnauthorizedAccessException uae) {
        	return false;
        }

        catch (System.IO.DirectoryNotFoundException dnfe) {
            return false;
        }

        catch (System.IO.IOException ioe) {
        	return false;
        }

        catch (Exception e) {
        	return false;
        }

        return false;
    }


    /*
    * Returns all configurations; either parsed from file or default
    */
    public DayConfiguration [] getConfigs () {
        return this.dayConfigs;
    }

    public string dbConn () {
        return this.dbConnection;
    }

    public int numDays () {
        if (this.dayConfigs != null) {
            return this.dayConfigs.Length;
        } else {
            return -1;
        }
    }
}