using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowUI : MonoBehaviour
{
    public Camera cameraToFollow;

    // Update is called once per frame
    private void Update()
    {
        if(cameraToFollow)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cameraToFollow.transform.position);
        }
    }
}
