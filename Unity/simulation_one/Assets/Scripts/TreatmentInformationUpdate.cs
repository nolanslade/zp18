using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreatmentInformationUpdate : MonoBehaviour {

    public GameObject treatment;
    public GameObject currentCost;
    public GameObject simManager;

    private SimManager simManagerComponent;
    private UnityEngine.UI.Text currentCostComp;
    private Treatment treatmentComp;

    // Only update every set duration, instead of every frame.
    public float customRefreshRate;
    private float elapsed;


    void Start()
    {
        this.simManagerComponent = simManager.GetComponent<SimManager>();
        this.treatmentComp = treatment.GetComponent<Treatment>();
        this.currentCostComp = currentCost.GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;

        // Reducing frame-by-frame operations
        if (elapsed > customRefreshRate)
        {
            float t = simManagerComponent.getElapsedDayTime();
            currentCostComp.text = "Treatment Cost: $" + treatmentComp.currentCost(t).ToString();
        }

    }
}
