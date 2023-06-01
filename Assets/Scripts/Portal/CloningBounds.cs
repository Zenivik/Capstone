using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloningBounds : MonoBehaviour
{
    public Transform ReferenceTransform;

    public delegate void PortalEnterHandler(Portal sender);
    public event PortalEnterHandler PortalEnter;

    public delegate void PortalExitHandler(Portal sender);
    public event PortalExitHandler PortalExit;

    public void OnTriggerEnter(Collider other)
    {
        var portal = other.GetComponent<Portal>();
        if (portal == null) return;
        PortalEnter?.Invoke(portal);
    }

    public void OnTriggerExit(Collider other)
    {
        var portal = other.GetComponent<Portal>();
        if (portal == null) return;
        PortalExit?.Invoke(portal);
    }
}
