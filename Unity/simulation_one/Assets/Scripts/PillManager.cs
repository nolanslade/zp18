using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillManager : MonoBehaviour {

    public GameObject simManager;
    public GameObject treatmentUI;
    public GameObject payPill;
    public GameObject waitPill;

    private SimManager simManagerComponent;
    private bool treatmentTaken;
    
    //private UnityEngine.UI.Text currentCostComp;

    // Only update every set duration, instead of every frame.
    public float customRefreshRate;
    private float elapsed;


    void Start()
    {
        this.simManagerComponent = simManager.GetComponent<SimManager>();
       // this.currentCostComp = currentCost.GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;

        // Reducing frame-by-frame operations
        if (elapsed > customRefreshRate)
        {
            //currentCostComp.text = "Treatment Cost: $" + simManagerComponent.getCurrentTreatmentCost().ToString();
        }

    }
}
