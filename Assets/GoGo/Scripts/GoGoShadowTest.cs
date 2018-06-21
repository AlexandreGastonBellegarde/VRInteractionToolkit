﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class GoGoShadowTest : MonoBehaviour
{

    public GameObject theController;

    public SteamVR_TrackedObject trackedObj;

    public GameObject theModel;

    private SteamVR_Controller.Device device;
    private Vector3 vector_from_device;
    private Vector3 device_origin;
    private bool remote_go_go = false;
    bool extendingForward = true; // If not in extendingmode the arm will retract
    bool extending = false;
    float extensionSpeed = 0.02f;
    bool ranAlready = false;
    bool calibrated = false;
    Vector3 chestPosition;
    Vector3 relativeChestPos;
    Vector3 trueRelativeChestPos;
    float armLength;

    public GameObject testObject;

    // TODO: THIS IS A HACK. NEED TO FIND A METHOD THAT JUST WAITS UNTIL THE MODEL IS INITALIZED INSTEAD OF CALLING OVER AND OVER
    void makeModelChild()
    {
        if (theModel.transform.childCount > 0)
        {
            theModel.transform.parent = this.transform;
        }

    }

    // Might have to have a manuel calibration for best use
    float getDistanceToExtend()
    {
        if (calibrated) // will only work if has been calibrated
        {         
            chestPosition = Camera.main.transform.position - relativeChestPos;

            float distChestPos = Vector3.Distance(trackedObj.transform.position, chestPosition);

            float k = 10f;  // Important for how far can extend

            float D = (2f * armLength) / 3f; // 2/3 of users arm length

            //D = 0;
            if (distChestPos >= D)
            {
                float extensionDistance = distChestPos + (k * (float)Math.Pow(distChestPos - D, 2));
                // Dont need both here as we only want the distance to extend by not the full distance
                // but we want to keep the above formula matching the original papers formula so will then calculate just the distance to extend below
                return extensionDistance - distChestPos;
            }
        }

        return 0; // dont extend
    }

    /*
    void calibrateChestInformationForGogo()
    {
        // Must be pointing controller out like a raycast parallel to arm for calibration (backwards towards user)
        Vector3 armVector = trackedObj.transform.forward * -1;
        Vector3 controller = trackedObj.transform.position;
        Vector3 headPos = Camera.main.transform.position;
        Vector3 headVector = Camera.main.transform.up * -1;

        // Uses skew lines formula
        Vector3 n = Vector3.Cross(headVector, Vector3.Cross(armVector, headVector));
        chestPosition = controller + ((Vector3.Dot((headPos - controller), n)) / (Vector3.Dot(armVector, n))) * armVector;

        //relativeChestPos = headPos - chestPosition; // need to get the position of chest relative to head so moves with user when he moves
        relativeChestPos = Camera.main.transform.InverseTransformPoint(chestPosition);

        armLength = Vector3.Distance(trackedObj.transform.position, chestPosition);
        calibrated = true;
    }
    */

    // Use this for initialization
    void Start()
    {
        //trackedObj = this.GetComponent<SteamVR_TrackedObject>();
        CopySpecialComponents(theController, this.gameObject);

    }

    private void CopySpecialComponents(GameObject _sourceGO, GameObject _targetGO)
    {
        foreach (var component in _sourceGO.GetComponentsInChildren<Component>())
        {
            var componentType = component.GetType();
            if (componentType != typeof(Transform) &&
                componentType != typeof(MeshFilter) &&
                componentType != typeof(MeshRenderer)
                )
            {
                Debug.Log("Found a component of type " + component.GetType());
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(_targetGO);
                Debug.Log("Copied " + component.GetType() + " from " + _sourceGO.name + " to " + _targetGO.name);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TEST DELETE LATER
        if (testObject != null && chestPosition != null)
        {
            testObject.transform.position = chestPosition;
        }

        makeModelChild();
        //this.GetComponentInChildren<SteamVR_RenderModel>().gameObject.SetActive(false);
        Renderer[] renderers = this.transform.parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.name == "Standard (Instance)")
            {
                renderer.enabled = true;
            }
        }
        checkForAction();
        moveControllerForward();


    }

    void moveControllerForward()
    {
        // Using the origin and the forward vector of the remote the extended positon of the remote can be calculated
        //Vector3 theVector = theController.transform.forward;
        Vector3 theVector = theController.transform.position - chestPosition;


        Vector3 pose = theController.transform.position;
        Quaternion rot = theController.transform.rotation;

        float distance_formula_on_vector = Mathf.Sqrt(theVector.x * theVector.x + theVector.y * theVector.y + theVector.z * theVector.z);

        float distanceToExtend = getDistanceToExtend();

        if (distanceToExtend != 0)
        {
            print("here");
            // Using formula to find a point which lies at distance on a 3D line from vector and direction
            pose.x = pose.x + (distanceToExtend / (distance_formula_on_vector)) * theVector.x;
            pose.y = pose.y + (distanceToExtend / (distance_formula_on_vector)) * theVector.y;
            pose.z = pose.z + (distanceToExtend / (distance_formula_on_vector)) * theVector.z;
        }

        transform.position = pose;
        transform.rotation = rot;
        print("Actual control pos: " + trackedObj.transform.position.x);
        print("New control pos: " + pose.x);
    }


    void checkForAction()
    {
        device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Axis0)) // top side of touchpad and pushing down
        {
            // 0.12f
            // Want the center of the chest so have to move the location back as the VR googles are extruding from the vive
            //Vector3 centerOfHead = Camera.main.transform.position + (Camera.main.transform.forward.normalized * -0.08f);
            // 0.3 down to chest
            Vector3 downToChest = Camera.main.transform.position + (Vector3.down * 0.2f);

            chestPosition = downToChest;

            relativeChestPos = Camera.main.transform.position - chestPosition; // need to get the position of chest relative to head so moves with user when he moves
            
        }
        if(device.GetPressUp(SteamVR_Controller.ButtonMask.Axis0))
        {

            armLength = Vector3.Distance(trackedObj.transform.position, chestPosition);
            calibrated = true;
        }
    }
}