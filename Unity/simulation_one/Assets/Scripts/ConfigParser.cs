using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * McDSL: VR Simulation One
 * Nolan Slade
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
public class ConfigParser {

    // Parsing
    private string configFilePath;
    private string dbConnection;
    private int dayCount;                               //total number of days in the config file, used for parseConfigs()
    private List<string> dayList = new List<string>();  //contains all day information ready to be split into dayConfigs

    // Simulation parameters
    private DayConfiguration [] dayConfigs = null;
    private bool lowNauseaMode;
    private bool instructionsEnabled;
    
    /*
    * If not using a config file - use these default values (for testing)
    *
    * Structure (3 days):
    * - Day one: no impairment, 1 minute, 2* multiplier
    * - Day two: speed penalty (pay 50 / wait 15s), 2 minutes
    * - Day three: speed penalty, fog (fog @ 80%, pay 100 / wait 25s), 2 minutes
    */
    public ConfigParser () {

        // Metrics
        // TODO ... db stuff here
        dbConnection = null;

        // Data comes from static class, set by the main menu
        lowNauseaMode = ParticipantData.nauseaSensitive;
        instructionsEnabled = true;

        /*  
        // Treatment argument ordering:
        float c_C,             // Cost function
        float c_a, 
        float c_b, 
        float c_c,             
        
        float w_C,             // Wait function
        float w_a, 
        float w_b, 
        float w_c,
        
        float effProb,         // Probability that treatment will actually work
        float eff,             // The percentage (0.0 to 1.0) that the treatment will take away, if it is effective
        float delProb,         // Delay Penalty 
        float del,             // "
        float deathProb        // Death Probability
        // ... Additional parameters here
        */

        // Day One
        Debug.Log("Setting Day One");
        Impairment [] dayOneImps = new Impairment [0];
        Treatment dayOneTreatment = null;   // TODO
        float dayOneDur = 120.0f;
        float dayOneMult = 2.0f;
        int dayOne = 1;

        // Day Two
        Debug.Log("Setting Day Two");
        float dayTwoDur = 120.0f;
        Impairment [] dayTwoImps = new Impairment [1];
        Treatment dayTwoTreatment = null;
        dayTwoImps [0] = new Impairment (Impairment.ImpairmentType.PHYSICAL_SHAKE, 0.6f);
        int dayTwo = 2;

        // Day Three
        Debug.Log("Setting Day Three");
        float dayThreeDur = 120.0f;
        Impairment [] dayThreeImps = new Impairment [1];
        Treatment dayThreeTreatment = new Treatment (
            100.0f,
            0.15f,
            3.0f,
            120.0f,
            100.0f,
            0.15f,
            3.0f,
            120.0f,
            1.0f,
            1.0f,
            0.0f,
            0.0f,
            0.0f
        );
        dayThreeImps[0] = new Impairment (Impairment.ImpairmentType.PHYSICAL_SHAKE, 0.6f);
        int dayThree = 3;


        // Set each day configuration with the above
        Debug.Log("Finalizing setup...");
        this.dayConfigs = new DayConfiguration [3];
        this.dayConfigs [0] = new DayConfiguration (dayOne, dayOneDur, dayOneImps, dayOneTreatment, dayOneMult);
        this.dayConfigs [1] = new DayConfiguration (dayTwo, dayTwoDur, dayTwoImps, dayTwoTreatment);
        this.dayConfigs [2] = new DayConfiguration (dayThree, dayThreeDur, dayThreeImps, dayThreeTreatment);
        Debug.Log("Config set up complete.");
    }


