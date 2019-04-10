using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Valve.VR;

/*
* Nolan Slade
* November 2018
* 
* Main event object; tracks simulation state, defines key
* parameters, and contains several methods for important
* actions including activating impairments, receiving treatment,
* providing instructions, and state-switching.
* 
* Also defines enums for all tutorial steps, and possible game states.
*/
public class SimManager : MonoBehaviour {

    public static string APPLICATION_VERSION  = "1.8";
    public static float UNITY_VIVE_SCALE      = 18.77f;    // Unity units / this value = metres in the physical world

    public bool persistenceEnabled;

    public float gravityImpairmentMaxDrop;  // Drop gravity by a maximum of this amount @ 100% strength
    public float fogImpairmentMaxAlpha;     // At 100% strength the fog will be this opaque
    public float fogImpairmentMinAlpha;     // At 0% strength the fog will be this opaque

    private const bool usingConfigFile                  = true;      // Toggles the usage of config files - if false, uses defaults in ConfigParser.cs
    private const float TRANSITION_TIME                 = 10.0f;     // Duration (seconds) of the transition state
    private float unimpairedDayZeroThreshold            = ConfigParser.DAY_0_DEFAULT_SCORE;    // Score needed to 'pass' day zero - configured through the file otherwise defaulted to 150 (unimpaired)
    private float impairedDayZeroThreshold              = ConfigParser.DAY_0_DEFAULT_SCORE;    // Score needed to 'pass' day zero - configured through the file otherwise defaulted to 150 (impaired)
    private const float DAY_ZERO_MOV_FREQ               = 0.125f;     // Calculate the participant's moving speed in day 0 on this interval for performance
    private const float COUNTDOWN_THRESHOLD             = 10.0f;     // Start countdown sound effects with this many seconds left
    private const float FILL_BUCKET_TRIGGER_THRESHOLD   = 40.0f;     // The participant needs to fill their bucket past this level to advance in the tutorial
    private const float CRITICAL_COUNTDOWN              = 5.1f;      // The last x seconds of countdown will have a different tone
    private const float PERSIST_RATE                    = 1.0f;      // Persist to csv or database every this many seconds
    private const float PHYSICS_BASE_AMT                = -30.0f;    // Default gravity strength
    private const float CONTAINER_PLATFORM_SPAWN_X      = 0.0f;      // When the bucket is moved to the wait platform for waiting for treatment
    private const float CONTAINER_PLATFORM_SPAWN_Y      = 22.93f;
    private const float CONTAINER_PLATFORM_SPAWN_Z      = -22.05f;

    // All instructions for Day 0 are defined below
    private Instruction locateBucketInstr   = new Instruction ("Objective: locate and walk to the bucket", 8.0f);
    private Instruction holdContainerInstr  = new Instruction ("To pick up, place one hand on the bucket\nand squeeze index finger", 7.0f);
    private Instruction fillInstr           = new Instruction ("Fill up the bucket by placing it\nunder the running water", 6.0f);
    private Instruction goToSinkInstr       = new Instruction ("Carefully turn around and carry\nthe bucket to the opposing sink", 7.0f);
    private Instruction pourBucketInstr     = new Instruction ("Pour the contents of the bucket\ninto the sink to earn money", 6.0f);
    private Instruction continueInstr;

    // State management
    public enum GameState
    {
        PAUSED,         // Still allows for physical movement
        RUNNING,
        TRANSITION,     // Countdown state between days
        COMPLETE,       // All days successfully completed - simulation is over
        ERROR,
        LIMBO           // Essentially the same as paused, but reserving the paused
                        // state just for the actual act of pausing the game
    }

    public enum TutorialStep
    {
        BUCKET,
        HOLD_CONTAINER,
        FILL,
        GO_TO_SINK,
        POUR_BUCKET,
        CONTINUE,
        DONE_TUTORIAL,
        NULL            // Adding for bucket pick-up bug fix. All triggers that use the 0-arg
                        // advanceTutorialStep() method should use this in the editor.
    }

    private float currentScore, dayScore, currentCumulativePayment, elapsedDayTime, elapsedTotalTime, currentDayDuration, 
                    nextDayDuration, timeWaitedForTreatmentDay, timeWaitedForTreatmentTotal, amountPayedForTreatmentDay,
                    amountPayedForTreatmentTotal, avgSpeedLastSecond, currentPayRate;

    // Impairment strength trackers for persistence only
    private float shakeImpStrCurrent = 0.0f;
    private float shakeImpStrInitial = 0.0f;
    private float speedImpStrCurrent = 0.0f;
    private float fogImpStrCurrent   = 0.0f;

    // Limbo functionality
    private Instruction [] limboInstrs;         // Instructions to display during limbo
    private int limboIndex;                     // What instruction we're on
    private float limboElapsed;                 // Since last instruction display

    // Waiting for treatment
    private bool waitingForTreatment = false;
    private float waitingForTreatmentDuration = 0.0f;

    private Vector3 posA, posB;                         // Speed tracking every second using delta distance in scene
    private Vector3 posADay0, posBDay0;                 // Position tracking for Day 0 average speed tracking
    private int currentDay, totalDays, currentPayload, cumulativePayload, dailyCumulativePayload, cumulativeDelivered, 
                    dailyCumulativeDelivered, totalSpilled, todaySpilled;
    private float persistTime = 0.0f;
    private bool paymentEnabled = true;                // Used with the destination limiter. Only pay the user if they're standing close enough

    // For countdown sound effects
    private bool  countdownStarted;
    private float countDownElapsed;             // Starts from 0, counts up with delta time
    private float countDownRelativeThreshold;   // starts from 1.0, goes up in 1 second increments until threshold - 1

