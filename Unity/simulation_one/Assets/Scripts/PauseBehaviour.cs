using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseBehaviour : MonoBehaviour {

    public bool bPaused = false;
    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Collision Detected");
        if (col.gameObject.name == "Virtual_Hand_Left" || col.gameObject.name == "Virtual_Hand_Right")
        {
            if (bPaused) bPaused = false;
            else bPaused = true;
            Debug.Log("Click " + bPaused);
        }
    }

}
