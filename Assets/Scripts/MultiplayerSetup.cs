using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerSetup : MonoBehaviour
{
    [SerializeField] Camera p1Cam;
    [SerializeField] Camera p2Cam;
    // Start is called before the first frame update
    void Start()
    {
        if(LoadScene.playerAmount > 1)
        {
            Rect rect = new Rect();
            rect.x = -0.5f;
            rect.width = 1f;
            rect.height = 1f;
            p1Cam.rect = rect;

            p2Cam.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
