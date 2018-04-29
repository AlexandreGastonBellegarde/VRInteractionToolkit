﻿using UnityEngine;

public class DepthRay : MonoBehaviour {

    /* Depth Ray implementation by Kieran May
     * University of South Australia
     * 
     * TODO
	 * -Add physics to gameObjects
     * -Refactor Code
     * */

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;
    public GameObject mirroredCube;
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
        mirroredCube.SetActive(false);
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        //cubeAssister.transform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        //laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, distance*10);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
        //print(distance);
    }
    private void ShowLaser() {
        laser.SetActive(true);
        mirroredCube.SetActive(true);


        //laser.transform.position = trackedObj.transform.position*2;
        //laser.transform.position = new Vector3(trackedObj.transform.position.x, trackedObj.transform.position.y, trackedObj.transform.position.z*1.25f);
        //laser.transform.position = Vector3.Lerp(trackedObj.transform.position, forward, 0.6f);
        //laserTransform.LookAt(forward);
        //laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, 1f);
        //laserTransform.position = Vector3.Lerp(trackedObj.transform.position, forward, .5f);
        /*laserTransform.position = Vector3.Lerp(trackedObj.transform.position, forward, .5f);
        laserTransform.LookAt(trackedObj.transform.position);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, 10f);*/
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

    private bool pickedUpObject = false; //ensure only 1 object is picked up at a time
    private GameObject tempObjectStored;
    void PickupObject(GameObject obj) {
        if (trackedObj != null) {
            if (controller.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger) && pickedUpObject == false) {
                //obj.GetComponent<Collider>().attachedRigidbody.isKinematic = true;
                obj.transform.SetParent(trackedObj.transform);
                tempObjectStored = obj; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
                pickedUpObject = true;
            }
            if (controller.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger) && pickedUpObject == true) {
                //obj.GetComponent<Collider>().attachedRigidbody.isKinematic = false;
                tempObjectStored.transform.SetParent(null);
                pickedUpObject = false;
            }
        }
    }


    void moveCubeAssister() {
        //getControllerPosition();
        Vector3 mirroredPos = trackedObj.transform.position;
        Vector3 pos = trackedObj.transform.position;
        Vector3 controllerPos = trackedObj.transform.forward;
        float distance_formula_on_vector = Mathf.Sqrt(controllerPos.x * controllerPos.x + controllerPos.y * controllerPos.y + controllerPos.z * controllerPos.z);
        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        if (extendDistance < 0) {
            extendDistance = 0;
        }
        pos.x = pos.x + (extendDistance / (distance_formula_on_vector)) * controllerPos.x;
        pos.y = pos.y + (extendDistance / (distance_formula_on_vector)) * controllerPos.y;
        pos.z = pos.z + (extendDistance / (distance_formula_on_vector)) * controllerPos.z;

        mirroredPos.x = mirroredPos.x + (100f / (distance_formula_on_vector)) * controllerPos.x;
        mirroredPos.y = mirroredPos.y + (100f / (distance_formula_on_vector)) * controllerPos.y;
        mirroredPos.z = mirroredPos.z + (100f / (distance_formula_on_vector)) * controllerPos.z;

        cubeAssister.transform.position = pos;
        cubeAssister.transform.rotation = trackedObj.transform.rotation;

        mirroredCube.transform.position = mirroredPos;
        mirroredCube.transform.rotation = trackedObj.transform.rotation;
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


    public bool controllerRightPicked;
    public bool controllerLeftPicked;

    void Awake() {
        GameObject controllerRight = GameObject.Find("Controller (right)");
        GameObject controllerLeft = GameObject.Find("Controller (left)");
        if (controllerRightPicked == true) {
            trackedObj = controllerRight.GetComponent<SteamVR_TrackedObject>();
        } else if (controllerLeftPicked == true) {
            trackedObj = controllerLeft.GetComponent<SteamVR_TrackedObject>();
        } else { //TODO: Automatically attempt to detect controller
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }
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
    private GameObject currentClosestObject;
    public Material outlineMaterial;
    private Material currentClosestObjectMaterial;
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
            if (raycastObjects[closestVal].transform.name == "Mirrored Cube") {
                //print("Could not find object");
            } else {
                //print("My closest value:" + raycastObjects[closestVal].transform.name);
                if (currentClosestObject != raycastObjects[closestVal].transform.gameObject) {
                    //print("new closest object");
                    if (currentClosestObject != null) {
                        if (currentClosestObjectMaterial != null) {
                            currentClosestObject.transform.GetComponent<Renderer>().material = currentClosestObjectMaterial;
                        }
                        currentClosestObjectMaterial = currentClosestObject.transform.GetComponent<Renderer>().material;
                    }
                    currentClosestObject = raycastObjects[closestVal].transform.gameObject;
                } else {
                    currentClosestObject.transform.GetComponent<Renderer>().material = outlineMaterial;
                }
                //currentClosestObject = raycastObjects[closestVal].transform.gameObject;
                //raycastObjects[closestVal].transform.GetComponent<Renderer>().material = outlineMaterial;
                PickupObject(raycastObjects[closestVal].transform.gameObject);
                //Renderer rend = raycastObjects[closestVal].transform.GetComponent<Renderer>();
                //rend.material.color = Color.red;
                //raycastObjects[closestVal].transform.GetComponent<Renderer>().material.color = Color.clear;
            }
        }
        //raycastObjects[closestVal].transform.GetComponent<Renderer>().material.color = Color.white;
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
