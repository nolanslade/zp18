using System;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Dec 22 2018
 * 
 * Keywords for config file parsing.
 */
public sealed class ConfigKeyword {

    public static readonly char NEWLINE             = '\n';
    public static readonly char COLON               = ':';
    public static readonly char TAB                 = '\t';
    public static readonly char HASH                = '#';
    public static readonly char SPACE               = ' ';
    public static readonly char PERCENTAGE          = '%';
    public static readonly char COMMA               = ',';

    public static readonly string COMMENT           = "#";
    public static readonly string SEPARATOR         = ":";
    public static readonly string INDENT            = "\t";
    public static readonly string VALUE_START       = "\"";
    public static readonly string VALUE_END         = "\"";
    public static readonly string PERCENT           = "%";
    public static readonly string F_SLASH           = "/";
    public static readonly string SIMULATION        = "Simulation";
    public static readonly string TUTORIAL          = "Tutorial";
    public static readonly string DAY               = "Day";
    public static readonly string NAME              = "Name"            .ToUpper();           // Configuration name/id
    public static readonly string OUTPUT            = "Output"          .ToUpper();         // Output type specification
    public static readonly string DESCRIPTION       = "Description"     .ToUpper();    // For simulation
    public static readonly string INSTRUCTIONS      = "Instructions"    .ToUpper();   // For simulation instrunctions
    public static readonly string SOUND             = "Sound"           .ToUpper();          // For simulation sound
    public static readonly string SCENE             = "Scene"           .ToUpper();          // For simulation scene
    public static readonly string SCORE             = "Score";          // For day 0 score (unimpaired)
    public static readonly string IMP_SCORE         = "ImpairedScore";  // For second stage of day 0 (optional) - impaired threshold
    public static readonly string DURATION          = "Duration";       // For a given day
    public static readonly string IMPAIRMENT        = "Impairment";     // For a given day
    public static readonly string TREATMENT         = "Treatment";      // For a given day
    public static readonly string BALLVALUE         = "BallValue";      // For a given day 
    public static readonly string TYPE              = "Type";           // For a given impairment
    public static readonly string STRENGTH          = "Strength";         // For a given impairment
    public static readonly string FOG_IMP           = "Visual/Fog";
    public static readonly string SHAKE_IMP         = "Physical/Shake";
    public static readonly string WAIT              = "Wait";           // For a given treatment - 
    public static readonly string CERTAINTY         = "Certainty";      // For a given treatment -
    public static readonly string COST              = "Cost";           // For a given treatment - in dollars / reward units
    public static readonly string EFFECTIVENESS     = "Effectiveness";  // For a given treatment - 
    public static readonly string C                 = "C";              //
    public static readonly string a                 = "a";              //
    public static readonly string b                 = "b";
    public static readonly string c                 = "c";
    public static readonly string PROBABILITY       = "Probability";
    public static readonly string EFFECT            = "Effect";
    public static readonly string DEFAULT           = "Default";
    public static readonly string DISABLED          = "Disabled";
    public static readonly string ENABLED           = "Enabled";

    public static readonly string LOCATE_BUCKET_INSTR   = "LocateBucketInstruction" .ToUpper();
    public static readonly string HOLD_BUCKET_INSTR     = "HoldBucketInstruction"   .ToUpper();
    public static readonly string FILL_BUCKET_INSTR     = "FillBucketInstruction"   .ToUpper();
    public static readonly string LOCATE_SINK_INSTR     = "LocateSinkInstruction"   .ToUpper();
    public static readonly string POUR_BUCKET_INSTR     = "PourBucketInstruction"   .ToUpper();

    public static readonly string START_FOG_INSTR           = "FogImpInstruction"               .ToUpper();
    public static readonly string START_SHAKE_INSTR         = "ShakeImpInstruction"             .ToUpper();
    public static readonly string START_GENERIC_INSTR       = "GenericImpInstruction"           .ToUpper();
    public static readonly string EXPLAIN_SHAKE_INSTR       = "ExplainShakeImpInstruction"      .ToUpper();
    public static readonly string EXPLAIN_GENERIC_INSTR     = "ExplainGenericImpInstruction"    .ToUpper();

    public static readonly string HYBRID_TMT_LOCATE_INSTR   = "HybridTreatmentLocateInstruction"    .ToUpper();
    public static readonly string HYBRID_TMT_METHOD_INSTR   = "HybridTreatmentMethodInstruction"    .ToUpper();
    public static readonly string HYBRID_TMT_PAY_INSTR      = "HybridTreatmentPayInstruction"       .ToUpper();
    public static readonly string HYBRID_TMT_WAIT_INSTR     = "HybridTreatmentWaitInstruction"      .ToUpper();
    public static readonly string HYBRID_TMT_END_INSTR      = "HybridTreatmentEndInstruction"       .ToUpper();

    public static readonly string PAY_TMT_LOCATE_INSTR      = "PayTreatmentLocateInstruction"   .ToUpper();
    public static readonly string PAY_TMT_METHOD_INSTR      = "PayTreatmentMethodInstruction"   .ToUpper();
    public static readonly string PAY_TMT_END_INSTR         = "PayTreatmentEndInstruction"      .ToUpper();

    public static readonly string WAIT_TMT_LOCATE_INSTR         = "WaitTreatmentLocateInstruction"   .ToUpper();
    public static readonly string WAIT_TMT_METHOD_INSTR         = "WaitTreatmentMethodInstruction"   .ToUpper();
    public static readonly string WAIT_TMT_END_INSTR            = "WaitTreatmentEndInstruction"      .ToUpper();
    public static readonly string WAIT_TMT_CHOSEN_START_INSTR   = "WaitTreatmentChosenInstruction"   .ToUpper();
    public static readonly string WAIT_TMT_CHOSEN_END_INSTR     = "WaitTreatmentResumeInstruction"   .ToUpper();

}
