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
        NULL,
        VISUAL_FOG,
        PHYSICAL_GRAVITY,       // Makes things easier to drop by accident
        PHYSICAL_SHAKE,
        PHYSICAL_SPEED_PENALTY  // e.g. remove water from bucket if participant moves too quickly
    }

    public Impairment () {
        this.type = ImpairmentType.NULL;
        this.strength = -99.0f;
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

    public void setType (ImpairmentType t) {
        this.type = t;
    }

    public void setStrength (float s) {
        this.strength = s;
    }
}