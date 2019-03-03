using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Valve.VR;

public class SimManager : MonoBehaviour {

    private const string OUTPUT_DIR     = "C:/Users/CS4ZP6 user/Documents/sim_output/";         // Data persistence - files will be in this dir with a standard name + timestamp
    private const string CONFIG_PATH    = "C:/Users/CS4ZP6 user/Documents/sim_config.txt";   

    private const bool usingConfigFile      = false;     // Toggles the usage of config files - if false, uses defaults in ConfigParser.cs
    private const float TRANSITION_TIME     = 10.0f;     // Duration (seconds) of the transition state 
    private const float DAY_ZERO_REQ_SCORE  = 15.0f;     // Score needed to 'pass' day zero
    private const float COUNTDOWN_THRESHOLD = 10.0f;     // Start countdown sound effects with this many seconds left
    private const float CRITICAL_COUNTDOWN  = 5.1f;      // The last x seconds of countdown will have a different tone
    private const float PERSIST_RATE        = 1.0f;      // Persist to csv or database every this many seconds

    public float maximumShakeOffset;                     // Physical shake impairment strength relative to this value
    
    // State management
    public enum GameState
    {
        PAUSED,         // Still allows for physical movement
        RUNNING,
        TRANSITION,     // Countdown state between days 
        COMPLETE,       // All days successfully completed - simulation is over
        ERROR
    }

    private float currentPayload, currentScore, currentCumulativePayment, elapsedDayTime, elapsedTotalTime, currentDayDuration, nextDayDuration;
    private int currentDay, totalDays;
    private float persistTime = 0.0f;
    private bool paymentEnabled = false;                // Used with the destination limiter. Only pay the user if they're standing close enough

    // For countdown sound effects
    private bool  countdownStarted;
    private float countDownElapsed;             // Starts from 0, counts up with delta time
    private float countDownRelativeThreshold;   // starts from 1.0, goes up in 1 second increments until threshold - 1    

    // Key scene objects
    public GameObject audioManager;
    public GameObject flowManager;          // Manages tap flow 
    public GameObject virtualCamera;        // [CameraRig] object - position relative to Unity Units
    public GameObject physicalCamera;       // Child object of [CameraRig]
    public GameObject pauseOverlay;
    public GameObject transitionOverlay;
    public GameObject pillManager;
    public GameObject curtainLeft;          // To reduce nausea caused by looking out the window
    public GameObject curtainRight;         

    // For hand shake impairment
    public GameObject leftHandVirtual, rightHandVirtual;
    private HandTracker leftHandTracker, rightHandTracker;
    // public GameObject viveControllerLeft, viveControllerRight;

    // Cached components
    private SimPersister simPersister;              // Outputs key data on specified intervals to csv and/or database
    private AudioManager audioManagerComponent;     // Plays all sound effects
    private ConfigParser configParser;              // Parses the configuration file and holds all required simulation parameters
    private PillManager pillManagerComponent;       // Manages treatment + related information displays
    private FlowManager flowManagerComponent;       // Starts/stops tap flow, and cleans scene (erases all water) when necessary

    // Dynamic day-by-day elements
    private GameState currentGameState;
    private DayConfiguration currentDayConfig;
    private Impairment [] currentDayImpairments;
    private Treatment currentDayTreatment;


