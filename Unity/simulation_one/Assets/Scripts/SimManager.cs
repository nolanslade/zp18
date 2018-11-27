using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimManager : MonoBehaviour {
    
    // State management
    enum GameState
    {
        PAUSED,
        RUNNING
    }

    private GameState currentGameState;
    private int currentScore;


    // Key scene objects
    public GameObject container;           // Vessel to carry water
    public GameObject sourceSink;
    public GameObject sourceFaucet;
    public GameObject destinationSink;
    public GameObject virtualHeadset;      // [CameraRig] object - position relative to Unity Units
    public GameObject physicalHeadset;     // Child object of [CameraRig]
    public GameObject waterDroplet;        // Water drop prefab


	// Run once  on initialization
	void Start () {

        // Hard coding this for now
        currentScore = 0;
        currentGameState    = GameState.RUNNING;
    }
	

	// Update is called once per frame
	void Update () {
		
	}


    GameState currentState () {
        return currentGameState;
    }


    void payReward ()
    {
        this.currentScore++;
        Debug.Log ("Reward payed (1). New Score:" + currentScore);
    }


    void payReward (int customAmount)
    {
        this.currentScore += customAmount;
        Debug.Log("Reward payed ("+ customAmount + "). New Score:" + currentScore);
    }
}
