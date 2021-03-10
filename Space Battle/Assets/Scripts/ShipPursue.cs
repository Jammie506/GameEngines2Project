using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPursue : MainBrain
{
    public ShipMain target;

    Vector3 targetPos;

    public void OnDrawGizmos()
    {
        if (Application.isPlaying && isActiveAndEnabled)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetPos);
        }
    }

    public override Vector3 Calculate()
    {
        float dist = Vector3.Distance(target.transform.position, transform.position);
        float time = dist / ShipMain.maxSpeed;

        targetPos = target.transform.position + (target.velocity * time);

        return ShipMain.SeekForce(targetPos);
    }
}