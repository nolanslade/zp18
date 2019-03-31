using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
* McDSL: VR Simulation One
* Nolan Slade
* March 7 2019
*
* Manage instruction messages displayed on the HUD.
*/
public class InstructionManager : MonoBehaviour {

    public GameObject instrPanel;
    public GameObject instrText;
    private UnityEngine.UI.Text instrTextComponent;
    private float currentMsgTimeRemaining;
    private bool instructionsEnabled = true;

    // Use this for initialization
    void Start () {
        this.instrTextComponent = instrText.GetComponent<UnityEngine.UI.Text>();
        currentMsgTimeRemaining = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
		if (currentMsgTimeRemaining > 0.0f) {
            currentMsgTimeRemaining -= Time.deltaTime;
            if (currentMsgTimeRemaining < 0.0f) {
                currentMsgTimeRemaining = 0.0f;
                instrPanel.SetActive(false);
            }
        }
	}

    public void setInstructionsDisable()
    {
        this.instructionsEnabled = false;
    }

    /*
     * Enables the instruction panel and displays a message for a set duration
     * then disables both the message and panel.
     */
    public void setTemporaryMessage (string message, float displayDuration) {
        this.instrTextComponent.text = message;
        this.currentMsgTimeRemaining = displayDuration;
        instrPanel.SetActive(instructionsEnabled);
    }

    /*
    * As above, except captures the required parameters inside 
    * a single Instruction object rather than individually
    */
    public void setTemporaryMessage (Instruction instruction) {
        this.instrTextComponent.text = instruction.message;
        this.currentMsgTimeRemaining = instruction.displayDuration;
        instrPanel.SetActive(instructionsEnabled);
    }
}
