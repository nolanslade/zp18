using System;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Dec 19 2018
 * 
 * Class to encapsulate all parameters
 * for each day in the config file. 
 * These parameters include
 * - ID / day number
 * - Duration (in seconds)
 * - Active impairments and their strengths
 * - Reward function multiplier
 */
public class DayConfiguration {

    private int dayNumber;
    private float duration;
    private Impairment[] impairments;
    private Treatment treatment;
    private float rewardFunctionMultiplier = 1.0f;

    public DayConfiguration (int id, float dur, Impairment [] imps, Treatment tr, float rewMult) 
    {
        this.dayNumber = id;
        this.duration = dur;
        this.impairments = imps;
        this.treatment = tr;
        this.rewardFunctionMultiplier = rewMult;
    }

    public DayConfiguration(int id, float dur, Impairment[] imps, Treatment tr)
    {
        this.dayNumber = id;
        this.duration = dur;
        this.impairments = imps;
        this.treatment = tr;
    }

    public int getDayNumber () {
        return this.dayNumber;
    }

    public float getDuration () {
        return this.duration;
    }

    public Impairment[] getImpairments () {
        return this.impairments; 
    }

    public Treatment getTreatment () {
        return this.treatment;
    }

    public float getRewardMultiplier () {
        return this.rewardFunctionMultiplier;
    }
}
