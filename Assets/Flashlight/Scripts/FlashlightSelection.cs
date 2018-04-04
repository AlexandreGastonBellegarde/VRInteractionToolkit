﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightSelection : MonoBehaviour {

    public SteamVR_TrackedObject theController;

    private GameObject trackedObj;
    private GameObject collidingObject;
    private GameObject objectInHand;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)theController.index); }
    }

    void OnEnable()
    {
        trackedObj = this.transform.gameObject;
        var render = SteamVR_Render.instance;
        if (render == null)
        {
            enabled = false;
            return;
        }
    }

    void Awake()
    {
        //trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    private void SetCollidingObject(Collider col)
    {

        if (collidingObject || !col.GetComponent<Rigidbody>())
        {
            return;
        }

        collidingObject = col.gameObject;
    }


    public void OnTriggerEnter(Collider other)
    {
        SetCollidingObject(other);
    }


    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }


    public void OnTriggerExit(Collider other)
    {
        if (!collidingObject)
        {
            return;
        }

        collidingObject = null;
    }

    private void GrabObject()
    {

        objectInHand = collidingObject;
        collidingObject = null;

        var joint = AddFixedJoint();
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    private void ReleaseObject()
    {

        if (GetComponent<FixedJoint>())
        {

            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());

            objectInHand.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
            objectInHand.GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity;
        }

        objectInHand = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Controller.GetHairTriggerDown())
        {
            if (collidingObject)
            {
                GrabObject();
            }
        }


        if (Controller.GetHairTriggerUp())
        {
            if (objectInHand)
            {
                ReleaseObject();
            }
        }
    }
}