    // Key scene objects
    public GameObject containerBase;
    public GameObject audioManager;
    public GameObject flowManager;          // Manages tap flow
    public GameObject virtualCamera;        // [CameraRig] object - position relative to Unity Units
    public GameObject physicalCamera;       // Child object of [CameraRig]
    public GameObject pauseOverlay;
    public GameObject transitionOverlay;
    public GameObject pillManager;
    public GameObject curtainLeft;          // To reduce nausea caused by looking out the window
    public GameObject curtainRight;
    public GameObject instructionManager;
    public GameObject fogImpairmentPanel;
    public GameObject sourceUI;
    public GameObject destUI;
    public GameObject WaterDropletCounter;
    public GameObject claustroAssets;           // Enable these if claustrophobic and nausea-sensitive
    public GameObject disableOnClaustro;        // Disable these if claustrophobic and nausea-sensitive
    public GameObject DayZeroSpeedCounter;      // Used to determine when the participant is carrying water to the sink

    // Instruction Markers and tutorial booleans
    public GameObject bucketMarker;
    public GameObject bucketPickUpTrigger;
    public GameObject bucketPickUpTriggerLower;
    public GameObject farSinkMarker;
    public GameObject farSinkTrigger;

    // Hands, trackers and Hand scripts
    public GameObject leftHandVirtual, rightHandVirtual;
    private HandTracker leftHandTracker, rightHandTracker;
    private Valve.VR.InteractionSystem.Hand leftHandScriptComp, rightHandScriptComp;
    private Valve.VR.InteractionSystem.Hand currentCarryHand = null;

    // Cached components
    private SimPersister simPersister;              // Outputs key data on specified intervals to csv and/or database
    private AudioManager audioManagerComponent;     // Plays all sound effects
    private ConfigParser configParser;              // Parses the configuration file and holds all required simulation parameters
    private PillManager pillManagerComponent;       // Manages treatment + related information displays
    private FlowManager flowManagerComponent;       // Starts/stops tap flow, and cleans scene (erases all water) when necessary
    private InstructionManager instructionManagerComponent;
    private WaterDropletCounter waterDropletCounterComponent;

    // Dynamic day-by-day elements
    private GameState currentGameState;
    private TutorialStep currentTutorialStep;
    private DayConfiguration currentDayConfig;
    private Impairment [] currentDayImpairments;
    private Treatment currentDayTreatment;

    //Variables for tracking speed during Day 0 for speed impairment
    private const float WALK_SPEED_FREQ = 0.2f;
    private float speedPenaltyElapsed   = 0.0f;
    private float avgWalkingSpeedDay0   = 0.0f;
    private float secondsInDay1         = 0.0f;
    private bool speedPenaltyFlag       = false;
    public bool inDay0SpeedCaptureZone  = false;
    private Vector3 day0PosA            = new Vector3 ();
    private Vector3 day0PosB            = new Vector3 ();
    private float dayZeroMovingElapsed  = 0.0f;
    public int dayZeroMovingCount       = 0;            // We only want to track position changes inside the 
                                                        // zone, not the edge cases where have just entered or just left

    /*
    * Initialization method
    * Runs once at scene load
    */
    void Start () {

        // Cache necessary components
        this.audioManagerComponent          = audioManager.GetComponent<AudioManager>();
        this.leftHandTracker                = leftHandVirtual.GetComponent<HandTracker>();
        this.rightHandTracker               = rightHandVirtual.GetComponent<HandTracker>();
        this.flowManagerComponent           = flowManager.GetComponent<FlowManager>();
        this.pillManagerComponent           = pillManager.GetComponent<PillManager>();
        this.instructionManagerComponent    = instructionManager.GetComponent<InstructionManager>();
        this.leftHandScriptComp             = leftHandVirtual.GetComponent<Valve.VR.InteractionSystem.Hand>();
        this.rightHandScriptComp            = rightHandVirtual.GetComponent<Valve.VR.InteractionSystem.Hand>();
        this.waterDropletCounterComponent   = WaterDropletCounter.GetComponent<WaterDropletCounter>();

        // Prepare for the first day
        resetCountdown();

        if (!establishSimulationParameters()) {
            currentGameState = GameState.ERROR;
            Debug.Log ("Startup error: invalid parameters.");
        }

        else {

            Debug.Log("Initializing persister.");
            simPersister = new SimPersister (this, this.configParser.dbConn());
            Debug.Log("Setting days.");
            totalDays = this.configParser.numDays();
            Debug.Log ("Starting with total days: " + totalDays);

            if (totalDays == -1) {
                currentGameState = GameState.ERROR;
                Debug.Log ("Startup error: days invalid.");
            }

            else {

                // Set up environment parameters - disable exterior components
                // in order to improve performance if the curtains are drawn. No
                // point in having trees or grass if they can't see out the window.
                if (this.configParser.lowNauseaModeEnabled()) {

                    if (this.configParser.claustrophobicModeEnabled())
                    {
                        this.claustroAssets.SetActive(true);
                        Destroy(disableOnClaustro);
                    }

                    else
                    {
                        this.curtainRight.SetActive(true);
                        this.curtainLeft.SetActive(true);
                        Destroy(claustroAssets);
                    }

                    foreach (GameObject tree in GameObject.FindGameObjectsWithTag ("Trees")) { Destroy (tree); }
                    foreach (GameObject extTerr in GameObject.FindGameObjectsWithTag ("ExteriorTerrain")) { Destroy (extTerr); }
                }

                else {
                    Destroy(claustroAssets);
                    Destroy(curtainLeft);
                    Destroy(curtainRight);
                }

                totalSpilled                = 0;
                todaySpilled                = 0;
                currentDay                  = 0;                    // Training/tutorial day
                currentPayRate              = 1.0f;                 // Day 0 uses a default rate of $1 per water droplet
                currentPayload              = 0;                    // Amount of droplets in bucket
                cumulativePayload           = 0;                    // Amount of droplets ever carried
                dailyCumulativePayload      = 0;                    // Amount of droplets carried on this day
                cumulativeDelivered         = 0;                    // Amount of droplets ever successfully delivered
                dailyCumulativeDelivered    = 0;                    // Amount of droplets successfully delivered on this day
                currentScore                = 0.0f;                 // Holds across all days except 0, except when paying for treatment or penalized
                dayScore                    = 0.0f;
                currentCumulativePayment    = 0.0f;                 // Holds across all days except 0
                elapsedDayTime              = 0.0f;
                elapsedTotalTime            = 0.0f;                 // Don't ever reset this

                currentGameState            = GameState.RUNNING;
                pillManagerComponent.disablePanels();       // There is no treatment/impairment on day 0
                Debug.Log("Starting up " + currentGameState);
                Debug.Log("Enabling UIs");
                this.destUI.SetActive(true);
                this.sourceUI.SetActive(true);
                currentTutorialStep = TutorialStep.BUCKET;
                bucketMarker.SetActive(true);

                // Updating all instructions to use Instruction objects
                // instructionManagerComponent.setTemporaryMessage("Objective: locate and walk to the bucket", 8.0f);
                instructionManagerComponent.setTemporaryMessage(locateBucketInstr);
            }
        }
    }


