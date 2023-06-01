using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalableObjectClone : MonoBehaviour
{
    public CloningBounds CloningBounds;
    private readonly HashSet<Portal> currentlyTouchingPortals = new HashSet<Portal>();

    private PortalableObject PortalableObject;
    private AbstractClone[] Clones;

    public Portal ClosestTouchingPortal
    {
        get
        {
            var currentMin = (portal: (Portal)null, distance: float.PositiveInfinity);
            var referencePosition = CloningBounds.ReferenceTransform.position;
            
            foreach(var portal in currentlyTouchingPortals)
            {
                var closestPointOnPlane = portal.Plane.ClosestPointOnPlane(referencePosition);
                var distance = Vector3.Distance(closestPointOnPlane, referencePosition);

                if (distance < currentMin.distance) currentMin = (portal, distance);
            }

            return currentMin.portal;
        }
    }

    private void Awake()
    {
        PortalableObject = GetComponent<PortalableObject>();
        PortalableObject.HasTeleported += OnTeleported;

        Clones = GetComponentsInChildren<AbstractClone>();
        foreach (var clone in Clones) clone.OnCloneAwake();

        CloningBounds.PortalEnter += OnEnterPortal;
        CloningBounds.PortalExit += OnExitPortal;
    }

    private void OnDestroy()
    {
        PortalableObject.HasTeleported -= OnTeleported;
        CloningBounds.PortalEnter -= OnEnterPortal;
        CloningBounds.PortalExit -= OnExitPortal;
    }

    private void OnTeleported(Portal sender, Portal destination, Vector3 newPosition, Quaternion newRotation)
    {
        // OnTrigger events won't fire until next tick (Portal crossing calculations happen after OnTrigger events),
        // so if a frame is going to be rendered after this tick, we need the currentlyTouchingPortals to be correct.
        // This means manually editing it with the known teleport info. Since it is a HashMap, this should not
        // have any ill effects when OnTrigger happens next tick.
        currentlyTouchingPortals.Remove(sender);
        currentlyTouchingPortals.Add(destination);
        UpdateClones(); // // Force update after portal change for new transforms
    }

    private void OnEnterPortal(Portal sender)
    {
        // Only call OnCloneEnable for first portal entered
        if(currentlyTouchingPortals.Count == 1)
        {
            foreach(var clone in Clones)
            {
                clone.OnCloneEnable(sender, sender.TargetPortal);
            }
        }

        currentlyTouchingPortals.Add(sender);

        UpdateClones(); // Force update after portal change for new transforms
    }

    private void OnExitPortal(Portal sender)
    {
        currentlyTouchingPortals.Remove(sender);

        // Only call OnCloneDisable if all portals have been exited
        if(currentlyTouchingPortals.Count == 0)
        {
            foreach(var clone in Clones)
            {
                clone.OnCloneDisable(sender, sender.TargetPortal);
            }
        }
    }

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            try
            {
                if (currentlyTouchingPortals.Count != 0) UpdateClones();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void UpdateClones()
    {
        var closestPortal = ClosestTouchingPortal;
        if (closestPortal == null)
        {
            //throw new Exception("No touching portals found when trying to update clones.");
            return;
        }

        foreach (var clone in Clones)
            clone.OnCloneUpdate(closestPortal, closestPortal.TargetPortal);
    }
}
