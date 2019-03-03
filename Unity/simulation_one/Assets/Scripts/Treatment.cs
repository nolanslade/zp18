using System;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Jan 20 2019
 * 
 * Class to model treatment options
 * for a particular simulation day
 */
public class Treatment {

	// Cost Function Parameters
	// Default function: cost = C * (omega - DT + (1/omega)*T^2)
	// Dynamic function: cost = C * (c - bT + aT^2)
	// Where T is the time of day, omega = length of current day, D = current day

    // Wait function similar - except in seconds.

	private float cost_C;			// Original cost; will depreciate according to the below a,b,c		
	private float cost_a;	
	private float cost_b;
	private float cost_c;
    private float wait_C;           // Same as above, expect for wait function      
    private float wait_a;   
    private float wait_b;
    private float wait_c;
	private float obtainTime = -1.0f;  

    private bool effective;                     // Calculated once based on the effectiveness probability
    private bool effectiveCalculated = false;   // Whether or not the above has been calculated
    private bool death;                         // As above, except for the death penalty
    private bool deathCalculated = false;

	// Behaviour Parameters
    private float effectiveProbability;		// 0.0 to 1.0 - probability that the treatment will be effective 
    private float effectiveness;			// 0.0 to 1.0 - removes this amount from the impairment factor
    private float delayPenaltyProbability;	// 0.0 to 1.0 - probability that receiving treatment will result in a delay penalty
    private float delayPenaltyAmt;			// Seconds that scoring / tap flow / etc will be disabled
    private float deathPenaltyProbability;	// 0.0 to 1.0 - probability that receiving treatment results in death (end simulation? or end day?) // TODO

    // TODO: should probably auto correct or throw exceptions after format / type checking parameters (e.g. someone entering 70 instead of 0.7, or whatever)

    public Treatment ( 
    	float c_C,             // Cost function
    	float c_a, 
    	float c_b, 
    	float c_c,             
        float w_C,             // Wait function
        float w_a, 
        float w_b, 
        float w_c,
    	float effProb,         // Probability that treatment will actually work
    	float eff,             // The percentage (0.0 to 1.0) that the treatment will take away, if it is effective
    	float delProb,         // Delay Penalty 
    	float del,             // "
    	float deathProb    	   // Death Probability
    	// ... Additional parameters here
    	) {
        
    	this.cost_C						= c_C; 
    	this.cost_a 					= c_a; 
    	this.cost_b 					= c_b; 
    	this.cost_c 					= c_c;

        this.wait_C                     = w_C; 
        this.wait_a                     = w_a; 
        this.wait_b                     = w_b; 
        this.wait_c                     = w_c;

    	this.effectiveProbability 	 	= effProb;
    	this.effectiveness 			 	= eff;
    	this.delayPenaltyProbability 	= delProb;
    	this.delayPenaltyAmt 		 	= del;
    	this.deathPenaltyProbability 	= deathProb;
    }

    /*
	* Calculates current cost based on the 
	* applicable cost function and the current time
    */
    public float currentCost (float currentSimTime) {
    	// cost = C * (c - bT + aT^2)
    	return cost_C * (cost_c - cost_b * currentSimTime + cost_a * currentSimTime * currentSimTime);
    }


    /*
    * Similar to the above, but calculates wait time 
    * instead of a cost. Used when treatment received
    * for free in return for waiting.
    */
    public float currentWaitTime (float currentSimTime) {
        // wait time = C * (c - bT + aT^2)
        return wait_C * (wait_c - wait_b * currentSimTime + wait_a * currentSimTime * currentSimTime);
    }


    /*
    * Returns a one-time calculated boolean value that signifies whether
    * or not the treatment will be effective - based on the 
    * effectiveness probability parameter
    */
    public bool isEffective () {
        
        if (!effectiveCalculated) {
            System.Random r = new System.Random();
            effective = r.Next(100) < ((int) (100.0f * effectiveProbability));
            effectiveCalculated = true;
        }

        return effective;
    }


    /*
    * As above, except for death penalty - calculates once
    *
    */
    public bool causesDeath () {
        
        if (!deathCalculated) {
            System.Random r = new System.Random();
            death = r.Next(100) < ((int) (100.0f * deathPenaltyProbability));
            deathCalculated = true;
        }

        return death;
    }


    /*
    * Standard getters
    */
    public float getEffectiveness () {
    	return this.effectiveness;
    }

    public float getEffectiveProbability () {
    	return this.effectiveProbability;
    }

    public float getDelayPenalty () {
    	return this.delayPenaltyAmt;
    }

    public float getDelayPenaltyProbability () {
    	return this.delayPenaltyProbability;
    }

    public float getDeathPenaltyProbability () {
    	return this.deathPenaltyProbability;
    }

    public void obtain (float t) {
        this.obtainTime = t;
    }

    public bool hasBeenObtained () {
        return this.obtainTime != -1.0f;
    }
}