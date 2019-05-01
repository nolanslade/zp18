using System;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * McDSL: VR Simulation One
 * Aaska Shah
 * Dec 22 2018
 *
 * Configuration file parser. Config
 * file is indent-based, with keywords,
 * values, and attributes. This parsing
 * is done at execution-time.
 *
 * If parsing is successful, then the SimManager
 * can access the needed parameters here, as well.
 */
public class ConfigParser
{
    // Parsing
    private string configFilePath;
    private string dbConnection;
    private int dayCount;                               //total number of days in the config file, used for parseConfigs()
    private List<string> dayList = new List<string>();  //contains all day information ready to be split into dayConfigs
    private List<string> sim = new List<string>();      //contains all info about the simulation itself

    // Simulation parameters
    private DayConfiguration[] dayConfigs = null;
    private bool lowNauseaMode;
    private bool claustrophobicMode;
    private bool instructionsEnabled;
    private bool soundEnabled;

    // Day 0 Scoring / impairments - Nolan April 2019
    public List<Impairment> dayZeroImpairments = new List <Impairment> ();
    public static float DAY_0_DEFAULT_SCORE = 150.0f;  
    private float dayZeroScoreUnimpaired = 0.0f;        // How much the participant is required to earn before moving on to either the first paid day, or the impaired training period
    private float dayZeroScoreImpaired = 0.0f;          // Same, but in the impaired training period (if one is requested)
    public static int UNIMPAIRED = 0;   // For fetching day 0 score threshold (unimpaired)
    public static int IMPAIRED = 1;     // Same, but for the impaired threshold

    private float ballValue;
    private string name;
    private string[] output;
    private string description;
    private string instructions;
    private string sound;
    private string scene;

    // Adding configurable instructions
    private Instruction locateBucketInstr   = new Instruction ("Objective: locate and walk to the bucket", 8.0f);
    private Instruction holdContainerInstr  = new Instruction ("To pick up, place one hand on the\nbucket and squeeze index finger", 7.0f);
    private Instruction fillInstr           = new Instruction ("Fill up the bucket with balls by\nplacing it under the pipe", 6.0f);
    private Instruction goToSinkInstr       = new Instruction ("Carefully turn around and carry\nthe bucket to the opposing sink", 7.0f);
    private Instruction pourBucketInstr     = new Instruction ("Pour the contents of the bucket\ninto the sink to earn money", 6.0f);
    private Instruction continueInstr;

    private Instruction fogImpairedRoundStart       = new Instruction ("When the simulation resumes, you\nwill notice you have reduced vision.\nYou are impaired.", 7.5f);
    private Instruction shakeImpairedRoundStart     = new Instruction ("You will notice your controllers\nare now shaking. You are impaired.", 7.0f);
    private Instruction genericImpairedRoundStart   = new Instruction ("When the simulation resumes,\nyou will be impaired.", 5.5f);
    private Instruction shakeImpairedRoundExplain   = new Instruction ("While you are impaired, carrying water\nand earning money will be more difficult.", 8.0f);
    private Instruction genericImpairedRoundExplain = new Instruction ("While you are impaired, earning money\nwill be more difficult.", 6.5f);
    private Instruction impairedRoundObjective;     


    // Adding configurable instructions
    public Instruction getInstruction (Instruction.InstructionType instrType) {
        
        // Return the corresponding instruction object, which may have been customized 
        // during the parsing process.
        switch (instrType) {
            
            // Main tutorial instructions
            case Instruction.InstructionType.DZ_LOCATE_BUCKET:
                return locateBucketInstr;
            case Instruction.InstructionType.DZ_HOLD_BUCKET:
                return holdContainerInstr;
            case Instruction.InstructionType.DZ_FILL_BUCKET:
                return fillInstr;
            case Instruction.InstructionType.DZ_GO_TO_SINK:
                return goToSinkInstr;
            case Instruction.InstructionType.DZ_POUR_OUT_BUCKET:
                return pourBucketInstr;
            case Instruction.InstructionType.DZ_OBJECTIVE:
                continueInstr = new Instruction ("To start the experiment, repeat this\nprocess until you've earned $" + dayZeroScoreUnimpaired.ToString("0.00"), 7.0f);
                return continueInstr;

            // Impaired portion of day 0
            case Instruction.InstructionType.DZ_IMP_START_FOG:
                return fogImpairedRoundStart;
            case Instruction.InstructionType.DZ_IMP_START_SHAKE:
                return shakeImpairedRoundStart;
            case Instruction.InstructionType.DZ_IMP_START_GENERIC:
                return genericImpairedRoundStart;
            case Instruction.InstructionType.DZ_IMP_EXPLAIN_SHAKE:
                return shakeImpairedRoundExplain;
            case Instruction.InstructionType.DZ_IMP_EXPLAIN_GENERIC:
                return genericImpairedRoundExplain;
            case Instruction.InstructionType.DZ_IMP_OBJECTIVE:
                impairedRoundObjective = new Instruction ("New Objective: Earn another $" + dayZeroScoreImpaired.ToString("0.00"), 6.0f);
                return impairedRoundObjective;

            // Impairments for paid days 

            // Treatments

            default:
                Debug.Log("No instruction to fetch for type: " + instrType);
                return null;
        }
    }


