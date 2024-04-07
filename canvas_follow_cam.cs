using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class canvas_follow_cam : MonoBehaviour
{
    public Transform cameraTransform; // Assign vr cam thingy in inspector
    public float uiDistance;

    void Update()
    {
        // Follow cam
        transform.position = cameraTransform.position;
        transform.rotation = cameraTransform.rotation;
        transform.position = cameraTransform.position + cameraTransform.forward * uiDistance;
    }
}