    /*
    * Creates the parser object to read in the configuration
    * file for this simulation.
    * Returns true on success of all parameters being set, false on any error.
    */
    private bool establishSimulationParameters () {

        Debug.Log("Establishing params.");

        if (usingConfigFile) {

            string configPath = getSimConfigName();
            Debug.Log ("Using custom parameters: " + configPath);

            this.configParser = new ConfigParser (configPath);

            if (!this.configParser.getSimSound())
            {
                audioManagerComponent.mute();
            }

            if (!this.configParser.getSimInstruction())
            {
                instructionManagerComponent.setInstructionsDisable();
            }

            // Nolan April 2019 - changing to support two different stages in day 0
            float[] dayZeroThresholds = this.configParser.getDayZeroScore();
            unimpairedDayZeroThreshold = dayZeroThresholds[ConfigParser.UNIMPAIRED];
            impairedDayZeroThreshold = dayZeroThresholds[ConfigParser.IMPAIRED];
            Debug.Log("Day 0 unimpaired threshold: " + unimpairedDayZeroThreshold.ToString());
            Debug.Log("Day 0 impaired threshold: " + impairedDayZeroThreshold.ToString());
            return !(this.configParser.getConfigs() == null || this.configParser.getConfigs().Length == 0);
        }

        else {
            Debug.Log ("Using default parameters.");
            this.configParser = new ConfigParser();
            return !(this.configParser.getConfigs() == null || this.configParser.getConfigs().Length == 0);
        }
    }


    /*
    * We'll track most metrics during the tutorial just so we can look
    * into how they're doing. But once the tutorial is done, we need
    * to reset everything to a blank slate for day one.
    */
    private void resetMetricsForDayOne () {
        currentScore                = 0.0f;
        currentCumulativePayment    = 0.0f;
        elapsedDayTime              = 0.0f;
        totalSpilled                = 0;
        todaySpilled                = 0;
        cumulativePayload           = 0;
        dailyCumulativePayload      = 0;
        cumulativeDelivered         = 0;
        dailyCumulativeDelivered    = 0;
    }


    /*
    * Toggles whether or not payment should be allowed
    * for whatever reason.
    */
    public void togglePayment (bool val) {
        this.paymentEnabled = val;
    }


    /*
    * Some drainage objects will increase spill totals if
    * water hits them.
    */
    public void registerSpill () {
        // Not checking for day 0 because we reset the counters
        // at the start of day 1 in update()
        if (currentGameState == GameState.RUNNING) {
            this.totalSpilled++; this.todaySpilled++;
        }
    }


    /*
    * Standard reward payment - 1 unit of payment per water droplet
    * colliding with the target drain
    */
    public void payReward () {
        if (paymentEnabled && currentGameState == GameState.RUNNING) {

            this.currentScore               += currentPayRate; 
            this.currentCumulativePayment   += currentPayRate; 
            this.dayScore                   += currentPayRate; 

            this.dailyCumulativeDelivered   ++; 
            this.cumulativeDelivered        ++;

            if (currentTutorialStep == TutorialStep.POUR_BUCKET) {
                advanceTutorialStep();
            }
        }
    }


    /*
    * General use getters
    */
    public float getCurrentTreatmentCost () {
        return (currentDayTreatment == null || currentDayTreatment.hasBeenObtained()) ? -1.0f : currentDayTreatment.currentCost(elapsedDayTime);
    }