    /*
    * If using a configuration file, then load in day configurations
    * by parsing in the file if the path argument is valid.
    */
    public ConfigParser (string path) {

        lowNauseaMode = ParticipantData.nauseaSensitive;

        // TODO

        // NEED CONFIG OPTION FOR THIS
        instructionsEnabled = true;

        // ---------

        this.configFilePath = path;
        dbConnection = null;
        List<string> sim = new List<string>();          //contains all info about the simulation itself
        List<string> dayByLine = new List<string>();    //each line in a list for day portion of file
        StreamReader reader = new StreamReader(this.configFilePath);
        string line = reader.ReadLine();
        char[] delimiter = { '\n', ':', '\t', '#', ' ', '%' }; //splitting a string requires chars therefore ConfigKeyword class cannot be used

        this.dayCount = 0;
        bool isSim = false;
        bool isDay = false;
        while (line != null)
        {
            string[] fields = line.Split(delimiter[0]);

            //splits information into 2 groups, Simulation and Day
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Contains(ConfigKeyword.SIMULATION))
                {
                    isSim = true;
                    isDay = false;
                    sim.Add(fields[i]);
                }
               // currently the day group is further split into it's separate lines for better parsing 
                else if (fields[i].Contains(ConfigKeyword.DAY))
                {
                    this.dayCount++;
                    dayByLine.Add("Day " + dayCount.ToString());
                    isSim = false;
                    isDay = true;
                }


                if ((fields[i].Contains(ConfigKeyword.INDENT)) && isSim)
                {
                    sim.Add(fields[i].Replace(ConfigKeyword.INDENT, ""));
                }
                else if ((fields[i].Contains(ConfigKeyword.INDENT)) && isDay)
                {
                    dayByLine.Add(fields[i].Replace(ConfigKeyword.INDENT, ""));

                }

            }

