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

    // Track positions of the pill bottles to trigger obtain attempts
    // and subsequent repositioning if they can't afford it, etc
    private const float MOVE_THRESHOLD  = 0.01f;
    private const float NULL_POS        = -9999.99f;
    private Vector3 waitBottlePositionA = new Vector3 (NULL_POS, NULL_POS, NULL_POS);
    private Vector3 payBottlePositionA  = new Vector3 (NULL_POS, NULL_POS, NULL_POS);
    private Vector3 waitBottlePositionB = new Vector3 (NULL_POS, NULL_POS, NULL_POS);
    private Vector3 payBottlePositionB  = new Vector3 (NULL_POS, NULL_POS, NULL_POS);

    public float payBottleInitXPosition;
    public float payBottleInitYPosition;
    public float payBottleInitZPosition;
    public float waitBottleInitXPosition;
    public float waitBottleInitYPosition;
    public float waitBottleInitZPosition;

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
    private bool attemptObtain (TreatmentObtainType t) {

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
            Debug.Log("Invalid treatment obtain attempt. Resetting " + t.ToString() + " bottle position.");
            if (t == TreatmentObtainType.PAY) {
                payPill.SetActive(false); payPill.SetActive(true);      // THIS DOESNT WORK - NEED ANOTHER WAY TO RESET IT
                payPill.transform.position = new Vector3 (
                    payBottleInitXPosition,
                    payBottleInitYPosition,
                    payBottleInitZPosition
                ); payBottlePositionA = payPill.transform.position;
            }
            else if (t == TreatmentObtainType.WAIT) {
                waitPill.SetActive(false); waitPill.SetActive(true);    // THIS DOESNT WORK - NEED ANOTHER WAY TO RESET IT
                waitPill.transform.position = new Vector3 (
                    waitBottleInitXPosition,
                    waitBottleInitYPosition,
                    waitBottleInitZPosition
                ); waitBottlePositionA = waitPill.transform.position;
            }
            else {
                Debug.Log("Unrecognized bottle type. Not resetting position.");
            }
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

        // Start tracking both pill positions
        payPill.transform.position = new Vector3 (payBottleInitXPosition, payBottleInitYPosition, payBottleInitZPosition);
        payBottlePositionA = payPill.transform.position;
        Debug.Log("Pay bottle instantiated at pos: " + payBottlePositionA.x.ToString() + "," + payBottlePositionA.y.ToString() + "," + payBottlePositionA.z.ToString());

        waitPill.transform.position = new Vector3 (waitBottleInitXPosition, waitBottleInitYPosition, waitBottleInitZPosition);
        waitBottlePositionA = waitPill.transform.position;
        Debug.Log("Wait bottle instantiated at pos: " + waitBottlePositionA.x.ToString() + "," + waitBottlePositionA.y.ToString() + "," + waitBottlePositionA.z.ToString());

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

            // Track the pay bottle's position
            payPill.transform.position = new Vector3 (payBottleInitXPosition, payBottleInitYPosition, payBottleInitZPosition);
            payBottlePositionA = payPill.transform.position;
            Debug.Log("Pay bottle instantiated at pos: " + payBottlePositionA.x.ToString() + "," + payBottlePositionA.y.ToString() + "," + payBottlePositionA.z.ToString());
        } 

        else if (panelType == TreatmentObtainType.WAIT) {
            treatmentDay = true;
            justWait = true;
            waitPedestal.SetActive(true);
            waitPill.SetActive(true);
            waitPanel.SetActive(true);
            treatmentInformationPanel.SetActive(true);

            // Track the wait bottle's position
            waitPill.transform.position = new Vector3 (waitBottleInitXPosition, waitBottleInitYPosition, waitBottleInitZPosition);
            waitBottlePositionA = waitPill.transform.position;
            Debug.Log("Wait bottle instantiated at pos: " + waitBottlePositionA.x.ToString() + "," + waitBottlePositionA.y.ToString() + "," + waitBottlePositionA.z.ToString());
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
        payBottlePositionA = new Vector3 (NULL_POS, NULL_POS, NULL_POS);
        payBottlePositionB = new Vector3 (NULL_POS, NULL_POS, NULL_POS);
        waitBottlePositionA = new Vector3 (NULL_POS, NULL_POS, NULL_POS);
        waitBottlePositionB = new Vector3 (NULL_POS, NULL_POS, NULL_POS);
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

                // Pay obtain attempts
                if (payBottlePositionA.x != NULL_POS) {
                    payBottlePositionB = payPill.transform.position;
                    if ((System.Math.Abs(payBottlePositionA.x - payBottlePositionB.x) > MOVE_THRESHOLD) || 
                        (System.Math.Abs(payBottlePositionA.y - payBottlePositionB.y) > MOVE_THRESHOLD) ||
                        (System.Math.Abs(payBottlePositionA.z - payBottlePositionB.z) > MOVE_THRESHOLD)) {
                        Debug.Log("Detected pay bottle moved - attempting obtain...");
                        Debug.Log("Position A at move detection: (" + payBottlePositionA.x + "," + payBottlePositionA.y + "," + payBottlePositionA.z + ")");
                        Debug.Log("Position B at move detection: (" + payBottlePositionB.x + "," + payBottlePositionB.y + "," + payBottlePositionB.z + ")");
                        bool obtainResult = attemptObtain(TreatmentObtainType.PAY);
                        Debug.Log("PAY OBTAIN SUCCESS: " + obtainResult.ToString());
                    } else {
                        payBottlePositionA = payPill.transform.position;
                    }
                }

                // Wait obtain attempts
                if (waitBottlePositionA.x != NULL_POS) {
                    waitBottlePositionB = waitPill.transform.position;
                    if ((System.Math.Abs(waitBottlePositionA.x - waitBottlePositionB.x) > MOVE_THRESHOLD) ||
                        (System.Math.Abs(waitBottlePositionA.y - waitBottlePositionB.y) > MOVE_THRESHOLD) ||
                        (System.Math.Abs(waitBottlePositionA.z - waitBottlePositionB.z) > MOVE_THRESHOLD)) {
                        Debug.Log("Detected wait bottle moved - attempting obtain...");
                        Debug.Log("Position A at move detection: (" + waitBottlePositionA.x + "," + waitBottlePositionA.y + "," + waitBottlePositionA.z + ")");
                        Debug.Log("Position B at move detection: (" + waitBottlePositionB.x + "," + waitBottlePositionB.y + "," + waitBottlePositionB.z + ")");
                        bool obtainResult = attemptObtain(TreatmentObtainType.WAIT);
                        Debug.Log("WAIT OBTAIN SUCCESS: " + obtainResult.ToString());
                    } else {
                        waitBottlePositionA = waitPill.transform.position;
                    }
                }

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
