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

    // Simulation parameters
    private DayConfiguration [] dayConfigs;


    public ConfigParser (string path) {
        this.configFilePath = path;   
    }


    /*
	* Parse the file and create objects for each parsed item
    */
    public bool parseConfig () {
        
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
}