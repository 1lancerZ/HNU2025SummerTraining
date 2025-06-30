using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimationTriggers : MonoBehaviour
{
    private Gun gun => GetComponentInParent<Gun>();

    private void AnimationTrigger()
    {
        gun.AnimationTrigger();
    }

    public void OnEjectMag()
    {
        gun.OnEjectMag();
    }

    public void OnReloadInsertMag()
    {
        gun.OnReloadInsertMag();
    }
}
