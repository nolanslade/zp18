using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Mar 18 2019
 * 
 * Encapsulating an instruction
 */
public class Instruction {

    public string message;
    public float displayDuration;
    public bool playSoundAtStart;  // Will play a countdown at the start of the display duration

    public Instruction (string m, float d)
    {
        this.message = m;
        this.displayDuration = d;
        this.playSoundAtStart = false;
    }

    public Instruction (string m, float d, bool p) {
    	this.message = m;
        this.displayDuration = d;
        this.playSoundAtStart = p;
    }
}
