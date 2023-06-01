using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    public Transform PlayerCamera;
   /* public Transform Portal;
    public Transform OtherPortal;*/

    // Update is called once per frame
    void Update()
    {
        //Vector3 playerOffsetFromPortal = PlayerCamera.position - OtherPortal.position;
        //transform.position = Portal.position + playerOffsetFromPortal;

        /*float angleDifferenceBetweenPortalRotations = Quaternion.Angle(Portal.rotation, OtherPortal.rotation);

        Quaternion portalRotationDifference = Quaternion.AngleAxis(angleDifferenceBetweenPortalRotations, Vector3.up);
        Vector3 newCameraDirection = portalRotationDifference * PlayerCamera.forward;*/
        transform.rotation = PlayerCamera.rotation; //Quaternion.LookRotation(newCameraDirection, Vector3.up);
    }
}