    /*
    * If using a configuration file, then load in day configurations
    * by parsing in the file if the path argument is valid.
    */
    public ConfigParser (string path)
    {
        lowNauseaMode = ParticipantData.nauseaSensitive;
        claustrophobicMode = ParticipantData.claustrophicSensitive;

        this.configFilePath = path;
        dbConnection = null;
        StreamReader reader = new StreamReader(this.configFilePath);
        string line = reader.ReadLine();

        this.dayCount = 0;
        bool isSim = false;
        bool isDay = false;
        bool isTut = false;
        while (line != null)
        {
            string[] fields = line.Split(ConfigKeyword.NEWLINE);

            //splits information into 2 groups, Simulation and Day
            for (int i = 0; i < fields.Length; i++)
            {

                if (fields[i].Contains(ConfigKeyword.COMMENT))
                {
                    string[] split = fields[i].Split(ConfigKeyword.HASH);
                    fields[i] = split[0];
                }
                if (fields[i] == string.Empty)
                {
                    continue;
                }
                else if (fields[i].Contains(ConfigKeyword.SIMULATION))
                {
                    isSim = true;
                    isDay = false;
                    isTut = false;
                    this.sim.Add(fields[i]);
                }
                else if (fields[i].Contains(ConfigKeyword.TUTORIAL))
                {
                    isTut = true;
                    isSim = false;
                    isDay = false;

                }
                // currently the day group is further split into it's separate lines for better parsing
                else if (fields[i].Contains(ConfigKeyword.DAY))
                {
                    this.dayCount++;
                    dayList.Add("Day " + dayCount.ToString());                    
                    isDay = true;
                    isTut = false;
                    isSim = false;
                }


                if ((fields[i].Contains(ConfigKeyword.INDENT)) && isSim)
                {
                    string simParam = fields[i].Replace(ConfigKeyword.INDENT, "");
                    this.sim.Add(simParam);
                }


                /* ****************************************************************************************************************************** */
                // Nolan April 2019
                // Fixing this whole section because this is really hacky. They could write any keyword instead of score and it would still work.
                else if ((fields[i].Contains(ConfigKeyword.INDENT)) && isTut)
                {
                    // Unimpaired portion of day 0 (standard tutorial round)
                    if (fields[i].ToUpper().Contains(ConfigKeyword.SCORE.ToUpper()) && !(fields[i].ToUpper().Contains(ConfigKeyword.IMP_SCORE.ToUpper()))) {

                        string score = fields[i].Replace(ConfigKeyword.INDENT, "");
                        this.dayZeroScoreUnimpaired = float.Parse(score.Split(ConfigKeyword.COLON)[1]);
                    }

                    // Impaired tutorial stage of day 0
                    else if (fields[i].ToUpper().Contains(ConfigKeyword.IMP_SCORE.ToUpper())) {
                        string score = fields[i].Replace(ConfigKeyword.INDENT, "");
                        this.dayZeroScoreImpaired = float.Parse(score.Split(ConfigKeyword.COLON)[1]);
                    }

                    // Parse a second-round day zero impairment
                    else if (fields[i].ToUpper().Contains(ConfigKeyword.IMPAIRMENT.ToUpper())) {

                        int trackInf = 0; int infiniteLoop = 100;

                        Impairment newDayZeroImp = new Impairment ();

                        line = reader.ReadLine();

                        while (true) {
                            
                            trackInf++;

                            // Set impairment type
                            if (line.ToUpper().Contains(ConfigKeyword.TYPE.ToUpper())) {

                                if (line.ToUpper().Contains(ConfigKeyword.FOG_IMP.ToUpper())) {
                                    newDayZeroImp.setType(Impairment.ImpairmentType.VISUAL_FOG);
                                }

                                else if (line.ToUpper().Contains(ConfigKeyword.SHAKE_IMP.ToUpper())) {
                                    newDayZeroImp.setType(Impairment.ImpairmentType.PHYSICAL_SHAKE);
                                }

                                else {
                                    Debug.Log("UNSUPPORTED IMPAIRMENT TYPE FOR DAY ZERO: " + fields[i]);
                                }


                                if (newDayZeroImp.getStrength() == -99.0f) {
                                    line = reader.ReadLine();
                                }

                                else {

                                    if (newDayZeroImp.getType() == Impairment.ImpairmentType.NULL || newDayZeroImp.getStrength() == -99.0f) {
                                        Debug.Log("Problem parsing day 0 impairment - not enough details for impairment.");
                                    }

                                    else {
                                        dayZeroImpairments.Add(newDayZeroImp);
                                        Debug.Log("New impairment for day 0 with type: " + newDayZeroImp.getType().ToString() + " and strength: " + newDayZeroImp.getStrength().ToString());
                                    }

                                    break; // We're done
                                }
                            }

                            // Set impairment strength
                            else if (line.ToUpper().Contains(ConfigKeyword.STRENGTH.ToUpper())) {

                                newDayZeroImp.setStrength (
                                    float.Parse(line.Split(':')[1].Split('#')[0].Replace("%", "").Trim()) / 100.0f
                                );

                                if (newDayZeroImp.getType() == Impairment.ImpairmentType.NULL) {
                                    line = reader.ReadLine();
                                }

                                else {

                                    if (newDayZeroImp.getType() == Impairment.ImpairmentType.NULL || newDayZeroImp.getStrength() == -99.0f) {
                                        Debug.Log("Problem parsing day 0 impairment - not enough details for impairment.");
                                    }

                                    else {
                                        dayZeroImpairments.Add(newDayZeroImp);
                                        Debug.Log("New impairment for day 0 with type: " + newDayZeroImp.getType().ToString() + " and strength: " + newDayZeroImp.getStrength().ToString());
                                    }

                                    break; // We're done
                                }
                            }

                            if (trackInf >= infiniteLoop) { Debug.Log("Problem parsing day 0 impairment - infinite loop."); break; }
                        }
                    }

                    else
                    {
                        Debug.Log ("Unexpected line during tutorial parsing: \"" + fields[i] + "\"");
                    }
                }
                /* ****************************************************************************************************************************** */

                else if ((fields[i].Contains(ConfigKeyword.INDENT)) && isDay)
                {
                    dayList.Add(fields[i].Replace(ConfigKeyword.INDENT, ""));

                }

            }

            line = reader.ReadLine();
        }

        parseSim();
        parseConfig();
        printConfigSummary();

    }