	/* 
    * Initialization method
    * Runs once on startup
    */
	void Start () {

        // Cache necessary components
        this.audioManagerComponent  = audioManager.GetComponent<AudioManager>();
        this.leftHandTracker        = leftHandVirtual.GetComponent<HandTracker>();
        this.rightHandTracker       = rightHandVirtual.GetComponent<HandTracker>();
        this.flowManagerComponent   = flowManager.GetComponent<FlowManager>();
        this.pillManagerComponent   = pillManager.GetComponent<PillManager>();

        // Prepare for the first day
        resetCountdown();

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

                // Set up environment parameters
                if (this.configParser.lowNauseaModeEnabled()) {
                    this.curtainRight.SetActive(true);
                    this.curtainLeft.SetActive(true);
                } else {
                    this.curtainRight.SetActive(false);
                    this.curtainLeft.SetActive(false);
                }

                currentDay                  = 0;                    // Training/tutorial day
                currentScore                = 0.0f;                 // Holds across all days except 0, except when paying for treatment or penalized
                currentCumulativePayment    = 0.0f;                 // Holds across all days except 0
                elapsedDayTime              = 0.0f;               
                elapsedTotalTime            = 0.0f;                 // Don't ever reset this
                currentGameState            = GameState.RUNNING;
                pillManagerComponent.disablePanels();       // There is no treatment/impairment on day 0
            }
        }
    }
	

    /*
    * Creates the parser object to read in the configuration
    * file for this simulation. 
    * Returns true on success of all parameters being set, false on any error.
    */
    private bool establishSimulationParameters () {
       
        if (usingConfigFile) {
            Debug.Log ("Using custom parameters: " + CONFIG_PATH);
            this.configParser = new ConfigParser(CONFIG_PATH);
            return !(this.configParser.getConfigs() == null || this.configParser.getConfigs().Length == 0); 
        }

        else {
            Debug.Log ("Using default parameters.");
            this.configParser = new ConfigParser();
            return !(this.configParser.getConfigs() == null || this.configParser.getConfigs().Length == 0);
        }
    }


    /*
    * Toggles whether or not payment should be allowed
    * for whatever reason.
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
            this.currentScore += 1.0f; this.currentCumulativePayment += 1.0f;
        }
    }


    /* 
    * Custom payment - this can be used if a reward function multiplier
    * is being applied for the current day. Since the default payout
    * is one unit, the multiplier can be passed in as the custom amount.
    */
    public void payReward (float customAmount) {
        if (paymentEnabled && currentGameState == GameState.RUNNING) {
            this.currentScore += customAmount; this.currentCumulativePayment += customAmount;
        }
    }


    /*
    * General use getters
    */
    public float getCurrentTreatmentCost () {
        return (currentDayTreatment == null || currentDayTreatment.hasBeenObtained()) ? -1.0f : currentDayTreatment.currentCost(elapsedDayTime);
    }

    public float getCurrentTreatmentWaitTime () {
        return (currentDayTreatment == null || currentDayTreatment.hasBeenObtained()) ? -1.0f : currentDayTreatment.currentWaitTime(elapsedDayTime);
    }

    public float getTotalPaymentReceived () {       // Cumulative amount earned - not affected by deductions through penalizing or payment for treatment
        return currentCumulativePayment;
    }

    public float getElapsedDayTime () {
        return elapsedDayTime;
    }

    public float getRemainingDayTime () {
        return currentGameState == GameState.TRANSITION ? nextDayDuration : (currentDayDuration - elapsedDayTime);
    }

    public float getRemainingTransitionTime () {
        return TRANSITION_TIME - elapsedDayTime;
    }

    public int getCurrentDay () {
        return currentDay;
    }

    public int getTotalDays () {
        return totalDays;
    }

    public float getCurrentScore () {        // Not cumulative! This amount will decrease because of penalizing or payment for treatments
        return currentScore;
    }

    public bool isComplete () {
        return this.currentGameState == GameState.COMPLETE;
    }

    public GameState currentState () {
        return currentGameState;
    }

    public DayConfiguration getCurrentDayConfiguration () {
        return this.currentDayConfig;
    } 

    
    /* 
    * Hard reset to some value for current amount of water in the bucket.
    */
    public void setCurrentWaterCarry (int numDrops) {
        this.currentPayload = numDrops;
    }


    /*
    * When a new drop enters the bucket, we need to keep track of it so
    * that we always know how much they are carrying.
    */
    public void increasePayload (int amt) {
        this.currentPayload += amt;
    }


    /*
    * For the speed penalty impairment (and perhaps others) - 
    * depreciates the value of the container content by some amount.
    */
    public void decreasePayload (int amt)
    {
        // Prevent negativity
        this.currentPayload = System.Math.Max(this.currentPayload - amt, 0);
    }


    /*
    * Resetting countdown parameters is necessary
    * for the start of every new day, as well as on
    * simulation startup. Countdown happens in the last
    * few seconds of a given day.
    */
    private void resetCountdown () {
        countdownStarted = false;
        countDownElapsed = 0.0f;
        countDownRelativeThreshold = 1.0f;
    }


    /*
    * If the participant meets the criteria to receive treatment, then
    * make the necessary changes to the simulation environment to 
    * reflect the treatment being taken. This depends on the type of 
    * treatment, i.e. pay or wait, as well as how effective the treatment
    * is, etc.
    */
    public void determinePostTreatmentActions (PillManager.TreatmentObtainType obtainType, float effectiveCost, float effectiveWaitTime) {

        currentDayTreatment.obtain(elapsedDayTime);
        bool isEffective = currentDayTreatment.isEffective();
        float effectiveness = currentDayTreatment.getEffectiveness();
        
        if (obtainType == PillManager.TreatmentObtainType.PAY) {

            if (isEffective) {
                audioManagerComponent.playSound(AudioManager.SoundType.TAKE_MEDICINE);
                modifyImpairmentFactors(effectiveness);
            } 

            else {
                // TODO - maybe should have another sound effect to let them know it didnt work
                // or something else
                Debug.Log (
                    "Treatment uneffective."
                );
            }
        }

        else if (obtainType == PillManager.TreatmentObtainType.WAIT) {
            int a = 1; // TODO
        }
    }


    /*
    * Modifies strength of impairments on the current day (i.e.
    * after paying / waiting for treatment has been completed)
    */
    private void modifyImpairmentFactors (float factor) {

        if (factor == 1.0f) { 
            unapplyImpairments(); 
        }

        else {
            foreach (Impairment i in this.currentDayImpairments) {
                switch (i.getType()) {
                    case Impairment.ImpairmentType.PHYSICAL_SHAKE:
                        rightHandTracker.modifyStrength(factor);
                        leftHandTracker.modifyStrength(factor);
                        break;
                    default:
                        break;
                }
            }
        }
    }


    /*
    * Entirely removes all active impairments for the current day
    */
    private void unapplyImpairments () {
        foreach (Impairment i in this.currentDayImpairments) {
            switch (i.getType()) {
                case Impairment.ImpairmentType.PHYSICAL_SHAKE:
                    rightHandTracker.clearImpairment();
                    leftHandTracker.clearImpairment();
                    break;
                default:
                    break;
            }
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

            // Pausing with thumb buttons
            if (SteamVR_Input._default.inActions.Teleport.GetStateDown(SteamVR_Input_Sources.Any))
            {
                if (currentGameState == GameState.PAUSED)
                {
                    currentGameState = GameState.RUNNING;
                    pauseOverlay.SetActive(false);
                    flowManagerComponent.startFlow();
                }
                else
                {
                    currentGameState = GameState.PAUSED;
                    pauseOverlay.SetActive(true);
                    flowManagerComponent.stopFlow();
                }
            }

            // Persist every X second(s)
            /*
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
                    physicalCamera.transform.position.z, // should also have the quaternion angles here to see where they're looking
                    0.0f,   // TODO
                    0.0f    // TODO
                );
                persistTime = 0.0f;


            }*/
        }


        /*
        * State Management
        *
        */
        if (currentGameState == GameState.COMPLETE) {

            // TODO - anything else needed here?

            int a = 1;
        }


        else if (currentGameState == GameState.ERROR || this.configParser.getConfigs() == null) {

            // TODO - should put a red haze into the headset or 
            // something with the error message in the middle

            int a = 1;
        } 


        else if (currentGameState == GameState.PAUSED) {

            // TODO - anything else needed here?

            int a = 1;
        } 


        else if (currentGameState == GameState.TRANSITION) {
            
            elapsedDayTime += Time.deltaTime;

            if (elapsedDayTime > TRANSITION_TIME) {

                currentDay += 1;
                Debug.Log ("New day: " + currentDay);

                // Set up the next day here
                if (currentDay <= this.configParser.numDays()) {

                    // Establish key parameters from the day configuration object
                    currentDayConfig = configParser.getConfigs()[currentDay - 1];
                    currentDayDuration = currentDayConfig.getDuration();

                    if (currentDay != this.configParser.numDays()) {
                        nextDayDuration = configParser.getConfigs()[currentDay].getDuration();
                    } else {
                        nextDayDuration = 0.0f;
                    }

                    // Call out to necessary scripts to apply impairments for the current day (if any)
                    if ((currentDayImpairments = currentDayConfig.getImpairments()) != null && currentDayImpairments.Length > 0) {
                        foreach (Impairment imp in currentDayImpairments) {

                            float str = imp.getStrength();
                            switch (imp.getType()) {
                                case Impairment.ImpairmentType.PHYSICAL_SHAKE:
                                    str *= maximumShakeOffset;
                                    rightHandTracker.applyImpairment(str);
                                    leftHandTracker.applyImpairment(str);
                                    //SteamVR_Controller.Input ((int)viveControllerLeft.index).TriggerHapticPulse(500);
                                    //SteamVR_Controller.Input ((int)viveControllerRight.index).TriggerHapticPulse(500);
                                    break;
                                // TODO ... others
                                default:
                                    Debug.Log("Invalid impairment type");
                                    break;
                            }
                        }
                    }

                    // Set up the treatment station if there should be treatments available
                    if ((currentDayTreatment = currentDayConfig.getTreatment()) != null) {
                        pillManagerComponent.activatePanels();
                    }

                    // Reset simulation parameters and play effects
                    currentGameState = GameState.RUNNING;
                    elapsedDayTime = 0.0f;
                    transitionOverlay.SetActive(false);
                    audioManagerComponent.playSound(AudioManager.SoundType.START_DAY);
                    flowManagerComponent.startFlow();
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

                /*
                countdownStarted;             // Whether or not the first beep has played
                countDownElapsed;             // Starts from 0, counts up with delta time
                countDownRelativeThreshold;   // starts from 1.0, goes up in 1 second increments until threshold - 1
                */

                float remaining = currentDayDuration - elapsedDayTime; 
                if (remaining < COUNTDOWN_THRESHOLD) {

                    countDownElapsed += Time.deltaTime;
                    
                    if (!countdownStarted) {
                        countdownStarted = true;
                        Debug.Log("Starting count down");
                        audioManagerComponent.playSound (
                            (remaining < CRITICAL_COUNTDOWN) ? 
                                AudioManager.SoundType.CRITICAL_TICK : AudioManager.SoundType.NORMAL_TICK
                        );
                    }

                    else if (countDownElapsed >= countDownRelativeThreshold) {
                        
                        countDownRelativeThreshold += 1.0f;
                        Debug.Log("Increasing count down threshold");

                        audioManagerComponent.playSound (
                            (remaining < CRITICAL_COUNTDOWN) ? 
                                AudioManager.SoundType.CRITICAL_TICK : AudioManager.SoundType.NORMAL_TICK
                        );
                    }
                }

                // Time's up for the current day
                if (elapsedDayTime > currentDayDuration) {

                    flowManagerComponent.cleanScene();
                    pillManagerComponent.disablePanels();
                    unapplyImpairments();

                    // Either enter the transition phase before beginning the new day, or
                    // we're all done - play sound effects and set states accordingly.
                    if (currentDay + 1 > totalDays) {
                        currentGameState = GameState.COMPLETE;
                        audioManagerComponent.playSound(AudioManager.SoundType.SIM_COMPLETE);
                        resetCountdown();
                    } 

                    else {
                        audioManagerComponent.playSound(AudioManager.SoundType.DAY_COMPLETE);
                        Debug.Log("Day " + currentDay + " complete with day time: " + elapsedDayTime);
                        currentGameState = GameState.TRANSITION;
                        transitionOverlay.SetActive(true);
                        elapsedDayTime = 0.0f;
                        resetCountdown();
                    }
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
            else if (currentScore >= DAY_ZERO_REQ_SCORE) {
                audioManagerComponent.playSound(AudioManager.SoundType.DAY_COMPLETE);
                Debug.Log ("Day 0 passed.");
                currentGameState = GameState.TRANSITION;
                transitionOverlay.SetActive(true);
                currentScore = 0.0f;
                currentCumulativePayment = 0.0f;
                elapsedDayTime = 0.0f;
                flowManagerComponent.cleanScene();
                pillManagerComponent.disablePanels();
            }
        }
    }
}
