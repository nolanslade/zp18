using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PillManager : MonoBehaviour {

    public GameObject simManager;
    public GameObject payPanel;
    public GameObject waitPanel;
    public GameObject payText;
    public GameObject waitText;
    public GameObject treatmentInformationPanel;
    public GameObject waitPill;
    public GameObject waitPedestal;
    public GameObject payPill;
    public GameObject payPedestal;

    private SimManager simManagerComponent;
    private UnityEngine.UI.Text payTextComp;
    private UnityEngine.UI.Text waitTextComp;

    // Only update every set duration, instead of every frame.
    public float customRefreshRate;
    private float elapsed;

    private float treatmentCost;
    private float treatmentWait;

    private bool treatmentDay;
    private bool justPay;
    private bool justWait;


    public enum TreatmentObtainType 
    {
        PAY,
        WAIT
    }


    void Start()
    {
        this.simManagerComponent = simManager.GetComponent<SimManager>();
        this.payTextComp = payText.GetComponent<UnityEngine.UI.Text>();
        this.waitTextComp = waitText.GetComponent<UnityEngine.UI.Text>();
        treatmentDay = false;
        justWait = false;
        justPay = false;
    }


    /*
    * Attempt to make a transaction - validate that the participant
    * can afford it, and then call out as necessary to apply changes
    * to the environment to reflect the transaction.
    */
    public bool attemptObtain (TreatmentObtainType t) {

        float effectiveWaitTime = -1.0f;     // Seconds
        float effectiveCost     = -1.0f;     // Lab dollars
        bool transactionValid   = false;

        SimManager.GameState currentSimState = simManagerComponent.currentState();

        if (t == TreatmentObtainType.PAY) {
            effectiveCost    = simManagerComponent.getCurrentTreatmentCost();
            transactionValid = (
                currentSimState == SimManager.GameState.RUNNING 
                    && simManagerComponent.getCurrentScore() > effectiveCost
            );
        } 

        else if (t == TreatmentObtainType.WAIT) {
            effectiveWaitTime = simManagerComponent.getCurrentTreatmentWaitTime();
            transactionValid  = currentSimState == SimManager.GameState.RUNNING;
        }


        // Only apply changes to the environment if the transaction was approved
        if (transactionValid) {
            disablePanels();
            simManagerComponent.determinePostTreatmentActions(t, effectiveCost, effectiveWaitTime);
        } else {
            Debug.Log("Invalid treatment obtain attempt.");
        }

        return transactionValid;
    }


    /*
    * Activates both the panels for wait and pay options
    */
    public void activatePanels()
    {
        treatmentDay = true;
        payPanel.SetActive(true);
        waitPanel.SetActive(true);
        payPill.SetActive(true);
        waitPill.SetActive(true);
        payPedestal.SetActive(true);
        waitPedestal.SetActive(true);
        treatmentInformationPanel.SetActive(true);
    }


    /*
    * Activates only the components corresponding to the desired
    * treatment type, PAY or WAIT
    */
    public void activatePanel (TreatmentObtainType panelType) {
        
        if (panelType == TreatmentObtainType.PAY) {
            treatmentDay = true;
            justPay = true;
            payPedestal.SetActive(true);
            payPill.SetActive(true);
            payPanel.SetActive(true);
            treatmentInformationPanel.SetActive(true);
        } 

        else if (panelType == TreatmentObtainType.WAIT) {
            treatmentDay = true;
            justWait = true;
            waitPedestal.SetActive(true);
            waitPill.SetActive(true);
            waitPanel.SetActive(true);
            treatmentInformationPanel.SetActive(true);
        }
    }


    /*
    * Disables all panels related to treatment
    */
    public void disablePanels()
    {
        treatmentDay = false;
        justWait = false;
        justPay = false;
        payPanel.SetActive(false);
        waitPanel.SetActive(false);
        payPill.SetActive(false);
        waitPill.SetActive(false);
        payPedestal.SetActive(false);
        waitPedestal.SetActive(false);
        treatmentInformationPanel.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        if (treatmentDay)
        {
            elapsed += Time.deltaTime;

            // Reducing frame-by-frame operations
            if (elapsed > customRefreshRate)
            {
                if (!justWait) {
                    treatmentCost = simManagerComponent.getCurrentTreatmentCost();
                    payTextComp.text = "$ " + treatmentCost.ToString("0.00");
                }
                
                if (!justPay) {
                    treatmentWait = simManagerComponent.getCurrentTreatmentWaitTime();
                    waitTextComp.text = treatmentWait.ToString() + " s.";
                }

                elapsed = 0.0f;
            }
        }
    }
}
