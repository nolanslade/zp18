using System;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
 * Dec 22 2018
 * 
 * Keywords for config file parsing.
 */
public sealed class ConfigKeyword {

    // Auxiliary
    public static readonly string COMMENT       = "#";
    public static readonly string SEPARATOR     = ":";
    public static readonly string INDENT        = "\t";
    public static readonly string VALUE_START   = "\"";
    public static readonly string VALUE_END     = "\"";
    public static readonly string PERCENT       = "%";
    public static readonly string F_SLASH       = "/";

    // Top-level keywords
    public static readonly string SIMULATION    = "Simulation";
    public static readonly string DAY           = "Day";

    // Key-value tier 2 keywords
    public static readonly string NAME          = "Name";           // Configuration name/id
    public static readonly string OUTPUT        = "Output";         // Output type specification
    public static readonly string DESCRIPTION   = "Description";    // For simulation
    public static readonly string SOUND         = "Sound";          // For simulation sound
    public static readonly string SCENE         = "Scene";          // For simulation scene

    public static readonly string DURATION      = "Duration";       // For a given day
    public static readonly string IMPAIRMENT    = "Impairment";     // For a given day
    public static readonly string TREATMENT     = "Treatment";      // For a given day


    // Key-value tier 3 keywords
    public static readonly string TYPE          = "Type";           // For a given impairment
    public static readonly string FACTOR        = "Factor";         // For a given impairment

    // Key-value, tier 4
    public static readonly string WAIT          = "Wait";           // For a given treatment - 
    public static readonly string CERTAINTY     = "Certainty";      // For a given treatment -
    public static readonly string COST          = "Cost";           // For a given treatment - in dollars / reward units

    //Key-value, tier 5
    public static readonly string C             = "C";              //
    public static readonly string a             = "a";              //
    public static readonly string b             = "b";
    public static readonly string c             = "c";
}


