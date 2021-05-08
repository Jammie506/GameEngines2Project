using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wander : MainBrain
{
    public float jitter = 100;

    public Vector3 target;
    public Vector3 worldTarget;

    public float wideness;

    /*public void OnDrawGizmos()
    {
        if (isActiveAndEnabled && Application.isPlaying)
        {
            Vector3 localCP = Vector3.forward * distance;
            Vector3 worldCP = transform.TransformPoint(localCP);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(worldCP, radius);
            Gizmos.DrawSphere(worldTarget, 0.1f);
            Gizmos.DrawLine(transform.position, worldTarget);
        }
    }*/


    public override Vector3 Calculate()
    {
        Vector3 disp = jitter * Random.insideUnitSphere * Time.deltaTime;
        target += disp;

        target = Vector3.ClampMagnitude(target, wideness);

        Vector3 localTarget = (target * wideness);

        worldTarget = transform.TransformPoint(localTarget);
        worldTarget.y = Random.Range(0, wideness);
        
        return worldTarget - transform.position;
    }
}