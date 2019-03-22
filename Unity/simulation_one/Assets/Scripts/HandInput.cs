 using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class HandInput : MonoBehaviour
{

    private Hand hand;
    private SteamVR_Action_Boolean m_GrabAction = null;
    private SteamVR_Behaviour_Pose m_Pose = null;
    private FixedJoint m_Joint = null;

    private Interactable m_CurrentInteractable = null;
    public List<Interactable> m_ContactInteractables = new List<Interactable>();

    // Use this for initialization
    void Start()
    {
        hand = gameObject.GetComponent<Hand>();
    }
    private void Awake()
    {
        m_Pose = GetComponent<SteamVR_Behaviour_Pose>();
        m_Joint = GetComponent<FixedJoint>();

    }
    private void Update()
    {
        if(m_GrabAction.GetStateDown(m_Pose.inputSource))
        {
            PickUp();
        }
        if (m_GrabAction.GetStateDown(m_Pose.inputSource))
        {
            Drop();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Interactable"))
            return;
        m_ContactInteractables.Add(other.gameObject.GetComponent<Interactable>());
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Interactable"))
            return;
        m_ContactInteractables.Remove(other.gameObject.GetComponent<Interactable>());
    }
    public Vector2 getTrackPadPos()
    {
        SteamVR_Action_Vector2 trackpadPos = SteamVR_Input._default.inActions.touchPos;
        return trackpadPos.GetAxis(hand.handType);
    }

    public bool getPinch()
    {
        return SteamVR_Input._default.inActions.GrabPinch.GetState(hand.handType);
    }

    public bool getPinchDown()
    {
        return SteamVR_Input._default.inActions.GrabPinch.GetStateDown(hand.handType);
    }

    public bool getPinchUp()
    {
        return SteamVR_Input._default.inActions.GrabPinch.GetStateUp(hand.handType);
    }

    public bool getGrip()
    {
        return SteamVR_Input._default.inActions.GrabGrip.GetState(hand.handType);
    }

    public bool getGrip_Down()
    {
        return SteamVR_Input._default.inActions.GrabGrip.GetStateDown(hand.handType);
    }

    public bool getGrip_Up()
    {
        return SteamVR_Input._default.inActions.GrabGrip.GetStateUp(hand.handType);
    }

    public bool getMenu()
    {
        return SteamVR_Input._default.inActions.MenuButton.GetState(hand.handType);
    }

    public bool getMenu_Down()
    {
        return SteamVR_Input._default.inActions.MenuButton.GetStateDown(hand.handType);
    }

    public bool getMenu_Up()
    {
        return SteamVR_Input._default.inActions.MenuButton.GetStateUp(hand.handType);
    }

    public bool getTouchPad()
    {
        return SteamVR_Input._default.inActions.Teleport.GetState(hand.handType);
    }

    public bool getTouchPad_Down()
    {
        return SteamVR_Input._default.inActions.Teleport.GetStateDown(hand.handType);
    }

    public bool getTouchPad_Up()
    {
        return SteamVR_Input._default.inActions.Teleport.GetStateUp(hand.handType);
    }

    public Vector3 getControllerPosition()
    {
        SteamVR_Action_Pose[] poseActions = SteamVR_Input._default.poseActions;
        if (poseActions.Length > 0)
        {
            return poseActions[0].GetLocalPosition(hand.handType);
        }
        return new Vector3(0, 0, 0);
    }

    public Quaternion getControllerRotation()
    {
        SteamVR_Action_Pose[] poseActions = SteamVR_Input._default.poseActions;
        if (poseActions.Length > 0)
        {
            return poseActions[0].GetLocalRotation(hand.handType);
        }
        return Quaternion.identity;
    }
    public Interactable GetNearestInteractable()
    {
        Interactable nearest = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach (Interactable interactable in m_ContactInteractables)
        {
            distance = (interactable.handFollowTransform.position - transform.position).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = interactable;
            }
        }
        return nearest;
    }
    public void PickUp()
    {
        m_CurrentInteractable = GetNearestInteractable();
        if (!m_CurrentInteractable)
            return;
 

        m_CurrentInteractable.transform.position = transform.position;

        Rigidbody targetBody = m_CurrentInteractable.GetComponent<Rigidbody>();
        m_Joint.connectedBody = targetBody;

       
    }
    public void Drop()
    {

        if (!m_CurrentInteractable)
            return;

        Rigidbody targetBody = m_CurrentInteractable.GetComponent<Rigidbody>();
        targetBody.velocity = m_Pose.GetVelocity();
        targetBody.angularVelocity = m_Pose.GetAngularVelocity();

        m_Joint.connectedBody = null;

       

    }

    // Nolan Mar 22 - trying to disconnect pill bottles, etc - not using the physics
    public void disconnectSimple () {
        if (!m_CurrentInteractable)
            return;

        m_Joint.connectedBody = null;
    }
}
