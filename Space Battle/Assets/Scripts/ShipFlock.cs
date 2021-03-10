using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipFlock : MainBrain
{
    public ShipMain leader;
    Vector3 targetPos;
    Vector3 worldTarget;
    Vector3 offset;

    void Start()
    {
        offset = transform.position - leader.transform.position;

        offset = Quaternion.Inverse(leader.transform.rotation) * offset;
    }

    public override Vector3 Calculate()
    {
        worldTarget = leader.transform.TransformPoint(offset);
        float dist = Vector3.Distance(transform.position, worldTarget);
        float time = dist / ShipMain.maxSpeed;

        targetPos = worldTarget + (leader.velocity * time);
        return ShipMain.ArriveForce(targetPos);
    }
}