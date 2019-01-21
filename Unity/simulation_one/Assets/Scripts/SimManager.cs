using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimManager : MonoBehaviour {

    private bool usingConfigFile = false;           // Toggles the usage of config files - if false, uses defaults in ConfigParser.cs
    private const string CONFIG_PATH = "";    
    private const float TRANSITION_TIME = 10.0f;    // Duration (seconds) of the transition state 
    private const float DAY_ZERO_REQ_SCORE = 15.0f; // Score needed to 'pass' day zero
    
    // State management
    public enum GameState
    {
        PAUSED,         // Still allows for physical movement
        RUNNING,
        TRANSITION,     // Countdown state between days 
        COMPLETE,       // All days successfully completed - simulation is over
        ERROR
    }

    private GameState currentGameState;
    private float currentScore;
    private int currentDay, totalDays;
    private float elapsedDayTime, elapsedTotalTime;
    private bool paymentEnabled = false;

    // Parses the configuration file and holds all required simulation parameters
    private ConfigParser configParser;    

    // Data (metrics) Persistence
    private SimPersister simPersister;
    private const float PERSIST_RATE = 1.0f;
    private float persistTime = 0.0f;

    // Key scene objects
    public GameObject flowManager;          // Manages tap flow 
    public GameObject virtualCamera;        // [CameraRig] object - position relative to Unity Units
    public GameObject physicalCamera;       // Child object of [CameraRig]

	public GameObject timeRemainingText; //Text to display the time remaining

    public GameState currentState () {
        return currentGameState;
    }

	/* 
    * Initialization method
    * Runs once on startup
    */
	void Start () {

        if (!establishSimulationParameters()) {
            currentGameState = GameState.ERROR;
            Debug.Log ("Startup error: invalid parameters.");
        }

        else {

            simPersister = new SimPersister (this.configParser.dbConn());
            totalDays = this.configParser.numDays();
            Debug.Log ("Starting with total days: " + totalDays);
            
            if (totalDays == -1) {
                currentGameState = GameState.ERROR;
                Debug.Log ("Startup error: days invalid.");
            } 

            else {
                currentDay          = 0;                  // Training/tutorial day
                currentScore        = 0.0f;               // Holds across all days except 0
                elapsedDayTime      = 0.0f;               
                elapsedTotalTime    = 0.0f;               // Don't ever reset this
                currentGameState    = GameState.RUNNING;
            }
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
            return !(this.configParser.getConfigs() == null || this.configParser.getConfigs().Length == 0);
        }
    }


    /*
    * Toggles whether or not payment should be allowed
    * based on where the player is currently standing within
    * the room.
    */
    public void togglePayment (bool val) {
        this.paymentEnabled = val;
    }


    /*
    * Standard reward payment - 1 unit of payment per water droplet
    * colliding with the target drain
    */
    public void payReward () {
        if (paymentEnabled && currentGameState == GameState.RUNNING) {
            this.currentScore += 1.0f;
            Debug.Log ("Reward payed (1). New Score: " + currentScore);
        }
    }


    /* 
    * Custom payment - this can be used if a reward function multiplier
    * is being applied for the current day. Since the default payout
    * is one unit, the multiplier can be passed in as the custom amount.
    */
    public void payReward (float customAmount) {
        if (paymentEnabled && currentGameState == GameState.RUNNING) {
            this.currentScore += customAmount;
            Debug.Log ("Reward payed ("+ customAmount + "). New Score: " + currentScore);
        }
    }


    /*
    * Main loop
    *
    * Update() is called on every frame
    */
    void Update () {

        // Global timestamp tracking and data persistence
        if (currentGameState != GameState.COMPLETE) {
            
            elapsedTotalTime += Time.deltaTime;
            persistTime      += Time.deltaTime;

            // Persist every X second(s)
            if (currentGameState == GameState.RUNNING && persistTime > PERSIST_RATE) {

                /*
                float                   globalTime,             // Total simulation runtime (any state)
                int                     currentDay,             
                SimManager.GameState    currentState,
                float                   dayTime,                // Total day time (running state only)
                float                   totalScore,
                float                   dayScore,
                int                     currentlyCarrying,      // Water droplets inside of the container
                float                   headsetX,
                float                   headsetY,
                float                   headsetZ,
                float                   speedPenaltyFactorInitial,  // 0 if no impairment applied
                float                   speedPenaltyFactorCurrent   // This will drop if treatment received
                // .... 
                */

                simPersister.persist (
                    elapsedTotalTime,
                    currentDay,
                    currentGameState,
                    elapsedDayTime,
                    currentScore,
                    1.0f,   // TODO
                    0,      // TODO
                    physicalCamera.transform.position.x,
                    physicalCamera.transform.position.y,
                    physicalCamera.transform.position.z,
                    0.0f,   // TODO
                    0.0f    // TODO
                );
                persistTime = 0.0f;

				timeRemainingText = GameObject.Find ("TimeRemainingAmount");
				timeRemainingText.GetComponent<UnityEngine.UI.Text> ().text = elapsedDayTime.ToString ();
            }
        }


        /*
        * State Management
        *
        */
        if (currentGameState == GameState.COMPLETE) {
            // TODO - overlay, etc needed to signify that we're all done.
            int a = 1;
        }

        else if (currentGameState == GameState.ERROR || this.configParser.getConfigs() == null) {
            // TODO - should put a red haze into the headset or 
            // something with the error message in the middle
            int a = 1;
        } 

        else if (currentGameState == GameState.PAUSED) {
            // TODO - should overlay "Paused" in the headset or something
            // an idea to trigger a pause - both thumb buttons pushed within
            // half a second of each other?
            int a = 1;
        } 

        else if (currentGameState == GameState.TRANSITION) {
            
            elapsedDayTime += Time.deltaTime;

            if (elapsedDayTime > TRANSITION_TIME) {

                currentDay += 1;
                Debug.Log ("New day: " + currentDay);

                if (currentDay <= this.configParser.numDays()) {
                    currentGameState = GameState.RUNNING;
                    elapsedDayTime = 0.0f;
                } else {
                    Debug.Log ("Simulation complete.");
                    currentGameState = GameState.COMPLETE;
                }
            }
        }

        else {

            /*
            * Standard day state tracking
            *   - Update elapsed time on every frame
            *   - Watch for duration expiry and transition to next day
            *     when needed
            */
            if (currentDay > 0) {

                elapsedDayTime += Time.deltaTime;

                // Time's up for the current day
                if (elapsedDayTime > this.configParser.getConfigs()[currentDay - 1].getDuration()) {
                    Debug.Log ("Day " + currentDay + " complete with day time: " + elapsedDayTime);
                    currentGameState = GameState.TRANSITION;
                    elapsedDayTime = 0.0f;
                }

                else {
                    int a = 1;
                    // TODO
                }
            } 

            /*
            * Intro / Tutorial Day: "Day 0"
            * 
            * Here we can:
            *    - Gauge the user's unimpaired walking speed
            *    - Familiarize the user with the environment
            
            * No timing requirement. The user will 'pass' day 0
            * if they successfully carry some water over to the
            * sink. Then, enter a transition state before the
            * start of day one.
            */
            else {
                if (currentScore > DAY_ZERO_REQ_SCORE) {
                    Debug.Log ("Day 0 passed.");
                    currentGameState = GameState.TRANSITION;
                    currentScore = 0.0f;
                    elapsedDayTime = 0.0f;
                }
            }
        }
    }
}
