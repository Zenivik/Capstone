using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalProjectile : MonoBehaviour
{
    [SerializeField]
    GameObject PortalPrefab;
    [HideInInspector]
    public WeaponController PortalGun;

    private PortalableObject portalableObject;

    private void Awake()
    {
        portalableObject = GetComponent<PortalableObject>();
        portalableObject.HasTeleported += PortalableObjectOnHasTeleported;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        //CreatePortal();
        Destroy(gameObject);
        
    }


    private void PortalableObjectOnHasTeleported(Portal sender, Portal destination, Vector3 newPosition, Quaternion newRotation)
    {
        // For character controller to update
        Physics.SyncTransforms();
    }

    private void OnDestroy()
    {
        portalableObject.HasTeleported -= PortalableObjectOnHasTeleported;
    }

}