    /*
    * Nolan April 30 2019
    * For logging / debugging purposes - always print out
    * a summary of what we've parsed and what final values
    * are going into the simulation.
    */
    private void printConfigSummary () {

        StringBuilder sb = new StringBuilder ();

        sb.Append("*****************************");
        sb.Append("\nConfiguration Parsing Summary");

        sb.Append("\n\n--- Simulation ---");
        sb.Append("\nInstructions enabled: "  + this.instructionsEnabled.ToString());
        sb.Append("\nSound enabled: "         + this.soundEnabled.ToString());

        sb.Append("\n\n--- Tutorial ---");
        sb.Append("\nUnimpaired threshold: "  + this.dayZeroScoreUnimpaired.ToString());
        sb.Append("\nImpaired threshold: "    + this.dayZeroScoreImpaired.ToString());
        sb.Append("\nImpairments: "           + this.dayZeroImpairments.Count.ToString());
        
        for (int i = 0; i < this.dayZeroImpairments.Count; i++) {
            Impairment imp = this.dayZeroImpairments[i];
            sb.Append("\n -> Impairment " + (i+1).ToString() + " type: " + imp.getType().ToString());
            sb.Append("\n -> Impairment " + (i+1).ToString() + " strength: " + imp.getStrength().ToString("0.000"));
        }

        sb.Append("\n\n--- Structure ---");
        foreach (DayConfiguration d in this.dayConfigs) {
            sb.Append("\nDay " + d.getDayNumber().ToString());
            sb.Append("\n -> Duration: " + d.getDuration().ToString("0.0"));
            sb.Append("\n -> Ball value: " + d.getRewardMultiplier().ToString("0.0000"));
            sb.Append("\n -> Impairments: " + d.getImpairments().Length.ToString());
            
            int x = 1;
            foreach (Impairment i in d.getImpairments()) {
                sb.Append("\n ---> Impairment " + x.ToString() + " type: " + i.getType().ToString());
                sb.Append("\n ---> Impairment " + x.ToString() + " strength: " + i.getStrength().ToString("0.000"));
                x++;
            }

            Treatment dayTreatment = d.getTreatment();
            bool hasAny  = dayTreatment != null;
            sb.Append("\n -> Treatment: " + hasAny.ToString());
            if (hasAny) {
                bool hasPay  = dayTreatment != null && dayTreatment.hasPayOption();
                bool hasWait = dayTreatment != null && dayTreatment.hasWaitOption();
                sb.Append("\n ---> Pay treatment available: " + hasPay.ToString());
                sb.Append("\n ---> Wait treatment available: " + hasWait.ToString());
            }
        }

        sb.Append("\n\n--- Instructions ---");
        sb.Append("\n -> Day Zero Standard");
        sb.Append("\n" + locateBucketInstr.displayDuration.ToString("0.0") + "s: " + locateBucketInstr.message);
        sb.Append("\n" + holdContainerInstr.displayDuration.ToString("0.0") + "s: " + holdContainerInstr.message);
        sb.Append("\n" + fillInstr.displayDuration.ToString("0.0") + "s: " + fillInstr.message);
        sb.Append("\n" + goToSinkInstr.displayDuration.ToString("0.0") + "s: " + goToSinkInstr.message);
        sb.Append("\n" + pourBucketInstr.displayDuration.ToString("0.0") + "s: " + pourBucketInstr.message);

        sb.Append("\n -> Day Zero Impaired Shake/Fog/Generic -> Start/Explain");
        sb.Append("\n" + shakeImpairedRoundStart.displayDuration.ToString("0.0") + "s: " + shakeImpairedRoundStart.message);
        sb.Append("\n" + fogImpairedRoundStart.displayDuration.ToString("0.0") + "s: " + fogImpairedRoundStart.message);
        sb.Append("\n" + genericImpairedRoundStart.displayDuration.ToString("0.0") + "s: " + genericImpairedRoundStart.message);
        sb.Append("\n" + shakeImpairedRoundExplain.displayDuration.ToString("0.0") + "s: " + shakeImpairedRoundExplain.message);
        sb.Append("\n" + genericImpairedRoundExplain.displayDuration.ToString("0.0") + "s: " + genericImpairedRoundExplain.message);

        sb.Append("\n*****************************");
        Debug.Log(sb.ToString());
    }


