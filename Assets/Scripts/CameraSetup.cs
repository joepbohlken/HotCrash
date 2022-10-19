using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [SerializeField] Camera p1Cam;
    [SerializeField] Camera p2Cam;

    [NonSerialized] public Rect rect1;
    [NonSerialized] public Rect rect2;
    [NonSerialized] public Rect rect3;
    [NonSerialized] public Rect rect4;
    // Start is called before the first frame update
    public void SetCamera(int playerAmount)
    {
        rect1.x = 0f;
        rect1.y = 0f;
        rect1.width = 1f;
        rect1.height = 1f;
        if (playerAmount == 2)
        {
            rect1.x = -0.5f;
            rect1.width = 1f;
            rect1.height = 1f;

            rect2.x = 0.5f;
            rect2.width = 1f;
            rect2.height = 1f;
        }
        else if (playerAmount >= 3)
        {
            rect1.x = -0.5f;
            rect1.y = 0.5f;
            rect1.width = 1f;
            rect1.height = 1f;

            rect2.x = 0.5f;
            rect2.y = 0.5f;
            rect2.width = 1f;
            rect2.height = 1f;

            rect3.x = 0f;
            rect3.y = -0.5f;
            rect3.width = 1f;
            rect3.height = 1f;
        }
        else if (playerAmount >= 3)
        {
            rect1.x = -0.5f;
            rect1.y = 0.5f;
            rect1.width = 1f;
            rect1.height = 1f;

            rect2.x = 0.5f;
            rect2.y = 0.5f;
            rect2.width = 1f;
            rect2.height = 1f;

            rect3.x = -0.5f;
            rect3.y = -0.5f;
            rect3.width = 1f;
            rect3.height = 1f;

            rect4.x = 0.5f;
            rect4.y = -0.5f;
            rect4.width = 1f;
            rect4.height = 1f;
        }
    }

    public Rect GetRect(int player)
    {
        switch (player)
        {
            case 0:
                return rect1;
            case 1:
                return rect2;
            case 2:
                return rect3;
            case 3:
                return rect4;
            default:
                return new Rect();
        }
    }
}
