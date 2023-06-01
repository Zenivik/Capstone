using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Portal : MonoBehaviour
{
    public bool IsOnGround = true;
    public Portal TargetPortal;
    public Transform NormalVisible;
    public Transform NormalInvisible;

    public Portal[] VisiblePortals;

    public Renderer ViewthroughRenderer;
    public Texture viewthroughDefaultTexture;
    private Material ViewthroughMat;

    private Camera MainCamera;

    public Plane Plane;
    private Vector4 vectorPlane;

    private HashSet<PortalableObject> ObjectsInPortal = new HashSet<PortalableObject>();
    private HashSet<PortalableObject> ObjectsInPortalToRemove = new HashSet<PortalableObject>();

    public bool ShouldRender(Plane[] cameraPlanes) => ViewthroughRenderer.isVisible && 
        GeometryUtility.TestPlanesAABB(cameraPlanes, ViewthroughRenderer.bounds);

    private struct VisiblePortalResources
    {
        public Portal VisiblePortal;
        public RenderTexturePool.PoolItem PoolItem;
        public Texture OriginalTexture;
    }

    #region Helper Methods
    public static Vector3 TransformPositionBetweenPortals(Portal sender, Portal target, Vector3 position)
    {
        return target.NormalInvisible.TransformPoint(
                sender.NormalVisible.InverseTransformPoint(position));
    }

    public static Quaternion TransformRotationBetweenPortals(Portal sender, Portal target, Quaternion rotation)
    {
        return target.NormalInvisible.rotation * Quaternion.Inverse(sender.NormalVisible.rotation) * rotation;
    }

    public static Vector3 TransformDirectionBetweenPortals(Portal sender, Portal target, Vector3 position)
    {
        return
            target.NormalInvisible.TransformDirection(
                sender.NormalVisible.InverseTransformDirection(position));
    }
    #endregion

    private void Awake()
    {
        // Cloned material
        ViewthroughMat = ViewthroughRenderer.material;

        // Cache main camera
        MainCamera = Camera.main;

        // Generate bounding plane
        Plane = new Plane(NormalVisible.forward, transform.position);
        vectorPlane = new Vector4(Plane.normal.x, Plane.normal.y, Plane.normal.z, Plane.distance);

        StartCoroutine(WaitForFixedUpdateLoop());
    }

    private void OnDestroy()
    {
        Destroy(ViewthroughMat);
    }

    private void OnTriggerEnter(Collider other)
    {
        var portalableObject = other.GetComponent<PortalableObject>();

        if (portalableObject != null && portalableObject.gameObject.tag.Equals("Enemy"))
        {
            GameObject enemy = portalableObject.gameObject;
            enemy.GetComponent<NavMeshAgent>().enabled = false;
        }

        if (portalableObject)
        {
            ObjectsInPortal.Add(portalableObject);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        var portalableObject = other.GetComponent<PortalableObject>();
        if (portalableObject)
        {
            ObjectsInPortal.Remove(portalableObject);
        }
    }

    private void CheckForPortalCrossing()
    {
        // Clear removal queue
        ObjectsInPortalToRemove.Clear();

        // Check every touching object
        foreach (var portalableObject in ObjectsInPortal)
        {
            // If portalable object has been destroyed, remove it immediately
            if (portalableObject == null)
            {
                ObjectsInPortalToRemove.Add(portalableObject);
                continue;
            }

            // Check if portalable object is behind the portal using Vector3.Dot (dot product)
            // If so, they have crossed through the portal.
            var pivot = portalableObject.transform;
            var directionToPivotFromTransform = pivot.position - transform.position;
            directionToPivotFromTransform.Normalize();

            var pivotToNormalDotProduct = Vector3.Dot(directionToPivotFromTransform, NormalVisible.forward);
            if (pivotToNormalDotProduct > 0) continue;

            // Warp object
            var newPosition = TransformPositionBetweenPortals(this, TargetPortal, portalableObject.transform.position);
            var newRotation = TransformRotationBetweenPortals(this, TargetPortal, portalableObject.transform.rotation);

            // Make sure it stays upright
            Vector3 zxRotation = newRotation.eulerAngles;
            zxRotation.x = 0;
            zxRotation.z = 0;

            newRotation.eulerAngles = zxRotation;

            portalableObject.transform.SetPositionAndRotation(newPosition, newRotation);

            portalableObject.OnHasTeleported(this, TargetPortal, newPosition, newRotation);

            // Object is no longer touching this side of the portal
            ObjectsInPortalToRemove.Add(portalableObject);
        }

        // Remove all objects queued up for removal
        foreach (var portalableObject in ObjectsInPortalToRemove)
        {
            ObjectsInPortal.Remove(portalableObject);
        }
    }

    public void RenderViewthroughRecursive(Vector3 refPosition, Quaternion refRotation, out RenderTexturePool.PoolItem tempPoolItem, 
        out Texture originalTexture, out int debugRenderCount, Camera portalCamera, int currentRecursion, int maxRecursions)
    {
        debugRenderCount = 1;
        if(TargetPortal == null)
        {
            tempPoolItem = null;
            originalTexture = null;
            return;
        }

        // Calculate portal camera position and rotation
        var virtualPosition = TransformPositionBetweenPortals(this, TargetPortal, refPosition);
        var virtualRotation = TransformRotationBetweenPortals(this, TargetPortal, refRotation);

        // Setup up portal camera for calculations
        portalCamera.transform.SetPositionAndRotation(virtualPosition, virtualRotation);

        // Convert target portal's plane to camera space (relative to target camera)
        var targetViewThroughPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) 
            * TargetPortal.vectorPlane;

        // Set portal camera projection matrix to clip walls between target portal and target camera
        // Inherits main camera near/far clip plane and FOV settings
        var obliqueProjectionMatrix = MainCamera.CalculateObliqueMatrix(targetViewThroughPlaneCameraSpace);
        portalCamera.projectionMatrix = obliqueProjectionMatrix;

        var visiblePortalResourcesList = new List<VisiblePortalResources>();

        // Calculate camera planes for culling
        var cameraPlanes = GeometryUtility.CalculateFrustumPlanes(portalCamera);

        // Recurse if not at limit
        if (currentRecursion < maxRecursions)
        {
            foreach (var visiblePortal in TargetPortal.VisiblePortals)
            {
                if (!visiblePortal.ShouldRender(cameraPlanes)) continue;

                visiblePortal.RenderViewthroughRecursive(
                    virtualPosition, virtualRotation,
                    out var visiblePortalTemporaryPoolItem,
                    out var visiblePortalOriginalTexture,
                    out var visiblePortalRenderCount,
                    portalCamera,
                    currentRecursion + 1, maxRecursions);

                visiblePortalResourcesList.Add(new VisiblePortalResources()
                {
                    OriginalTexture = visiblePortalOriginalTexture,
                    PoolItem = visiblePortalTemporaryPoolItem,
                    VisiblePortal = visiblePortal
                });

                debugRenderCount += visiblePortalRenderCount;
            }
        }
        else
        {
            foreach (var visiblePortal in TargetPortal.VisiblePortals)
            {
                visiblePortal.ShowViewthroughDefaultTexture(out var visiblePortalOriginalTexture);

                visiblePortalResourcesList.Add(new VisiblePortalResources()
                {
                    OriginalTexture = visiblePortalOriginalTexture,
                    VisiblePortal = visiblePortal
                });
            }
        }

        // Get new temporary render texture and set to portal's material
        // Will be released by CALLER, not by this function. This is so that the caller can use the render texture
        // for their own purposes, such as a Render() or a main camera render, before releasing it.
        tempPoolItem = RenderTexturePool.Instance.GetTexture();

        // Use portal camera
        portalCamera.targetTexture = tempPoolItem.Texture;
        portalCamera.transform.SetPositionAndRotation(virtualPosition, virtualRotation);
        portalCamera.projectionMatrix = obliqueProjectionMatrix;

        // Render portal camera to target texture
        portalCamera.Render();

        // Reset and release
        foreach (var resources in visiblePortalResourcesList)
        {
            // Reset to original texture
            // So that it will remain correct if the visible portal is still expecting to be rendered
            // on another camera but has already rendered its texture. Originally the texture may be overriden by other renders.
            resources.VisiblePortal.ViewthroughMat.mainTexture = resources.OriginalTexture;

            // Release temp render texture
            if (resources.PoolItem != null)
            {
                RenderTexturePool.Instance.ReleaseTexture(resources.PoolItem);
            }
        }

        // Must be after camera render, in case it renders itself (in which the texture must not be replaced before rendering itself)
        // Must be after restore, in case it restores its own old texture (in which the new texture must take precedence)
        originalTexture = ViewthroughMat.mainTexture;
        ViewthroughMat.mainTexture = tempPoolItem.Texture;

    }

    public static bool RaycastRecursive(Vector3 position, Vector3 direction, LayerMask layerMask, int maxRecursions, out RaycastHit hitInfo)
    {
        return RaycastRecursiveInternal(position, direction, layerMask, maxRecursions, out hitInfo, 0, null);
    }

    private static bool RaycastRecursiveInternal(Vector3 position, Vector3 direction,
        LayerMask layerMask, int maxRecursions, out RaycastHit hitInfo, int currentRecursion, GameObject ignoreObject)
    {
        // Ignore a specific object when raycasting.
        // Useful for preventing a raycast through a portal from hitting the target portal from the back,
        // which makes a raycast unable to go through a portal since it'll just be absorbed by the target portal's trigger.
        var ignoreObjectOriginalLayer = 0;
        if (ignoreObject)
        {
            ignoreObjectOriginalLayer = ignoreObject.layer;
            ignoreObject.layer = 2; // Ignore raycast
        }

        // Shoot raycast
        var raycastHitSomething = Physics.Raycast(position, direction, out var hit, Mathf.Infinity, layerMask);

        // Reset ignore
        if (ignoreObject)
            ignoreObject.layer = ignoreObjectOriginalLayer;

        // If no objects are hit, the recursion ends here, with no effect
        if (!raycastHitSomething)
        {
            hitInfo = new RaycastHit(); // Dummy
            return false;
        }

        // If the object hit is a portal, recurse, unless we are already at max recursions
        var portal = hit.collider.GetComponent<Portal>();
        if (portal)
        {
            if(portal.TargetPortal == null)
            {
                hitInfo = new RaycastHit();
                return false;
            }
            else if(currentRecursion >= maxRecursions)
            {
                hitInfo = new RaycastHit(); // Dummy
                return false;
            }

            // Continue going down the rabbit hole

            return RaycastRecursiveInternal(TransformPositionBetweenPortals(portal, portal.TargetPortal, hit.point),
                TransformDirectionBetweenPortals(portal, portal.TargetPortal, direction),
                layerMask, maxRecursions, out hitInfo, currentRecursion + 1, portal.TargetPortal.gameObject);
        }

        // If the object hit is not a portal, then congrats! We stop here and report back that we hit something.
        hitInfo = hit;
        return true;
    }

    private void ShowViewthroughDefaultTexture(out Texture originalTexture)
    {
        originalTexture = ViewthroughMat.mainTexture;
        ViewthroughMat.mainTexture = viewthroughDefaultTexture;
    }

    private IEnumerator WaitForFixedUpdateLoop()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        while (true)
        {
            yield return waitForFixedUpdate;
            try
            {
                CheckForPortalCrossing();
            }
            catch(Exception e)
            {
                // Catch exceptions so our loop doesn't die whenever there is an error
                Debug.LogException(e);
            }
        }
    }
}
