﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImagePlane_StickyHand : MonoBehaviour {
    internal bool objSelected = false;
    private GameObject cameraHead;
    private GameObject cameraRig;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;
    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private GameObject mirroredCube;
    public GameObject pointOfInteraction;
    private GameObject selectedObject;
    private Transform oldParent;
    public Material outlineMaterial;

    public enum InteractionType { Selection, Manipulation_Movement, Manipulation_Full };
    public InteractionType interacionType;

    public enum ControllerPicked { Left_Controller, Right_Controller };
    public ControllerPicked controllerPicked;

    private void ShowLaser(RaycastHit hit) {
        mirroredCube.SetActive(false);
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(pointOfInteraction.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
        InstantiateObject(hit.transform.gameObject);
    }

    private void interactionPosition() {
        pointOfInteraction.transform.localPosition = trackedObj.transform.localPosition;
        pointOfInteraction.transform.localRotation = trackedObj.transform.localRotation;
        pointOfInteraction.transform.localRotation *= Quaternion.Euler(75, 0, 0);
    }

    internal void resetProperties() {
        objSelected = false;
        selectedObject.transform.SetParent(oldParent);
        cameraHead.transform.localScale = new Vector3(1f, 1f, 1f);
        cameraRig.transform.localScale = new Vector3(1f, 1f, 1f);
        cameraRig.transform.localPosition = new Vector3(0f, 0f, 0f);
    }

    private void InstantiateObject(GameObject obj) {
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            if (objSelected == false && obj.transform.name != "Mirrored Cube") {
                selectedObject = obj;
                oldParent = selectedObject.transform.parent;
                obj.transform.SetParent(trackedObj.transform);
                float dist = Vector3.Distance(trackedObj.transform.position, obj.transform.position);
                obj.transform.position = Vector3.Lerp(trackedObj.transform.position, obj.transform.position, 0.2f);
                obj.transform.localScale = (obj.transform.localScale / dist);
                obj.transform.localScale /= 2;
                obj.transform.GetComponent<Renderer>().material = outlineMaterial;
            } else if (objSelected == true) {
                //resetProperties();
            }
        }
    }


    private void WorldGrab() {
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) { // temp
            //Resetting everything back to normal
            objSelected = false;
            selectedObject.transform.SetParent(oldParent);
            trackedObj.transform.localScale = new Vector3(1f, 1f, 1f);
            cameraHead.transform.localScale = new Vector3(1f, 1f, 1f);
            cameraRig.transform.localScale = new Vector3(1f, 1f, 1f);
            cameraRig.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
    }

    private void ShowLaser() {
        laser.SetActive(true);
        mirroredCube.SetActive(true);
    }

    void extendDistance(float distance, GameObject obj) {
        Vector3 controllerPos = trackedObj.transform.forward;
        Vector3 pos = trackedObj.transform.position;
        float distance_formula_on_vector = Mathf.Sqrt(controllerPos.x * controllerPos.x + controllerPos.y * controllerPos.y + controllerPos.z * controllerPos.z);
        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        pos.x -= (distance / (distance_formula_on_vector)) * controllerPos.x;
        pos.y -= (distance / (distance_formula_on_vector)) * controllerPos.y;
        pos.z -= (distance / (distance_formula_on_vector)) * controllerPos.z;

        obj.transform.position = pos;
        obj.transform.rotation = trackedObj.transform.rotation;
    }


    private float cursorSpeed = 20f; // Decrease to make faster, Increase to make slower

    void mirroredObject() {
        Vector3 controllerPos = cameraHead.transform.forward;
        float distance_formula_on_vector = Mathf.Sqrt(controllerPos.x * controllerPos.x + controllerPos.y * controllerPos.y + controllerPos.z * controllerPos.z);
        Vector3 mirroredPos = pointOfInteraction.transform.position;

        mirroredPos.x = mirroredPos.x + (100f / (distance_formula_on_vector)) * controllerPos.x;
        mirroredPos.y = mirroredPos.y + (100f / (distance_formula_on_vector)) * controllerPos.y;
        mirroredPos.z = mirroredPos.z + (100f / (distance_formula_on_vector)) * controllerPos.z;

        mirroredCube.transform.position = mirroredPos;
        mirroredCube.transform.rotation = pointOfInteraction.transform.rotation;
    }

    void Awake() {
        GameObject controllerRight = GameObject.Find("Controller (right)");
        GameObject controllerLeft = GameObject.Find("Controller (left)");
        cameraHead = GameObject.Find("Camera (eye)");
        cameraRig = GameObject.Find("[CameraRig]");
        mirroredCube = this.transform.Find("Mirrored Cube").gameObject;
        if (controllerPicked == ControllerPicked.Right_Controller) {
            trackedObj = controllerRight.GetComponent<SteamVR_TrackedObject>();
        } else if (controllerPicked == ControllerPicked.Left_Controller) {
            trackedObj = controllerLeft.GetComponent<SteamVR_TrackedObject>();
        } else {
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }
    }

    void Start() {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }

    void castRay() {
        interactionPosition();
        mirroredObject();
        ShowLaser();
        Ray ray = Camera.main.ScreenPointToRay(pointOfInteraction.transform.position);
        RaycastHit hit;
        if (Physics.Raycast(pointOfInteraction.transform.position, cameraHead.transform.forward, out hit, 100)) {
            hitPoint = hit.point;
            ShowLaser(hit);
        }
    }

    void Update() {
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        //if (objSelected == false) {
        castRay();
        if (objSelected == true) {
            WorldGrab(); //Using the ScaledWorldGrab to scale down the world
        }
    }

}
