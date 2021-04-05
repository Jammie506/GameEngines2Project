using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPath : MainBrain {

    public Path path;

    Vector3 nextWaypoint;

    public float waypointDistance = 5;
    
    
    [SerializeField] private float targetVelocity = 10.0f;
    [SerializeField] private int numberOfRays = 17;
    [SerializeField] private float angle = 90;

    [SerializeField] private float rayRange = 2;

    void Update()
    {
        var deltaPosition = Vector3.zero;

        for (int i = 0; i < numberOfRays; i++)
        {
            var rotation = this.transform.rotation;
            var rotationMod =
                Quaternion.AngleAxis((i / ((float) numberOfRays - 1)) * angle * 2 - angle, this.transform.up);
            var direction = rotation * rotationMod * transform.forward;

            var ray = new Ray(this.transform.position, direction);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, rayRange))
            {
                deltaPosition -= (1.0f / numberOfRays) * targetVelocity * direction;
            }
            else
            {
                deltaPosition += (1.0f / numberOfRays) * targetVelocity * direction;
            }
        }
    }
    public void OnDrawGizmos()
    {
        if (isActiveAndEnabled && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, nextWaypoint);
        }
        
        for (int i = 0; i < numberOfRays; i++)
        {
            var rotation = this.transform.rotation;
            var rotationMod =
                Quaternion.AngleAxis((i / ((float) numberOfRays - 1)) * angle * 2 - angle, this.transform.up);
            var direction = rotation * rotationMod * transform.forward;
            
            Gizmos.DrawRay(this.transform.position, direction);
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