using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierTest : MonoBehaviour
{
    public Transform startPos;


    public Transform P1;

    public Transform P2;

    public Transform EndPos;


    public Material Mat;


    Vector3 start;
    Vector3 p1;
    Vector3 p2;
    Vector3 end;



    private void Start()
    {
        
    }
    private void Update()
    {
        start = startPos.position;
        p1 = P1.position;
        p2 = P2.position;
        end = EndPos.position;

        Mat.SetVector("p0", start);
        Mat.SetVector("p1", p1);
        Mat.SetVector("p2", p2);
        Mat.SetVector("p3", end);
    }

}
