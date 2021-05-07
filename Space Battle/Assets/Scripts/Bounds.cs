using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounds : MonoBehaviour
{
    [SerializeField] private float X;
    [SerializeField] private float Y;
    [SerializeField] private float Z;
    void OnDrawGizmosSelected()
    {
        // Draw a yellow cube at the transform position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(X*2, Y*2, Z*2));
    }
}
