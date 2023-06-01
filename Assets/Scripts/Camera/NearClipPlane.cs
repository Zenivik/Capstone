using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearClipPlane : MonoBehaviour
{
    public float nearClipPlaneValue = 0.0001f;

    void Start()
    {
        GetComponent<Camera>().nearClipPlane = nearClipPlaneValue;
    }
}
