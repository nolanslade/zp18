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

    //private UnityEngine.UI.Text currentCostComp;

    // Only update every set duration, instead of every frame.
    public float customRefreshRate;
    private float elapsed;

    private float treatmentCost;
    private float treatmentWait;

    private bool treatmentDay;


    void Start()
    {
        Debug.Log("starting pill manager ");
        this.simManagerComponent = simManager.GetComponent<SimManager>();
        this.payTextComp = payText.GetComponent<UnityEngine.UI.Text>();
        this.waitTextComp = waitText.GetComponent<UnityEngine.UI.Text>();
        treatmentDay = false;
    }

    public void activatePanels()
    {
        treatmentDay = true;
        Debug.Log("Enabling pill man ");
        payPanel.SetActive(true);
        waitPanel.SetActive(true);
        payPill.SetActive(true);
        waitPill.SetActive(true);
        payPedestal.SetActive(true);
        waitPedestal.SetActive(true);
        treatmentInformationPanel.SetActive(true);
    }

    public void disablePanels()
    {
        treatmentDay = true;
        Debug.Log("disabling pill man ");
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
                treatmentCost = simManagerComponent.getCurrentTreatmentCost();
                treatmentWait = simManagerComponent.getCurrentTreatmentWaitTime();
                //Update UIs
                payTextComp.text = "$ " + treatmentCost.ToString("0.00");
                waitTextComp.text = treatmentWait.ToString() + " s.";

            }
        }
    }
}
