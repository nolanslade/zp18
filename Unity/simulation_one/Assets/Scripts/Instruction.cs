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

    public Instruction (string m, float d)
    {
        this.message = m;
        this.displayDuration = d;
    }
}