            line = reader.ReadLine();
        }


        //gets rid of the comments (#)
        foreach (string i in dayByLine)
        {
            string[] split = i.Split(delimiter[3]);
            dayList.Add(split[0]);

        }


        /*Debug.Log("DAYS " + dayCount);
          foreach(string i in dayList){
            Debug.Log(i);
          }*/

        parseConfig();
        // TODO - parse the file (if it exists) and load in day configurations
    }


    /*
	* Parse the file and create objects for each parsed item
    *
    * *** TODO ***
    *
    */
    private bool parseConfig () {

        try
        {
            Debug.Log("PARSING FILE");
            this.dayConfigs = new DayConfiguration[this.dayCount];
            int day = 0, trackDays = -1, impair = -1, countCost = 0;
            bool isImp = false;
            bool isTreat = false;
            bool isCost = false;
            float dur = 0.00f, wait = 0.00f, certainty = 0.00f, factor = 0.00f, C = 0.00f, a = 0.00f, b = 0.00f, c = 0.00f;
            char[] delimiter = { '\n', ':', '\t', '#', ' ', '%'};
            List<Impairment> helperArray = new List<Impairment>();

     //    foreach (string i in this.dayList)
        for(int i = 0; i < this.dayList.Count; i++)
            {
                if (trackDays >= 0 && this.dayList[i].Contains(ConfigKeyword.DAY))
                {
                    Impairment impairObj;
                    Impairment[] dayImpairs;
                    Treatment dayTreats; //

                    if (impair == -1)
                    {
                        // impairObj = new Impairment();
                        // helperArray.Add(impairObj);
                        dayImpairs = new Impairment[trackDays];
                      //  Debug.Log("NO IMPAIRS");
                    }
                    else
                    {
                        impairObj = new Impairment((Impairment.ImpairmentType)impair, factor);

                        helperArray.Add(impairObj);

                        dayImpairs = helperArray.ToArray();
                    }

                    if (C != 0.00f)
                    {
                        dayTreats = new Treatment(
                          C,
                          a,
                          b,
                          c,

                            // TODO *********************
                            0.0f, 0.0f, 0.0f, 0.0f,

                          1.0f,
                          1.0f,
                          0.0f,
                          0.0f,
                          0.0f);
                    }
                    else
                    {
                        dayTreats = null;
                       // Debug.Log("NO TREATS");
                    }



                    this.dayConfigs[trackDays] = new DayConfiguration(day, dur, dayImpairs, dayTreats);
                    isImp = false;
                    isTreat = false;
                    isCost = false;
                    countCost = 0;
                    impair = -1;
                    dur = 0.00f; wait = 0.00f; certainty = 0.00f; factor = 0.00f; C = 0.00f; a = 0.00f; b = 0.00f; c = 0.00f;
                }
                if (this.dayList[i].Contains(ConfigKeyword.DAY))
                {
                    //  Debug.Log("Day");
                    string[] split = this.dayList[i].Split(delimiter[4]);
                    day = int.Parse(split[split.Length - 1]);
                  
                    trackDays++;
                }
                else if (this.dayList[i].Contains(ConfigKeyword.DURATION))
                {
                    string time = this.dayList[i].Split(new char[] { ':' }, 2)[1];
                    string[] split = time.Split(delimiter[1]);
                    dur = float.Parse(split[0])*60 + float.Parse(split[1]);
                    Debug.Log("DUR " + time);
                   
                }
                else if (this.dayList[i].Contains(ConfigKeyword.IMPAIRMENT))
                {
                    isImp = true;
                    isTreat = false;
                    isCost = false;
                }
                else if (this.dayList[i].Contains(ConfigKeyword.TREATMENT))
                {
                    isTreat = true;
                    isImp = false;
                    isCost = false;
                }
                else if (this.dayList[i].Contains(ConfigKeyword.COST))
                {
                    isCost = true;
                    isTreat = false;
                    isImp = false;
                }
                if (isImp)
                {
                    if (this.dayList[i].Contains("Fog"))
                    {
                        impair = 0;

                    }
                    else if (this.dayList[i].Contains("Gravity"))
                    {
                        impair = 1;

                    }
                    else if (this.dayList[i].Contains("Shake"))
                    {
                        impair = 2;
                        Debug.Log("SHAKE");
                    }
                    else if (this.dayList[i].Contains("Speed"))
                    {
                        impair = 3;

                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.FACTOR))
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        string[] splitPercent = split[1].Split(delimiter[5]);
                        factor = float.Parse(splitPercent[0]) / 100;

                    }

                }
                else if (isTreat)
                {
                    if (this.dayList[i].Contains(ConfigKeyword.WAIT))
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        wait = float.Parse(split[1]);

                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.CERTAINTY))
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        string[] splitPercent = split[1].Split(delimiter[5]);
                        certainty = float.Parse(splitPercent[0]) / 100;

                    }


                }
                else if (isCost)
                {

                    if (this.dayList[i].Contains(ConfigKeyword.C) && this.dayList[i] != ConfigKeyword.COST)
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        C = float.Parse(split[1]);
                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.a))
                    {
                        if (this.dayList[i].Contains("default"))
                        {
                            a = 1.00f;

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(delimiter[1]);
                            a = float.Parse(split[1]);

                        }

                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.b))
                    {
                        if (this.dayList[i].Contains("default"))
                        {
                            b = 1.00f;

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(delimiter[1]);
                            b = float.Parse(split[1]);

                        }
                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.c) && this.dayList[i] != ConfigKeyword.COST)
                    {
                        if (this.dayList[i].Contains("default"))
                        {
                            c = 1.00f;

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(delimiter[1]);
                            c = float.Parse(split[1]);

                        }

                    }

                    countCost++;
                    Debug.Log("COUNT " + countCost);
                }

                


            }
            return true;
        }

        catch (UnauthorizedAccessException uae) {
        	return false;
        }

        catch (System.IO.DirectoryNotFoundException dnfe) {
            return false;
        }

        catch (System.IO.IOException ioe) {
        	return false;
        }

        catch (Exception e) {
        	return false;
        }

        return false;
    }


    /*
    * Take measures to reduce nausea, e.g. put curtains up on windows
    */
    public bool lowNauseaModeEnabled () {
        return this.lowNauseaMode;
    }

    /*
    * Returns all configurations; either parsed from file or default
    */
    public DayConfiguration [] getConfigs () {
        return this.dayConfigs;
    }

    public string dbConn () {
        return this.dbConnection;
    }

    public int numDays () {
        if (this.dayConfigs != null) {
            return this.dayConfigs.Length;
        } else {
            return -1;
        }
    }
}