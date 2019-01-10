using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimManager : MonoBehaviour {

    private bool usingConfigFile = false;       // Toggles the usage of config files - if false, uses defaults in ConfigParser.cs
    private const string CONFIG_PATH = "";

    private 
    
    // State management
    public enum GameState
    {
        PAUSED,     // Still allows for physical movement
        RUNNING,
        ERROR
    }

    private GameState currentGameState;
    private int currentScore;
    private int currentDay, totalDays;
    private float elapsedDayTime;

    // Parses the configuration file and holds all required simulation parameters
    private ConfigParser configParser;    

    // Key scene objects
    public GameObject flowManager;          // Manages tap flow 
    public GameObject virtualCamera;        // [CameraRig] object - position relative to Unity Units
    public GameObject physicalCamera;       // Child object of [CameraRig]

	// Run once  on initialization
	void Start () {

        if (!establishSimulationParameters()) {
            currentGameState = GameState.ERROR;
            Debug.log ("Startup error: invalid parameters.");
        }

        else {

            totalDays = this.configParser.numDays();
            
            if (totalDays == -1) {
                currentGameState = GameState.ERROR;
                Debug.log ("Startup error: days invalid.");
            } 

            else {

                currentDay = 0;  // Intro / tutorial / instruction day?

                // TODO

                currentScore = 0;
                currentGameState = GameState.RUNNING;
                flowManager.GetComponent<FlowManager>().startFlow();
            }
        }
    }
	

	// Update is called once per frame
	void Update () {

        if (currentGameState == GameState.ERROR) {
            // TODO - should put a red haze into the headset or 
            // something with the error message in the middle
            int a = 1;
        } 

        else if (currentGameState == GameState.PAUSED) {
            // TODO - should overlay "Paused" in the headset or something
            int a = 1;
        } 

        else {
            int a = 1; 
        }
	}


    /*
    * Creates the parser object to read in the configuration
    * file for this simulation. 
    * Returns true on success of all parameters being set, false on any error.
    */
    private bool establishSimulationParameters () {
       
        // Custom configuration
        if (usingConfigFile) {
            return false; // TODO
        }

        // Use default (test) simulation parameters
        else {
            this.configParser = new ConfigParser ();
            return !(this.configParser.getConfigs == null || this.configParser.getConfigs().Length == 0);
        }
    }


    public GameState currentState () {
        return currentGameState;
    }


    public void payReward () {
        this.currentScore++;
        Debug.Log ("Reward payed (1). New Score: " + currentScore);
    }


    public void payReward (int customAmount) {
        this.currentScore += customAmount;
        Debug.Log("Reward payed ("+ customAmount + "). New Score: " + currentScore);
    }
}
