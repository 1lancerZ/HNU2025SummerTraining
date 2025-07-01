using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformLink : MonoBehaviour
{
    public Transform Target;

    public void UpdateTransform()
    {
        if (Target == null)
            return;
        Target.SetPositionAndRotation(this.transform.position, this.transform.rotation);
    }
}

