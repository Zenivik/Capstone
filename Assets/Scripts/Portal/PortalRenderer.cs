using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalRenderer : MonoBehaviour
{
    public Camera PortalCamera;
    public int MaxRecursions = 2;

    public int debugTotalRenderCount;

    private Camera MainCamera;
    private Portal[] AllPortals;

    private void Awake()
    {
        MainCamera = Camera.main;
    }


    private void Update()
    {
        AllPortals = FindObjectsOfType<Portal>();
    }

    private void OnPreRender()
    {
        debugTotalRenderCount = 0;

        if(AllPortals.Length > 0)
        {
            var cameraPlanes = GeometryUtility.CalculateFrustumPlanes(MainCamera);
            foreach (var portal in AllPortals)
            {
                if (!portal.ShouldRender(cameraPlanes)) continue;

                portal.RenderViewthroughRecursive(MainCamera.transform.position, MainCamera.transform.rotation,
                    out _, out _, out var renderCount, PortalCamera, 0, MaxRecursions);

                debugTotalRenderCount += renderCount;
            }
        }
        
    }

    private void OnPostRender()
    {
        RenderTexturePool.Instance.ReleaseAllTextures();
    }

}
