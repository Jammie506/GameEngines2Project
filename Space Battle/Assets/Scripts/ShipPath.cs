using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPath : MainBrain {

    public Path path;

    Vector3 nextWaypoint;

    public float waypointDistance = 5;

    public void OnDrawGizmos()
    {
        if (isActiveAndEnabled && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, nextWaypoint);
        }
    }
    
    public override Vector3 Calculate()
    {
        nextWaypoint = path.NextWaypoint();
        if (Vector3.Distance(transform.position, nextWaypoint) < waypointDistance)
        {
            path.AdvanceToNext();
        }

        if (!path.looped && path.IsLast())
        {
            return ShipMain.ArriveForce(nextWaypoint);
        }
        else
        {
            return ShipMain.SeekForce(nextWaypoint);
        }
    }
}