using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformClone : AbstractClone
{
    public Transform target;

    public override void OnCloneUpdate(Portal sender, Portal destination)
    {
        transform.SetPositionAndRotation(Portal.TransformPositionBetweenPortals(sender, destination, target.position),
            Portal.TransformRotationBetweenPortals(sender, destination, target.rotation));
    }
}