    private bool parseSim() {
        
        foreach (string i in this.sim)
        {
            
            // ----------------------------------------------------------
            // Nolan April 30 2019
            // Adding configurable instructions to the simulation section
            // Configs will be formatted like:
            // Key:message,duration

            string cleanStr = i.Trim().ToUpper();
            string value, msg, dur;

            // Standard portion of day zero
            if (cleanStr.StartsWith(ConfigKeyword.LOCATE_BUCKET_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.locateBucketInstr.message = msg;
                this.locateBucketInstr.displayDuration = float.Parse(dur);
            } else if (cleanStr.StartsWith(ConfigKeyword.HOLD_BUCKET_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.holdContainerInstr.message = msg;
                this.holdContainerInstr.displayDuration = float.Parse(dur);
            } else if (cleanStr.StartsWith(ConfigKeyword.FILL_BUCKET_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.fillInstr.message = msg;
                this.fillInstr.displayDuration = float.Parse(dur);
            } else if (cleanStr.StartsWith(ConfigKeyword.LOCATE_SINK_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.goToSinkInstr.message = msg;
                this.goToSinkInstr.displayDuration = float.Parse(dur);
            } else if (cleanStr.StartsWith(ConfigKeyword.POUR_BUCKET_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.pourBucketInstr.message = msg;
                this.pourBucketInstr.displayDuration = float.Parse(dur);
            } 

            // Impaired portion of day zero
            else if (cleanStr.StartsWith(ConfigKeyword.START_FOG_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.fogImpairedRoundStart.message = msg;
                this.fogImpairedRoundStart.displayDuration = float.Parse(dur);
            } else if (cleanStr.StartsWith(ConfigKeyword.START_SHAKE_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.shakeImpairedRoundStart.message = msg;
                this.shakeImpairedRoundStart.displayDuration = float.Parse(dur);
            } else if (cleanStr.StartsWith(ConfigKeyword.START_GENERIC_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.genericImpairedRoundStart.message = msg;
                this.genericImpairedRoundStart.displayDuration = float.Parse(dur);
            } else if (cleanStr.StartsWith(ConfigKeyword.EXPLAIN_SHAKE_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.shakeImpairedRoundExplain.message = msg;
                this.shakeImpairedRoundExplain.displayDuration = float.Parse(dur);
            } else if (cleanStr.StartsWith(ConfigKeyword.EXPLAIN_GENERIC_INSTR)) {
                value   = i.Split(ConfigKeyword.COLON)[1].Trim();
                msg     = value.Split(ConfigKeyword.COMMA)[0].Trim();
                dur     = value.Split(ConfigKeyword.COMMA)[1].Trim();
                this.genericImpairedRoundExplain.message = msg;
                this.genericImpairedRoundExplain.displayDuration = float.Parse(dur);
            }

            // ----------------------------------------------------------

            else if (i.Contains(ConfigKeyword.NAME))
            {
                string[] split = i.Split(ConfigKeyword.COLON);
                this.name = split[1];
            }
            else if (i.Contains(ConfigKeyword.OUTPUT))
            {
                string[] split = i.Split(ConfigKeyword.COLON);
                this.output = split[1].Split(ConfigKeyword.COMMA);
            }
            else if (i.Contains(ConfigKeyword.DESCRIPTION))
            {
                string[] split = i.Split(ConfigKeyword.COLON);
                this.description = split[1];
            }
            else if (i.Contains(ConfigKeyword.INSTRUCTIONS))
            {
                string[] split = i.Split(ConfigKeyword.COLON);
                this.instructions = split[1].ToLower();
                if (this.instructions.Contains(ConfigKeyword.DISABLED.ToLower()))
                {
                    this.instructionsEnabled = false;
                }
                else
                {
                    this.instructionsEnabled = true;
                }
            }
            else if (i.Contains(ConfigKeyword.SOUND))
            {
                string[] split = i.Split(ConfigKeyword.COLON);
                this.sound = split[1].ToLower();
                if (this.sound.Contains(ConfigKeyword.DISABLED.ToLower()))
                {
                    this.soundEnabled = false;
                }
                else
                {
                    this.soundEnabled = true;
                }

            }
            else if (i.Contains(ConfigKeyword.SCENE))
            {
                string[] split = i.Split(ConfigKeyword.COLON);
                this.scene = split[1];
            }
        }



        return true;
    }

    /*
	* Parse the file and create objects for each parsed item
    */
    private bool parseConfig()
    {

        try
        {
            this.dayConfigs = new DayConfiguration[this.dayCount];
            int day = 0, trackDays = -1;
            Impairment.ImpairmentType impType = Impairment.ImpairmentType.NULL;
            bool isImp = false;
            bool isTreat = false;
            bool isWait = false;
            bool isCost = false;
            bool isEff = false;
            float dur = 0.00f, wait = 0.00f, certainty = 1.00f, strength = 0.00f;
            float cost_C = 0.00f, cost_a = 0.00f, cost_b = 0.00f, cost_c = 0.00f, wait_C = 0.00f, wait_a = 0.00f, wait_b = 0.00f, wait_c = 0.00f;
            float ballValue = 1.00f, probability = 1.00f, effect = 1.00f;
            List<Impairment> helperArray = new List<Impairment>();

            //    foreach (string i in this.dayList)
            for (int i = 0; i < this.dayList.Count; i++)
            {
                if (this.dayList[i].Contains(ConfigKeyword.DAY))
                {
                    string[] split = this.dayList[i].Split(ConfigKeyword.SPACE);
                    day = int.Parse(split[split.Length - 1]);
                    trackDays++;
                }

                else if (this.dayList[i].Contains(ConfigKeyword.DURATION))
                {
                    string time = this.dayList[i].Split(new char[] { ConfigKeyword.COLON }, 2)[1];
                    string[] split = time.Split(ConfigKeyword.COLON);
                    dur = float.Parse(split[0]) * 60 + float.Parse(split[1]);

                }

                else if (this.dayList[i].Contains(ConfigKeyword.BALLVALUE))
                {
                    string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                    ballValue = float.Parse(split[1]);

                }

                else if (this.dayList[i].Contains(ConfigKeyword.IMPAIRMENT))
                {
                    isImp = true; 
                    isTreat = false;
                    isWait = false;
                    isCost = false;
                    isEff = false;
                }

                else if (this.dayList[i].Contains(ConfigKeyword.TREATMENT))
                {
                    isTreat = true;
                    isImp = false;
                    isWait = false;
                    isCost = false;
                    isEff = false;
                }

                else if (this.dayList[i].Contains(ConfigKeyword.WAIT))
                {
                    isWait = true;
                    isImp = false;
                    isTreat = false;
                    isCost = false;
                    isEff = false;
                }

                else if (this.dayList[i].Contains(ConfigKeyword.COST))
                {
                    isCost = true;
                    isTreat = false;
                    isImp = false;
                    isWait = false;
                    isEff = false;
                }

                else if (this.dayList[i].Contains(ConfigKeyword.EFFECTIVENESS))
                {            
                    isEff = true;
                    isTreat = false;
                    isImp = false;
                    isWait = false;
                    isCost = false;
                }

                if (isImp)
                {
                    if ((this.dayList[i].ToLower()).Contains(ConfigKeyword.FOG_IMP.ToLower()))
                    {
                        impType = Impairment.ImpairmentType.VISUAL_FOG;
                    }
                    else if ((this.dayList[i].ToLower()).Contains(ConfigKeyword.SHAKE_IMP.ToLower()))
                    {
                        impType = Impairment.ImpairmentType.PHYSICAL_SHAKE; 
                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.STRENGTH))
                    {
                        string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                        string[] splitPercent = split[1].Split(ConfigKeyword.PERCENTAGE);
                        strength = ((float.Parse(splitPercent[0])) != 0) ? (float.Parse(splitPercent[0]) / 100) : (0.00f) ;
                    }
                }

                else if (isTreat)
                {
                     if (this.dayList[i].Contains(ConfigKeyword.CERTAINTY))
                    {
                        string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                        string[] splitPercent = split[1].Split(ConfigKeyword.PERCENTAGE);
                        certainty = ((float.Parse(splitPercent[0])) != 0) ? (float.Parse(splitPercent[0]) / 100) : (0.00f) ;
                    }
                }

                else if (isWait)
                {
                    if (this.dayList[i].Contains(ConfigKeyword.C))
                    {
                        string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                        wait_C = float.Parse(split[1]);
                    }
                    
                    else if (this.dayList[i].Contains(ConfigKeyword.a) && !this.dayList[i].Contains(ConfigKeyword.WAIT) && !this.dayList[i].Contains(ConfigKeyword.c) && !this.dayList[i].Contains(ConfigKeyword.b))
                    {
                        if ((this.dayList[i].ToLower()).Contains(ConfigKeyword.DEFAULT.ToLower()))
                        {
                            wait_a = 1.00f /(dur / 60.0f);

                        }

                        else
                        {
                            string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                            
                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            wait_a = float.Parse(split[1]);

                            //DataTable dt_wait_a = new DataTable ();
                            //wait_a = (float) dt_wait_a.Compute (split[1], "");
                            //Debug.Log("Evaluated wait_a: " + wait_a.ToString());
                        }
                    }

                    else if (this.dayList[i].Contains(ConfigKeyword.b))
                    {
                        if ((this.dayList[i].ToLower()).Contains(ConfigKeyword.DEFAULT.ToLower()))
                        {
                            wait_b = 2;

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(ConfigKeyword.COLON);

                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            wait_b = float.Parse(split[1]);

                            //DataTable dt_wait_b = new DataTable ();
                            //wait_b = (float) dt_wait_b.Compute (split[1], "");
                            //Debug.Log("Evaluated wait_b: " + wait_b.ToString());
                        }
                    }

                    else if (this.dayList[i].Contains(ConfigKeyword.c))
                    {
                        if ((this.dayList[i].ToLower()).Contains(ConfigKeyword.DEFAULT.ToLower()))
                        {
                            wait_c = dur / 60.0f;

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                            
                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            wait_c = float.Parse(split[1]);

                            //DataTable dt_wait_c = new DataTable ();
                            //wait_c = (float) dt_wait_c.Compute (split[1], "");
                            //Debug.Log("Evaluated wait_c: " + wait_c.ToString());
                        }
                    }
                }

                else if (isCost)
                {

                    if (this.dayList[i].Contains(ConfigKeyword.C) && this.dayList[i].Contains(ConfigKeyword.COST) == false)
                    {
                        string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                        cost_C = float.Parse(split[1]);
                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.a) && this.dayList[i].Contains(ConfigKeyword.c) == false && this.dayList[i].Contains(ConfigKeyword.b) == false)
                    {
                        if ((this.dayList[i].ToLower()).Contains(ConfigKeyword.DEFAULT.ToLower()))
                        {
                            cost_a = 1.00f / (dur / 60.0f);
                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                            
                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            cost_a = float.Parse(split[1]);

                            //DataTable dt_cost_a = new DataTable ();
                            //cost_a = (float) dt_cost_a.Compute (split[1], "");
                            //Debug.Log("Evaluated cost_a: " + cost_a.ToString());
                        }

                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.b))
                    {
                        if ((this.dayList[i].ToLower()).Contains(ConfigKeyword.DEFAULT.ToLower()))
                        {
                            cost_b = 2;
                        }

                        else
                        {
                            string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                            cost_b = float.Parse(split[1]);
                        }
                    }

                    else if (this.dayList[i].Contains(ConfigKeyword.c) )
                    {
                        if ((this.dayList[i].ToLower()).Contains(ConfigKeyword.DEFAULT.ToLower()))
                        {
                            cost_c = dur/60.0f;

                        }

                        else
                        {
                            string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                            cost_c = float.Parse(split[1]);
                        }
                    }
                }

                else if (isEff)
                {
                    if (this.dayList[i].Contains(ConfigKeyword.PROBABILITY))
                    {
                        string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                        string[] splitPercent = split[1].Split(ConfigKeyword.PERCENTAGE);
                        probability = ((float.Parse(splitPercent[0])) != 0) ? (float.Parse(splitPercent[0]) / 100) : (0.00f) ;
                    } 

                    else if (this.dayList[i].Contains(ConfigKeyword.EFFECT) && !this.dayList[i].Contains(ConfigKeyword.EFFECTIVENESS))
                    {
                        string[] split = this.dayList[i].Split(ConfigKeyword.COLON);
                        string[] splitPercent = split[1].Split(ConfigKeyword.PERCENTAGE);
                        effect = ((float.Parse(splitPercent[0])) != 0) ? (float.Parse(splitPercent[0]) / 100) : (0.00f) ;
                    }
                }

                if ((i + 1) == this.dayList.Count || (trackDays >= 0 && this.dayList[i + 1].Contains(ConfigKeyword.DAY)))
                {
                    Impairment impairObj;
                    Impairment[] dayImpairs;
                    Treatment dayTreats;

                    if (impType == Impairment.ImpairmentType.NULL)
                    {
                        dayImpairs = new Impairment[0];
                    }

                    else
                    {
                        impairObj = new Impairment(impType, strength);
                        helperArray.Add(impairObj);
                        dayImpairs = helperArray.ToArray();
                    }

                    if (cost_C != 0.00f && wait_C != 0)
                    {
                        dayTreats = new Treatment(
                          cost_C,
                          cost_a,
                          cost_b,
                          cost_c,
                          wait_C,
                          wait_a,
                          wait_b,
                          wait_c,
                          probability,
                          effect,
                          0.0f,
                          0.0f,
                          0.0f);
                    }

                    else if(cost_C == 0.00f && cost_a == 0.00f && cost_b == 0.00f && cost_c == 0.00f && wait_C != 0.00f)
                    {
                        dayTreats = new Treatment(
                          Treatment.NONE,
                          Treatment.NONE,
                          Treatment.NONE,
                          Treatment.NONE,
                          wait_C,
                          wait_a,
                          wait_b,
                          wait_c,
                          probability,
                          effect,
                          0.0f,
                          0.0f,
                          0.0f);
                    }

                    else if (cost_C != 0.00f && wait_C == 0.00f && wait_a == 0.00f && wait_b == 0.00f && wait_c == 0.00f)
                    {
                        dayTreats = new Treatment(
                          cost_C,
                          cost_a,
                          cost_b,
                          cost_c,
                          Treatment.NONE,
                          Treatment.NONE,
                          Treatment.NONE,
                          Treatment.NONE,
                          probability,
                          effect,
                          0.0f,
                          0.0f,
                          0.0f);
                    }

                    else 
                    {
                        dayTreats = null;
                    }

                    this.dayConfigs[trackDays] = new DayConfiguration(day, dur, dayImpairs, dayTreats, ballValue);
                    isImp = false;
                    isTreat = false;
                    isCost = false;
                    isWait = false;
                    isEff = false;
                    impType = Impairment.ImpairmentType.NULL;
                    helperArray.Clear();
                    dur = 0.00f; ballValue = 1.00f; wait = 0.00f; certainty = 1.00f; strength = 0.00f; probability = 1.00f; effect = 1.00f ;
                    cost_C = 0.00f; cost_a = 0.00f; cost_b = 0.00f; cost_c = 0.00f; wait_C = 0.00f; wait_a = 0.00f; wait_b = 0.00f; wait_c = 0.00f;
                }
            }

            return true;
        }

        catch (UnauthorizedAccessException uae)
        {
            Debug.Log("Parsing exception: UnauthorizedAccessException");
        }

        catch (System.IO.DirectoryNotFoundException dnfe)
        {
            Debug.Log("Parsing exception: DirectoryNotFoundException");
        }

        catch (System.IO.IOException ioe)
        {
            Debug.Log("Parsing exception: IOException");
        }

        catch (Exception e)
        {
            Debug.Log("Parsing exception: Exception");
        }

        return false;
    }


    /*
    * Take measures to reduce nausea, e.g. put curtains up on windows
    */
    public bool lowNauseaModeEnabled()
    {
        return this.lowNauseaMode;
    }


    public bool claustrophobicModeEnabled()  {
        return this.claustrophobicMode;
    }


    /*
    * Returns all configurations; either parsed from file or default
    */
    public DayConfiguration[] getConfigs()
    {
        return this.dayConfigs;
    }


    public string getSimName()
    {
        return this.name;
    }


    public string[] getSimOutput()
    {
        return this.output;
    }


    public string getSimDescription()
    {
        return this.description;
    }


    public bool getSimInstruction()
    {
        return instructionsEnabled;
    }


    public bool getSimSound()
    {
        return soundEnabled;
    }


    public string getSimScene()
    {
        return this.scene;
    }


    public float getBallValue()
    {
        return this.ballValue;
    }


    /*
    * Nolan April 2019
    * Updating this method to return an array instead of a single score
    * The array is indexed for impaired and unimpaired day 0 thresholds
    * so that a second round of training can be used if desired.
    */
    public float [] getDayZeroScore()
    {
        if(this.dayZeroScoreUnimpaired == 0)
            this.dayZeroScoreUnimpaired = DAY_0_DEFAULT_SCORE;

        if (this.dayZeroScoreImpaired == 0) 
            this.dayZeroScoreImpaired = DAY_0_DEFAULT_SCORE;

        float [] r      = new float[2];
        r[IMPAIRED]     = dayZeroScoreImpaired;
        r[UNIMPAIRED]   = dayZeroScoreUnimpaired;

        return r;
    }


    /*
    * Fetches list of all impairments active during the second round
    * of day 0 (impaired training)
    */
    public List<Impairment> getDayZeroImpairments () {
        return this.dayZeroImpairments;
    }


    public string dbConn()
    {
        return this.dbConnection;
    }


    public int numDays()
    {
        return (this.dayConfigs != null) ? (this.dayConfigs.Length) : (-1);
    }
}
