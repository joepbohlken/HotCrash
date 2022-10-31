using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    private Transform gunTip;
    public FistPush carFist;

    void Awake()
    {
        gunTip = carFist.transform.Find("GunTip");
    }

    //Called after Update
    void LateUpdate()
    {
        DrawRope();
    }
}