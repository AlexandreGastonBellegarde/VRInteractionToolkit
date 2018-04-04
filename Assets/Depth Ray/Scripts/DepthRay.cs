﻿using UnityEngine;

public class DepthRay : MonoBehaviour {

    /* Flexible Pointer implementation by Kieran May
     * University of South Australia
     * 
     * TODO
     * -Change alpha of object to see laser
     * -Fix issue with laser going through controller
     * */

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;
    private RaycastHit[] raycastObjects;

    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    public GameObject cubeAssister;
    private Vector3 hitPoint;
    private Vector3 hitPoint2D;

    //Giving a weird get_FrameCount error in the console for some reason?
    /*int rightIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
    /int leftIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);*/

    private void ShowLaser(RaycastHit hit) {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        //cubeAssister.transform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        //laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, distance*10);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance*2);
        //print(distance);
    }


    private void ShowLaser() {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, forward, .5f);
        laserTransform.LookAt(forward);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, 10f);
    }

    private float extendDistance = 0f;
    private float cursorSpeed = 20f; // Decrease to make faster, Increase to make slower

    private void PadScrolling() {
        if (controller.GetAxis().y != 0) {
            //print(controller.GetAxis().y);
            //cursor.transform.position += new Vector3(0f, 0f, controller.GetAxis().y/20);
            extendDistance += controller.GetAxis().y / cursorSpeed;
            moveCubeAssister();
        }
    }


    void moveCubeAssister() {
        //getControllerPosition();
        Vector3 pose = trackedObj.transform.position;
        Vector3 controllerPos = trackedObj.transform.forward;
        float distance_formula_on_vector = Mathf.Sqrt(controllerPos.x * controllerPos.x + controllerPos.y * controllerPos.y + controllerPos.z * controllerPos.z);
        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        pose.x = pose.x + (extendDistance / (distance_formula_on_vector)) * controllerPos.x;
        pose.y = pose.y + (extendDistance / (distance_formula_on_vector)) * controllerPos.y;
        pose.z = pose.z + (extendDistance / (distance_formula_on_vector)) * controllerPos.z;

        cubeAssister.transform.position = pose;
        cubeAssister.transform.rotation = trackedObj.transform.rotation;
    }

    public float thickness = 0.002f;
    float dist = 100f;

    private int ClosestObject() {
        int lowestValue = 0;
        float lowestDist = 0;
        for (int i = 0; i < raycastObjects.Length; i++) {
            float dist = Vector3.Distance(cubeAssister.transform.position, raycastObjects[i].transform.position) / 2f;
            if (i == 0) {
                lowestDist = dist;
                lowestValue = 0;
            } else {
                if (dist < lowestDist) {
                    lowestDist = dist;
                    lowestValue = i;
                }
            }
        }

            return lowestValue;
    }


    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start() {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
        cubeAssister.transform.position = trackedObj.transform.position;
        /*GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;
        for (int i=0; i<allObjects.Length; i++) {
            if (allObjects[i].layer != LayerMask.NameToLayer("Ignore Raycast")) {
                raycastObjects[count] = allObjects[i];
            }
            count++;
        }*/
    }
    float distance = 0f;
    Vector3 forward;

    void Update() {
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        moveCubeAssister();
        PadScrolling();
        forward = trackedObj.transform.TransformDirection(Vector3.forward) * 10;
        ShowLaser();
        RaycastHit[] hits = Physics.RaycastAll(trackedObj.transform.position, forward, 100.0F);
        if (hits.Length >= 1) {
            raycastObjects = hits;
            int closestVal = ClosestObject();
            print("My closest value:" + raycastObjects[closestVal].transform.name);
        }
        for (int i = 0; i < hits.Length; i++) {
            RaycastHit hit = hits[i];
            //print("hit:" + hit.transform.name + " index:"+i);
            distance = hit.distance;
            hitPoint = hit.point;
            //hit.transform.gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.5f);
            ShowLaser(hit);
        }
    }
        // Update is called once per frame
        /*void Update() {
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        RaycastHit hit;
        forward = trackedObj.transform.TransformDirection(Vector3.forward) * 10;
        //ShowLaser();
        if (Physics.Raycast(trackedObj.transform.position, forward, out hit)) {
            distance = hit.distance;
            hitPoint = hit.point;
            print(distance + " | "+hit.collider.gameObject.name);
            ShowLaser(hit);
        }*/
        /*Ray ray = Camera.main.ScreenPointToRay(trackedObj.transform.position);
        if (controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
            RaycastHit hit;
            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100)) {
                print("hit:" + hit.transform.name);
                hitPoint = hit.point;
                ShowLaser(hit);
            } else {
                hitPoint = ray.GetPoint(10);
                ShowLaser();
            }
        } else {
            laser.SetActive(false);
        }*/
}