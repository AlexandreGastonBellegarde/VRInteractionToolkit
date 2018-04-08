﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class FlexiblePointer : MonoBehaviour
{
    // DOING PUBLICLY FOR TESTING LATER SEE IF CAN DO IT AUTOMATICALLY! Also because it needs both controlers need checks that they are both online otherwise do nothing
    public SteamVR_TrackedObject trackedObj1;
    public SteamVR_TrackedObject trackedObj2;
    public GameObject testControlPoint;

    private float curve = 0.2f;
    public float scaleFactor = 2f;

    private float[] point0; // Hand location
    private float[] point1; // The curve
    private float[] point2; // The distance

    // Laser vars
    private int numOfLasers = 20;
    public GameObject laserPrefab;
    private GameObject[] lasers;
    private Transform[] laserTransform;
    private Vector3 hitPoint;

    void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
        // Setting up points
        point0 = new float[3];
        point1 = new float[3];
        point2 = new float[3];

        // Initalizing all the lasers
        lasers = new GameObject[numOfLasers];
        laserTransform = new Transform[numOfLasers];
        for (int i = 0; i < numOfLasers; i++)
        {
            GameObject laserPart = Instantiate(laserPrefab, new Vector3((float)i, 1, 0), Quaternion.identity) as GameObject;
            laserTransform[i] = laserPart.transform;
            lasers[i] = laserPart;
        }
        setPoint0and1();
    }

    // Returns 1 for controller 1 and 2 for controller 2
    int calculatePointingController()
    {
        Vector3 playerPos = this.transform.position;
        float distTo1 = distBetweenVectors(playerPos, trackedObj1.transform.position);
        float distTo2 = distBetweenVectors(playerPos, trackedObj2.transform.position);

        return 1;
        // Allows to switch between but is buggy so while testing will keep off
        /*
        if(distTo1 > distTo2)
        {
            return 1;
        } else
        {
            return 2;
        }
        */
    }

    void setPoint0and1()
    {
        // Setting test points
        Vector3 controller1Pos = trackedObj1.transform.position;

        Vector3 controller2Pos = trackedObj2.transform.position;

        Vector3 forwardVectorBetweenRemotes;

        // Will extend further based on the scale factor
        // by multiplying the distance between controllers by it
        // and calculating new end control point
        float distanceBetweenControllers = distBetweenVectors(controller1Pos, controller2Pos) * scaleFactor;

        if (calculatePointingController() == 1)
        {
            forwardVectorBetweenRemotes = new Vector3(controller1Pos.x - controller2Pos.x, controller1Pos.y - controller2Pos.y, controller1Pos.z - controller2Pos.z);

            // Start control point
            point0[0] = controller2Pos.x;
            point0[1] = controller2Pos.y;
            point0[2] = controller2Pos.z;

            float distance_formula_on_vector = Mathf.Sqrt(forwardVectorBetweenRemotes.x * forwardVectorBetweenRemotes.x + forwardVectorBetweenRemotes.y * forwardVectorBetweenRemotes.y + forwardVectorBetweenRemotes.z * forwardVectorBetweenRemotes.z);

            // End control point
            point2[0] = controller1Pos.x + (distanceBetweenControllers / (distance_formula_on_vector)) * forwardVectorBetweenRemotes.x; ;
            point2[1] = controller1Pos.y + (distanceBetweenControllers / (distance_formula_on_vector)) * forwardVectorBetweenRemotes.y; ;
            point2[2] = controller1Pos.z + (distanceBetweenControllers / (distance_formula_on_vector)) * forwardVectorBetweenRemotes.z; ;
        }
        else
        {
            forwardVectorBetweenRemotes = new Vector3(controller2Pos.x - controller1Pos.x, controller2Pos.y - controller1Pos.y, controller2Pos.z - controller1Pos.z);

            // Start control point
            point0[0] = controller1Pos.x;
            point0[1] = controller1Pos.y;
            point0[2] = controller1Pos.z;

            float distance_formula_on_vector = Mathf.Sqrt(forwardVectorBetweenRemotes.x * forwardVectorBetweenRemotes.x + forwardVectorBetweenRemotes.y * forwardVectorBetweenRemotes.y + forwardVectorBetweenRemotes.z * forwardVectorBetweenRemotes.z);

            // End control point
            point2[0] = controller2Pos.x + (distanceBetweenControllers / (distance_formula_on_vector)) * forwardVectorBetweenRemotes.x;
            point2[1] = controller2Pos.y + (distanceBetweenControllers / (distance_formula_on_vector)) * forwardVectorBetweenRemotes.y;
            point2[2] = controller2Pos.z + (distanceBetweenControllers / (distance_formula_on_vector)) * forwardVectorBetweenRemotes.z;
        }

        setCurveControlPoint();
    }

    // Initalizes point1 's curve
    void setCurveControlPoint()
    {
        /*
        // Setting control point of curve
        // Based off of rotation of controllers
        // to calculate the control point we find the perpendicular vector (in the center of the vector between each remote
        // Then we calculate the intersection of that vector with the forward vector of the non-pointing remote 
        // and use the backward vector of the pointing remote and its intersection

        // assuming 1 is pointing controller for test
        Vector3 d1 = trackedObj1.transform.forward * -1;
        Vector3 d2 = trackedObj2.transform.forward;

        Vector3 p1 = trackedObj1.transform.position;
        Vector3 p2 = trackedObj2.transform.position;

        // as these two vectors will probably create skew lines (on different planes) have to calculate the points on the lines that are
        // closest to eachother and then getting the midpoint between them giving a fake 'intersection'
        // This is achieved by utilizing parts of the fromula to find the shortest distance between two skew lines
        Vector3 n1 = Vector3.Cross(d1, (Vector3.Cross(d2, d1)));
        Vector3 n2 = Vector3.Cross(d2, (Vector3.Cross(d1, d2)));

        // Figuring out point 1
        float p1Numerator = Vector3.Dot((p2 - p1), n2);
        float p1Denominator = Vector3.Dot(d1, n2);
        float p1afterDivision = p1Numerator / p1Denominator;
        Vector3 point1 = p1 + p1afterDivision * d1;

        // Figuring out point 2
        float p2Numerator = Vector3.Dot((p1 - p2), n1);
        float p2Denominator = Vector3.Dot(d2, n1);
        float p2afterDivision = p2Numerator / p2Denominator;
        Vector3 point2 = p2 + p2afterDivision * p2;

        Vector3 midPointBetweenPoints = (point1+point2)/2;
        print(point1.x);

        testControlPoint.transform.position = midPointBetweenPoints;
        */
        
        // Midpoint of the points 0 and 1
        float midX = (point0[0] + point1[0]) / 2;
        float midY = (point0[1] + point1[1]) / 2;
        float midZ = (point0[2] + point1[2]) / 2;

        point1[0] = midX;
        point1[1] = midY;
        point1[2] = midZ;

        testControlPoint.transform.position = new Vector3(midX, midY, midZ);
    }

    float distBetweenVectors(Vector3 one, Vector3 two)
    {
        return Mathf.Sqrt(Mathf.Pow(one.x - two.x, 2) + Mathf.Pow(one.y - two.y, 2) + Mathf.Pow(one.z - two.z, 2));
    }

    // Update is called once per frame
    void Update()
    {
        setPoint0and1();
        castBezierRay();
    }

    void castBezierRay()
    {
        float valueToSearchBezierBy = 0f;

        Vector3 positionOfLastLaserPart;
        if (calculatePointingController() == 1)
        {
            positionOfLastLaserPart = trackedObj2.transform.position;
        }
        else
        {
            positionOfLastLaserPart = trackedObj1.transform.position;
        }

        for (int i = 0; i < numOfLasers; i++)
        {
            lasers[i].SetActive(true);
            float[] pointOnBezier = getBezierPoint(valueToSearchBezierBy);
            Vector3 nextPart = new Vector3(pointOnBezier[0], pointOnBezier[1], pointOnBezier[2]);
            float distBetweenParts = distBetweenVectors(nextPart, positionOfLastLaserPart);

            laserTransform[i].position = Vector3.Lerp(positionOfLastLaserPart, nextPart, .5f);
            laserTransform[i].LookAt(nextPart);
            laserTransform[i].localScale = new Vector3(laserTransform[i].localScale.x, laserTransform[i].localScale.y,
        distBetweenParts);

            positionOfLastLaserPart = nextPart;
            valueToSearchBezierBy += (1f / numOfLasers);
        }
    }

    // t being betweek 0 and 1 to get a spot on the curve
    float[] getBezierPoint(float t)
    {
        float[] thePoint = new float[3];
        // Formula used to get position on point
        thePoint[0] = (1 - t) * (1 - t) * point0[0] + 2 * (1 - t) * t * point1[0] + t * t * point2[0];  // x
        thePoint[1] = (1 - t) * (1 - t) * point0[1] + 2 * (1 - t) * t * point1[1] + t * t * point2[1];  // y
        thePoint[2] = (1 - t) * (1 - t) * point0[2] + 2 * (1 - t) * t * point1[2] + t * t * point2[2];  // z
        return thePoint;
    }
}
