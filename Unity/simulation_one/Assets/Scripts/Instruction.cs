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

    // For configurable instructions
    public enum InstructionType {

        // Day Zero 'DZ'
        DZ_LOCATE_BUCKET,
        DZ_HOLD_BUCKET,
        DZ_FILL_BUCKET,
        DZ_GO_TO_SINK,
        DZ_POUR_OUT_BUCKET,
        DZ_OBJECTIVE,

        // Impaired Day Zero
        DZ_IMP_START_FOG,
        DZ_IMP_START_SHAKE,
        DZ_IMP_START_GENERIC,
        DZ_IMP_EXPLAIN_SHAKE,
        DZ_IMP_EXPLAIN_GENERIC,
        DZ_IMP_OBJECTIVE,

        // Treatment instructions
        TMT_HYBRID_LOCATE_STATION,
        TMT_HYBRID_METHOD,
        TMT_HYBRID_PAY_OPTION,
        TMT_HYBRID_WAIT_OPTION,
        TMT_HYBRID_ENDING,

        TMT_PAY_LOCATE_STATION,
        TMT_PAY_METHOD,
        TMT_PAY_ENDING,

        TMT_WAIT_LOCATE_STATION,
        TMT_WAIT_METHOD,
        TMT_WAIT_ENDING,
        TMT_WAIT_CHOSEN_START,
        TMT_WAIT_CHOSEN_END

    }

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