    public float getCurrentTreatmentWaitTime () {
        if (!waitingForTreatment)
            return (currentDayTreatment == null || currentDayTreatment.hasBeenObtained()) ? -1.0f : currentDayTreatment.currentWaitTime(elapsedDayTime);
        else
            return (waitingForTreatmentDuration);
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

    public float getDayScore()
    {
        return dayScore;
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

    public string getSimConfigName () {
        return usingConfigFile ? (Application.dataPath + "/InputData/sim_config.txt") : "--";
    }

    public bool isWaitingForTreatment () {
        return this.waitingForTreatment;
    }


    /*
    * Puts the simulation into a limbo state (like being paused)
    * for a given duration of time in order to display instructions. 
    * For example, when treatment becomes available, we can put
    * the simulation into limbo until all instructions have been
    * displayed. Then, the simulation can resume.
    */
    public void limbo (Instruction [] instructionsToDisplay) {
        Debug.Log("Entering limbo (function-call exit)");
        this.currentGameState = GameState.LIMBO;
        this.limboInstrs = instructionsToDisplay;
        this.limboIndex = 0;        // After the first is displayed in this function for its duration, we'll move onto the second
        this.limboElapsed = 0.0f;
        // Always display the first instruction before going back to update()
        this.instructionManagerComponent.setTemporaryMessage(instructionsToDisplay[0].message, instructionsToDisplay[0].displayDuration);
        if (Array.Exists(this.currentDayImpairments, element => element.getType() == Impairment.ImpairmentType.VISUAL_FOG))
        {
            fogImpairmentPanel.SetActive(false);
        }
    }

    public void exitLimbo () {
        Debug.Log("Exiting limbo state");
        this.currentGameState = GameState.RUNNING;
        if (Array.Exists(this.currentDayImpairments, element => element.getType() == Impairment.ImpairmentType.VISUAL_FOG))
        {
            fogImpairmentPanel.SetActive(true);
        }
    }


    /*
    * Instruction management
    */
    public void advanceTutorialStep () {

        switch (currentTutorialStep) {
            
            case TutorialStep.BUCKET:
                currentTutorialStep = TutorialStep.HOLD_CONTAINER;
                instructionManagerComponent.setTemporaryMessage(holdContainerInstr);
                bucketPickUpTrigger.SetActive(true);
                bucketPickUpTriggerLower.SetActive(true);
                break;
            
            case TutorialStep.HOLD_CONTAINER:
                currentTutorialStep = TutorialStep.FILL;
                instructionManagerComponent.setTemporaryMessage(fillInstr);
                break;
            
            case TutorialStep.FILL:
                currentTutorialStep = TutorialStep.GO_TO_SINK;
                instructionManagerComponent.setTemporaryMessage(goToSinkInstr);
                farSinkMarker.SetActive(true);
                farSinkTrigger.SetActive(true);
                break;
            
            case TutorialStep.GO_TO_SINK:
                currentTutorialStep = TutorialStep.POUR_BUCKET;
                instructionManagerComponent.setTemporaryMessage(pourBucketInstr);
                break;
            
            case TutorialStep.POUR_BUCKET:
                currentTutorialStep = TutorialStep.CONTINUE;
                continueInstr = new Instruction ("To start the experiment, repeat this\nprocess until you've earned $" + unimpairedDayZeroThreshold.ToString("0.00"), 7.0f);
                instructionManagerComponent.setTemporaryMessage(continueInstr);
                break;
            
            case TutorialStep.CONTINUE:
                Debug.Log("All tutorial steps complete.");
                currentTutorialStep = TutorialStep.DONE_TUTORIAL;
                break;
            
            default:
                Debug.Log("Invalid tutorial step.");
                break;
        }
    }


    /*
    * Same as above, except lock to a specific step (bucket pick-up bug fix)
    * Sometimes, where multiple triggers are used, we could advance the step
    * twice by accident.
    */
    public void advanceTutorialStep (TutorialStep stepToAdvanceTo) {
        if (currentTutorialStep != stepToAdvanceTo) {
            advanceTutorialStep();
        }
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
        if (currentTutorialStep == TutorialStep.FILL && currentPayload > FILL_BUCKET_TRIGGER_THRESHOLD) {
            advanceTutorialStep();
        }

        // Nolan mar 17
        // Changing kerala's code here - not making it tutorial
        // dependent, because instructions could be disabled
        // if (currentTutorialStep == TutorialStep.DONE_TUTORIAL) {
            this.cumulativePayload += amt;
            this.dailyCumulativePayload += amt;
        // }
    }


    /*
    * For the speed penalty impairment (and perhaps others) -
    * depreciates the value of the container content by some amount.
    */
    public void decreasePayload (int amt) {
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

        Debug.Log("Determining post treatment actions.");

        currentDayTreatment.obtain(elapsedDayTime);
        bool isEffective = currentDayTreatment.isEffective();
        float effectiveness = currentDayTreatment.getEffectiveness();

        Debug.Log("Obtained, effective: " + isEffective.ToString()  + ", effectiveness: " + effectiveness.ToString());

        if (obtainType == PillManager.TreatmentObtainType.PAY) {

            Debug.Log("Decrementing score (" + currentScore.ToString() + ") by: " + effectiveCost.ToString());
            currentScore -= effectiveCost;
            dayScore -= effectiveCost;
            Debug.Log("New score: " + currentScore.ToString());
            Debug.Log("Increasing amounts payed for treatment, total and day.");
            amountPayedForTreatmentDay += effectiveCost;
            amountPayedForTreatmentTotal += effectiveCost;

            if (isEffective) {
                Debug.Log("Treatment was effective. Modifying impairment.");
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

            Debug.Log("Waiting for treatment started...");
            this.waitingForTreatment = true;
            this.waitingForTreatmentDuration = effectiveWaitTime;
            pillManagerComponent.disableComponentsForWaiting();

            // Detach the bucket from the hand holding it
            currentCarryHand = getHandScriptHoldingObj(this.containerBase);
            if (currentCarryHand != null)
            {
                currentCarryHand.DetachObject(this.containerBase);
            }

            // Move the bucket to the platform and lock it into position (disable the pick up hand scripts)
            Debug.Log("Disabling hands: L=" + leftHandScriptComp.enabled + " R=" + rightHandScriptComp.enabled);
            leftHandScriptComp.enabled = false;
            rightHandScriptComp.enabled = false;
            Debug.Log("Disabling hands: L=" + leftHandScriptComp.enabled + " R=" + rightHandScriptComp.enabled);
            containerBase.transform.position = new Vector3 (
                CONTAINER_PLATFORM_SPAWN_X,
                CONTAINER_PLATFORM_SPAWN_Y,
                CONTAINER_PLATFORM_SPAWN_Z
            ); containerBase.transform.eulerAngles = new Vector3 (0.0f, 0.0f, 0.0f);

            // Limbo, display the instructions for how the waiting process is going to work.
            Debug.Log("Initializing limbo for waiting for treatment...");
            Instruction[] instrs = new Instruction [2];
            Instruction instrOne = new Instruction ("You've decided to wait for treatment.\nUntil the timer reaches 0, you'll be\nunable to pick up the bucket.", 8.0f);
            Instruction instrTwo = new Instruction ("Once the timer reaches 0, you will\nreceive treatment, and may begin\nearning money again, unimpaired.", 8.0f);
            instrs[0] = instrOne; instrs[1] = instrTwo;
            limbo(instrs);
        }
    }


    /*
    * Returns LEFT or RIGHT or NONE, depending on which hand is
    * holding the given object.
    */
    public HandTracker.HandSide getHandSideHoldingObj (GameObject obj) {
        return (leftHandScriptComp.ObjectIsAttached(obj)) ?
            HandTracker.HandSide.LEFT :
            (rightHandScriptComp.ObjectIsAttached(obj) ?
            HandTracker.HandSide.RIGHT :
            HandTracker.HandSide.NONE);
    }


    /*
    * Same as above, except returns the corresponding side's hand script. 
    * Returns null if none holding obj.
    */
    public Valve.VR.InteractionSystem.Hand getHandScriptHoldingObj (GameObject obj) {
        return (leftHandScriptComp.ObjectIsAttached(obj)) ?
            leftHandScriptComp :
            (rightHandScriptComp.ObjectIsAttached(obj) ?
            rightHandScriptComp :
            null);
    }


    /*
    * Follows from the above - but actually carries out the changes, once they've been determined. 
    * Only being used for the wait option right now.
    */
    private void applyPostTreatmentActions ()
    {
        bool isEffective = currentDayTreatment.isEffective();
        float effectiveness = currentDayTreatment.getEffectiveness();

        Debug.Log("Effective: " + isEffective.ToString());
        Debug.Log("Effectiveness: " + effectiveness.ToString());

        if (isEffective)
        {
            Debug.Log("Treatment was effective. Modifying impairment.");
            audioManagerComponent.playSound(AudioManager.SoundType.TAKE_MEDICINE);
            modifyImpairmentFactors(effectiveness);
        }

        else
        {
            // TODO - maybe should have another sound effect to let them know it didnt work
            // or something else
            Debug.Log(
                "Treatment uneffective."
            );
        }
    }


    /*
    * Modifies strength of impairments on the current day (i.e.
    * after paying / waiting for treatment has been completed)
    */
    private void modifyImpairmentFactors (float factor) {

        // If factor = 1.0f, then it means remove 100% of the current
        // impairment strengths.

        if (factor >= 1.0f) {
            unapplyImpairments();
        }

        else {

            // TODO!!! need to update this to actually work properly for non 1.0f factors

            foreach (Impairment i in this.currentDayImpairments) {
                switch (i.getType()) {
                    case Impairment.ImpairmentType.PHYSICAL_SHAKE:
                        rightHandTracker.modifyStrength(factor);
                        leftHandTracker.modifyStrength(factor);
                        shakeImpStrCurrent  = shakeImpStrCurrent - (shakeImpStrCurrent * factor);
                        if (shakeImpStrCurrent < 0.0f) shakeImpStrCurrent = 0.0f;
                        break;
                    case Impairment.ImpairmentType.VISUAL_FOG:
                        Image fogImgComp = fogImpairmentPanel.GetComponent<Image>();
                        fogImpStrCurrent = fogImpStrCurrent - (fogImpStrCurrent * factor);
                        fogImgComp.color = new Color(fogImgComp.color.r, fogImgComp.color.g, fogImgComp.color.b, fogImpStrCurrent);
                        Debug.Log("Modifying visual strength by factor of " + factor.ToString() + " Strength is now " + fogImpStrCurrent);
                        break;
                    case Impairment.ImpairmentType.PHYSICAL_SPEED_PENALTY:
                        speedImpStrCurrent = speedImpStrCurrent - (speedImpStrCurrent * factor);
                        Debug.Log("Modifying speed penalty strength by factor of " + factor.ToString() + " Strength is now " + speedImpStrCurrent);
                        break;
                    case Impairment.ImpairmentType.PHYSICAL_GRAVITY:
                        //Physics.gravity = new Vector3 (0.0f, PHYSICS_BASE_AMT - SOMETHING, 0.0f); ???
                        break;
                    default:
                        Debug.Log("Unable to modify unsupported impairment factor: " + i.getType().ToString());
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
                    shakeImpStrCurrent = 0.0f;
                    break;
                case Impairment.ImpairmentType.PHYSICAL_GRAVITY:
                    Physics.gravity = new Vector3 (0.0f, PHYSICS_BASE_AMT, 0.0f);
                    break;
                case Impairment.ImpairmentType.VISUAL_FOG:
                    fogImpairmentPanel.SetActive(false);
                    fogImpStrCurrent = 0.0f;
                    break;
                case Impairment.ImpairmentType.PHYSICAL_SPEED_PENALTY:
                    speedPenaltyFlag = false;
                    speedImpStrCurrent = 0.0f;
                    break;
                default:
                    Debug.Log("Unable to unapply unsupported impairment: " + i.getType().ToString());
                    break;
            }
        }
    }


    /* Standard distance formula for 3D points */
    private float distanceBetween (Vector3 a, Vector3 b) {
        return (
            (float) System.Math.Sqrt (
                System.Math.Pow(b.x - a.x, 2.0) +
                System.Math.Pow(b.y - a.y, 2.0) +
                System.Math.Pow(b.z - a.z, 2.0)
            )
        );
    }

    /* Same as above, except only measure lateral distance */
    private float lateralDistanceBetween(Vector3 a, Vector3 b)
    {
        return ( 
            (float) System.Math.Sqrt (
                System.Math.Pow(b.x - a.x, 2.0) +
                System.Math.Pow(b.z - a.z, 2.0)
            )
        );
    }

    /*
    * Main loop
    *
    * Update() is called on every frame
    */
    void Update () {


        // Quit Application using Esc Key
        if (Input.GetKey("escape")) {
            Application.Quit();
        }


        // Global timestamp tracking and data persistence
        if (currentGameState != GameState.COMPLETE) {

            elapsedTotalTime += Time.deltaTime;
            persistTime      += Time.deltaTime;


            // Persist every X second(s)
            if (persistenceEnabled && persistTime > PERSIST_RATE && currentGameState != GameState.COMPLETE) {

                /*
                Persistence parameter list (ordered):

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
                */

                // Get user speed over last second (in meters)
                posB = physicalCamera.transform.position;
                avgSpeedLastSecond = lateralDistanceBetween (posA, posB) / UNITY_VIVE_SCALE;

                simPersister.persist (
                    elapsedTotalTime,
                    currentDay,
                    currentGameState,
                    physicalCamera.transform.position.x / UNITY_VIVE_SCALE,
                    physicalCamera.transform.position.y / UNITY_VIVE_SCALE,
                    physicalCamera.transform.position.z / UNITY_VIVE_SCALE,
                    (int) (physicalCamera.transform.rotation.x * 100),
                    (int) (physicalCamera.transform.rotation.y * 100),
                    (int) (physicalCamera.transform.rotation.z * 100),
                    leftHandVirtual.transform.position.x / UNITY_VIVE_SCALE,
                    leftHandVirtual.transform.position.y / UNITY_VIVE_SCALE,
                    leftHandVirtual.transform.position.z / UNITY_VIVE_SCALE,
                    rightHandVirtual.transform.position.x / UNITY_VIVE_SCALE,
                    rightHandVirtual.transform.position.y / UNITY_VIVE_SCALE,
                    rightHandVirtual.transform.position.z / UNITY_VIVE_SCALE,
                    containerBase.transform.position.x / UNITY_VIVE_SCALE,
                    containerBase.transform.position.y / UNITY_VIVE_SCALE,
                    containerBase.transform.position.z / UNITY_VIVE_SCALE,
                    elapsedDayTime,
                    currentScore,       
                    dayScore,   
                    currentPayRate,
                    currentPayload,
                    cumulativePayload,
                    dailyCumulativePayload,
                    totalSpilled,
                    todaySpilled,
                    cumulativeDelivered,
                    dailyCumulativeDelivered,
                    (currentDayTreatment != null ? currentDayTreatment.hasPayOption() : false),
                    (currentDayTreatment != null ? currentDayTreatment.hasWaitOption() : false),
                    ((currentDayTreatment != null && currentDayTreatment.hasPayOption()) ? getCurrentTreatmentCost() : -1.0f),
                    ((currentDayTreatment != null && currentDayTreatment.hasWaitOption()) ? getCurrentTreatmentWaitTime() : -1.0f),
                    shakeImpStrCurrent,
                    shakeImpStrInitial,
                    timeWaitedForTreatmentDay,
                    amountPayedForTreatmentDay,
                    timeWaitedForTreatmentTotal,
                    amountPayedForTreatmentTotal,
                    avgSpeedLastSecond
                );

                persistTime = 0.0f;
                posA = physicalCamera.transform.position;
            }
        }


        /*
        * State Management
        *
        */
        if (currentGameState == GameState.COMPLETE) {

            // TODO - anything else needed here?
            int a = 1;
            simPersister.closeStreamWriter();
        }


        else if (currentGameState == GameState.LIMBO) {

            limboElapsed += Time.deltaTime;

            if (limboElapsed > limboInstrs[limboIndex].displayDuration) {
                limboIndex++;
                limboElapsed = 0.0f;

                if (limboIndex == limboInstrs.Length) {
                    Debug.Log("Iterated through all limbo instructions.");
                    exitLimbo();
                    audioManagerComponent.playSound(AudioManager.SoundType.START_DAY);     
                    flowManagerComponent.startFlow();     
                } 

                else {
                    this.instructionManagerComponent.setTemporaryMessage(limboInstrs[limboIndex]);

                    // Adding the ability to play a sound for desired instructions (i.e. countdown)
                    if (limboInstrs[limboIndex].playSoundAtStart) {
                        this.audioManagerComponent.playSound(AudioManager.SoundType.CRITICAL_TICK);
                    }
                }
            }
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
                dayScore = 0.0f;
                amountPayedForTreatmentDay = 0.0f;
                dailyCumulativePayload = 0;
                dailyCumulativeDelivered = 0;
                todaySpilled = 0;
                waitingForTreatment = false;
                waitingForTreatmentDuration = 0.0f;
                speedPenaltyElapsed = 0.0f;
                speedPenaltyFlag = false;
                fogImpairmentPanel.SetActive(false);
                Debug.Log ("New day: " + currentDay);

                // Set up the next day here
                if (currentDay <= this.configParser.numDays()) {

                    // Establish key parameters from the day configuration object
                    currentDayConfig = configParser.getConfigs()[currentDay - 1];
                    currentDayDuration = currentDayConfig.getDuration();
                    currentPayRate = currentDayConfig.getRewardMultiplier();
                    Debug.Log("Next day config loaded.");

                    if (currentDay != this.configParser.numDays()) {
                        nextDayDuration = configParser.getConfigs()[currentDay].getDuration();
                    } else {
                        nextDayDuration = 0.0f;
                    }

                    Debug.Log("Next day duration loaded.");

                    // Call out to necessary scripts to apply impairments for the current day (if any)
                    shakeImpStrCurrent = 0.0f;
                    shakeImpStrInitial = 0.0f;
                    speedImpStrCurrent = 0.0f;
                    fogImpStrCurrent   = 0.0f;

                    if ((currentDayImpairments = currentDayConfig.getImpairments()) != null && currentDayImpairments.Length > 0) {
                        foreach (Impairment imp in currentDayImpairments) {
                            float str = imp.getStrength();
                            switch (imp.getType()) {
                                case Impairment.ImpairmentType.PHYSICAL_SHAKE:
                                    rightHandTracker.applyImpairment(str);
                                    leftHandTracker.applyImpairment(str);
                                    shakeImpStrCurrent = str;
                                    shakeImpStrInitial = str;
                                    break;
                                case Impairment.ImpairmentType.VISUAL_FOG:
                                    fogImpairmentPanel.SetActive(true);
                                    Image fogImgComp = fogImpairmentPanel.GetComponent<Image>();
                                    fogImpStrCurrent = fogImpairmentMinAlpha + ((fogImpairmentMaxAlpha - fogImpairmentMinAlpha) * str);
                                    fogImgComp.color = new Color(fogImgComp.color.r, fogImgComp.color.g, fogImgComp.color.b, fogImpStrCurrent);
                                    break;
                                case Impairment.ImpairmentType.PHYSICAL_SPEED_PENALTY:
                                    speedPenaltyFlag = true;
                                    speedImpStrCurrent = str;
                                    break;
                                case Impairment.ImpairmentType.PHYSICAL_GRAVITY:
                                    //Physics.gravity = new Vector3 (0, PHYSICS_BASE_AMT + str * gravityImpairmentMaxDrop, 0);
                                    break;
                                default:
                                    Debug.Log("Invalid impairment type");
                                    break;
                            }
                        }
                    } Debug.Log("Next day impairments loaded.");


                    // Set up the treatment station if there should be treatments available
                    if ((currentDayTreatment = currentDayConfig.getTreatment()) != null) {

                        // Now updates acccording to the conditionals below
                        // pillManagerComponent.activatePanels();

                        // Set an appropriate temporary message / instruction set depending on the treatment
                        // scenario, e.g. is there just wait, just pay, or both?
                        bool hasPay = currentDayTreatment.hasPayOption();
                        bool hasWait = currentDayTreatment.hasWaitOption();

                        if (hasPay && hasWait)
                        {
                            pillManagerComponent.activatePanels(); // Activates both sets of panels/pills, etc
                            Debug.Log("Initializing limbo for pay&wait treatment...");
                            Instruction [] instrs   = new Instruction [8];
                            Instruction instrOne    = new Instruction ("Locate the medical station along\nthe wall opposite the windows.", 6.0f);
                            Instruction instrTwo    = new Instruction ("You can choose to either complete the\nnext day impaired, or, you can remove\nthe impairment in two ways.", 9.0f);
                            Instruction instrThree  = new Instruction ("Option 1: pay a fee to remove the\nimpairment instantly by\ngrabbing the red pill bottle.", 7.0f);
                            Instruction instrFour   = new Instruction ("Option 2: grab the blue pill bottle to\nwait for a duration of time to\nremove the impairment for free.", 8.0f);
                            Instruction instrFive   = new Instruction ("You can make this decision any time\nduring the next day.", 6.0f);

                            // Adding 3-second countdown to end of treatment instructions
                            Instruction countDownInstrOne   = new Instruction ("Resuming in:\n3", 1.0f, true);
                            Instruction countDownInstrTwo   = new Instruction ("Resuming in:\n2", 1.0f, true);
                            Instruction countDownInstrThree = new Instruction ("Resuming in:\n1", 1.0f, true);

                            instrs[0] = instrOne; instrs[1] = instrTwo; instrs[2] = instrThree; instrs[3] = instrFour; instrs[4] = instrFive;
                            instrs[5] = countDownInstrOne; instrs[6] = countDownInstrTwo; instrs[7] = countDownInstrThree;
                            limbo (instrs);
                        } 

                        else if (hasPay)
                        {
                            pillManagerComponent.activatePanel(PillManager.TreatmentObtainType.PAY);
                            Debug.Log("Initializing limbo for pay-only treatment...");
                            Instruction[] instrs    = new Instruction [6];
                            Instruction instrOne    = new Instruction ("Locate the medical station along\nthe wall opposite the windows.", 6.0f);
                            Instruction instrTwo    = new Instruction ("You can choose to either complete\nthis day impaired, or, pay a fee to remove\nthe impairment instantly.", 8.0f);
                            Instruction instrThree  = new Instruction ("If you wish to pay to receive treatment,\nyou can grab the pill bottle at any time\nduring the next day.", 8.0f);
                            
                            // Adding 3-second countdown to end of treatment instructions
                            Instruction countDownInstrOne   = new Instruction ("Resuming in:\n3", 1.0f, true);
                            Instruction countDownInstrTwo   = new Instruction ("Resuming in:\n2", 1.0f, true);
                            Instruction countDownInstrThree = new Instruction ("Resuming in:\n1", 1.0f, true);

                            instrs[0] = instrOne; instrs[1] = instrTwo; instrs[2] = instrThree;
                            instrs[3] = countDownInstrOne; instrs[4] = countDownInstrTwo; instrs[5] = countDownInstrThree;
                            limbo (instrs);
                        }

                        else if (hasWait)
                        {
                            pillManagerComponent.activatePanel(PillManager.TreatmentObtainType.WAIT);
                            Debug.Log("Initializing limbo for wait-only treatment...");
                            Instruction[] instrs    = new Instruction [6];
                            Instruction instrOne    = new Instruction ("Locate the medical station along\nthe wall opposite the windows.", 6.0f);
                            Instruction instrTwo    = new Instruction ("You can choose to either complete\nthis day impaired, or, wait a duration of time\nto remove the impairment at no cost.", 9.0f);
                            Instruction instrThree  = new Instruction ("If you wish to wait to receive treatment,\nyou can grab the pill bottle at any time\nduring the next day.", 8.0f);
                            
                            // Adding 3-second countdown to end of treatment instructions
                            Instruction countDownInstrOne   = new Instruction ("Resuming in:\n3", 1.0f, true);
                            Instruction countDownInstrTwo   = new Instruction ("Resuming in:\n2", 1.0f, true);
                            Instruction countDownInstrThree = new Instruction ("Resuming in:\n1", 1.0f, true);

                            instrs[0] = instrOne; instrs[1] = instrTwo; instrs[2] = instrThree;
                            instrs[3] = countDownInstrOne; instrs[4] = countDownInstrTwo; instrs[5] = countDownInstrThree;
                            limbo (instrs);
                        }

                        else Debug.Log("No treatment options found. Not displaying instruction message.");
                    }

                    Debug.Log("Finished loading current day treatment options.");

                    if (currentGameState != GameState.LIMBO) {
                        // Reset simulation parameters and play effects
                        currentGameState = GameState.RUNNING;
                        elapsedDayTime = 0.0f;
                        Debug.Log("Preparing to start day.");
                        transitionOverlay.SetActive(false);
                        audioManagerComponent.playSound(AudioManager.SoundType.START_DAY);      // On days with limbo, these two lines
                        flowManagerComponent.startFlow();                                       // need to be done elsewhere
                        Debug.Log("Starting day " + currentGameState);
                    } else {
                        elapsedDayTime = 0.0f;
                        Debug.Log("Preparing to start day with limbo enabled at start.");
                        transitionOverlay.SetActive(false);
                        Debug.Log("Starting day " + currentGameState + " with limbo enabled.");
                    }
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
                
                if (speedPenaltyFlag && currentPayload > 0 && speedPenaltyElapsed > WALK_SPEED_FREQ) {

                    // Speed tracking
                    day0PosB = physicalCamera.transform.position;
                    float walkSpd = lateralDistanceBetween (day0PosA, day0PosB) / WALK_SPEED_FREQ;

                    // Penalizing, if speed limit breached
                    if ((walkSpd > ((avgWalkingSpeedDay0) * (1.0f - speedImpStrCurrent)))) {
                        //Debug.Log("#");
                        waterDropletCounterComponent.removeDropsFromContainer(1);
                        //Debug.Log(String.Format("{0} | {1} | {2} | Removing 1 drop", walkSpd, avgWalkingSpeedDay0, (avgWalkingSpeedDay0 * (1.0f - speedImpStrCurrent))));
                    }

                    speedPenaltyElapsed = 0.0f;
                    day0PosA = physicalCamera.transform.position;
                }

                elapsedDayTime += Time.deltaTime;
                speedPenaltyElapsed += Time.deltaTime;

                // Track how long the participant has been waiting for treatment
                // since the end of the limbo after they grabbed the pill bottle
                if (waitingForTreatment)
                {
                    waitingForTreatmentDuration -= Time.deltaTime;
                    timeWaitedForTreatmentDay += Time.deltaTime;
                    timeWaitedForTreatmentTotal += Time.deltaTime;

                    if (waitingForTreatmentDuration <= 0.0f)
                    {
                        // Re-enable bucket pick up 
                        Debug.Log("Enabling hands: L=" + leftHandScriptComp.enabled + " R=" + rightHandScriptComp.enabled);
                        leftHandScriptComp.enabled = true;
                        rightHandScriptComp.enabled = true;
                        Debug.Log("Enabling hands: L=" + leftHandScriptComp.enabled + " R=" + rightHandScriptComp.enabled);
                        this.waitingForTreatment = false;
                        waitingForTreatmentDuration = 0.0f;
                        Debug.Log("Treatment wait time has been passed..");
                        applyPostTreatmentActions();
                        pillManagerComponent.disablePanels();
                    }
                }

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

                    Debug.Log("Time's up");
                    flowManagerComponent.cleanScene();
                    Debug.Log("Done cleaning");
                    pillManagerComponent.disablePanels();
                    Debug.Log("Disabled panels.");
                    unapplyImpairments();
                    Debug.Log("Day complete " + currentGameState);

                    // Either enter the transition phase before beginning the new day, or
                    // we're all done - play sound effects and set states accordingly.
                    if (currentDay + 1 > totalDays) {
                        Debug.Log("Simulation complete.");
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

            else if (currentScore >= unimpairedDayZeroThreshold) {
                audioManagerComponent.playSound(AudioManager.SoundType.DAY_COMPLETE);
                avgWalkingSpeedDay0 = avgWalkingSpeedDay0 / secondsInDay1;
                Debug.Log ("Day 0 passed.");
                Debug.Log ("Determined average moving speed = " + (avgWalkingSpeedDay0 / UNITY_VIVE_SCALE).ToString("0.000") + "m/s");
                currentGameState = GameState.TRANSITION;
                transitionOverlay.SetActive(true);
                resetMetricsForDayOne();
                flowManagerComponent.cleanScene();
                pillManagerComponent.disablePanels();
                Destroy(DayZeroSpeedCounter);
                day0PosA = physicalCamera.transform.position;
                day0PosB = physicalCamera.transform.position;
                Debug.Log("Day 0 over " + currentGameState);
            }

            else {
                // Only track the participant's speed if they're carrying water,
                // and if they've completed the tutorial / standing in the correct. 
                // place. This gives us a more accurate measurement of their delivery speed.
                if (inDay0SpeedCaptureZone && currentPayload > 0 && (currentTutorialStep == TutorialStep.CONTINUE || !this.configParser.getSimInstruction())) {

                    dayZeroMovingElapsed += Time.deltaTime;

                    // We only want to calculate speed once every quarter-second
                    // For performance reasons
                    if (dayZeroMovingElapsed > DAY_ZERO_MOV_FREQ) {
                        
                        // Exclude (literal) edge cases where the participant has just entered the zone
                        if (dayZeroMovingCount > 1) {
                            day0PosB = physicalCamera.transform.position;
                            avgWalkingSpeedDay0 += lateralDistanceBetween (day0PosA, day0PosB);  // Distance since last count
                            secondsInDay1 += DAY_ZERO_MOV_FREQ;
                        }

                        dayZeroMovingCount++;
                        dayZeroMovingElapsed = 0.0f;
                        day0PosA = physicalCamera.transform.position;
                    }
                }
            }  
        }
    }
}
