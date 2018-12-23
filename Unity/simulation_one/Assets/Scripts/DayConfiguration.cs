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
 * - Duration
 * - Active impairments and their strengths
 * - Reward function multiplier
 * - ......
 */
public class DayConfiguration {

    private int dayNumber;
    private float duration;
    private Impairment[] impairments;
    private float rewardFunctionMultiplier = 1.0f;

    public DayConfiguration (int id, float dur, Impairment [] imps, float rewMult) 
    {
        this.dayNumber = id;
        this.duration = dur;
        this.impairments = imps;
        this.rewardFunctionMultiplier = rewMult;
    }

    public DayConfiguration(int id, float dur, Impairment[] imps)
    {
        this.dayNumber = id;
        this.duration = dur;
        this.impairments = imps;
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

    public float getRewardMultiplier () {
        return this.rewardFunctionMultiplier;
    }
}
