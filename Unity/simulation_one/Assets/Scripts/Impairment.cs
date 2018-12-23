using System;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Dec 22 2018
 * 
 * Class to model each impairment as well as
 * all supported impairment types.
 */
public class Impairment {

    private ImpairmentType type;
    private float strength;

    public enum ImpairmentType {
        VISUAL_FOG,
        PHYSICAL_JITTER,
        PHYSICAL_SPEED_LIMITER  // e.g. remove water from bucket if participant moves too quickly
    }

    public Impairment (ImpairmentType t, float s) {
        this.type       = t;
        this.strength   = s;
    }

    public ImpairmentType getType () {
        return this.type;
    }

    public float getStrength () {
        return this.strength;
    }
}