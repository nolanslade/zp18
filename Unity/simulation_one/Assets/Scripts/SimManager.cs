using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimManager : MonoBehaviour {
    
    // State management
    public enum GameState
    {
        PAUSED,     // Still allows for physical movement
        RUNNING,
        ERROR
    }

    private GameState currentGameState;
    private int currentScore;
    private int currentDay;
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
        }

        else {
            currentScore = 0;
            currentGameState = GameState.RUNNING;
            flowManager.GetComponent<FlowManager>().startFlow();
        }
    }
	

	// Update is called once per frame
	void Update () {

        if (currentGameState == GameState.ERROR) {
            int a = 1;
        } 

        else if (currentGameState == GameState.PAUSED) {
            int a = 1;
        } 

        else {
            int a = 1; 
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


    /*
    * Creates the parser object to read in the configuration
    * file for this simulation. 
    * Returns true on success of all parameters being set, false on any error.
    */
    private bool establishSimulationParameters () {
        return true;
    }
}
