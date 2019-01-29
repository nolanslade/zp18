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
	private float C;			// Original cost; will depreciate according to the below a,b,c		
	private float a;	
	private float b;
	private float c;
	private float obtainTime;

	// Behaviour Parameters
    private float effectiveProbability;		// 0.0 to 1.0 - probability that the treatment will be effective 
    private float effectiveness;			// 0.0 to 1.0 - removes this amount from the impairment factor
    private float delayPenaltyProbability;	// 0.0 to 1.0 - probability that receiving treatment will result in a delay penalty
    private float delayPenaltyAmt;			// Seconds that scoring / tap flow / etc will be disabled
    private float deathPenaltyProbability;	// 0.0 to 1.0 - probability that receiving treatment results in death (end simulation? or end day?) // TODO

    // TODO: should probably auto correct or throw exceptions after format / type checking parameters (e.g. someone entering 70 instead of 0.7, or whatever)

    public Treatment ( 
    	float C, 
    	float a, 
    	float b, 
    	float c,
    	float effProb,
    	float eff,
    	float delProb,
    	float del,
    	float deathProb    	
    	// ... Additional parameters here
    	) {
        
    	this.C 							= C; 
    	this.a 							= a; 
    	this.b 							= b; 
    	this.c 							= c;
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
    	return C * (c - b * currentSimTime + a * currentSimTime * currentSimTime);
    }


    /*
    * Similar to the above, but calculates wait time 
    * instead of a cost. Used when treatment received
    * for free in return for waiting.
    */
    public float currentWaitTime (float currentSimTime) {
        // TODO
        return 0.0f;
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
}