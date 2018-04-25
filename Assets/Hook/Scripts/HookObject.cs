﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookObject : MonoBehaviour {
    public int score;
    public GameObject ContainingObject { get; set; }
    public float lastDistance;

    public HookObject(GameObject ContainingObject)
    {
        this.ContainingObject = ContainingObject;
        lastDistance = Mathf.Infinity;
    }

    public void setDistance(GameObject obj)
    {
        lastDistance = Vector3.Distance(ContainingObject.transform.position, obj.transform.position);
    }

    public void decreaseScore()
    {
        if(score > 0)
        {
            score--;
        }
    }

    public void increaseScore()
    {
        score++;
    }

    public override bool Equals(object other)
    {
        GameObject compared = other as GameObject;
        return ContainingObject.Equals(compared);
    }
}
