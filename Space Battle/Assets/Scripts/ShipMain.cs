using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMain : MonoBehaviour
{
    List<MainBrain> behaviours = new List<MainBrain>();

    public Vector3 force = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 velocity = Vector3.zero;
    public float mass = 1;

    [Range(0.0f, 10.0f)]
    public float damping = 0.01f;

    [Range(0.0f, 1.0f)]
    public float banking = 0.1f;
    public float maxSpeed = 5.0f;
    public float maxForce = 10.0f;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + velocity);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + acceleration);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + force * 10);
    }

    void Start()
    {

        MainBrain[] behaviours = GetComponents<MainBrain>();

        foreach (MainBrain mB in behaviours)
        {
            this.behaviours.Add(mB);            
        }
    }

    public Vector3 SeekForce(Vector3 target)
    {
        Vector3 desired = target - transform.position;
        desired.Normalize();
        desired *= maxSpeed;
        return desired - velocity;
    }

    public Vector3 ArriveForce(Vector3 target, float slowingDistance = 40.0f)
    {
        Vector3 toTarget = target - transform.position;

        float distance = toTarget.magnitude;

        
        if (distance > 0)
        {        
            float ramped = maxSpeed * (distance / slowingDistance);

            float clamped = Mathf.Min(ramped, maxSpeed);
            Vector3 desired = clamped * (toTarget / distance);

            return desired - velocity;
        }
        else
        {
            return Vector3.zero;
        }        
    }
    

    Vector3 Calculate()
    {
        force = Vector3.zero;

        // Weighted prioritised truncated running sum
        // 1. Behaviours are weighted
        // 2. Behaviours are prioritised
        // 3. Truncated
        // 4. Running sum


        foreach (MainBrain mB in behaviours)
        {
            if (mB.isActiveAndEnabled)
            {
                force += mB.Calculate() * mB.weight;      
                float f = force.magnitude;
                if (f >= maxForce)
                {
                    force = Vector3.ClampMagnitude(force, maxForce);
                    break;
                }                         
            }
        }

        return force;
    }


    void Update()
    {
        force = Calculate();
        acceleration = force / mass;
        velocity += acceleration * Time.deltaTime;

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        
        if (velocity.magnitude > 0)
        {
            Vector3 tempUp = Vector3.Lerp(transform.up, Vector3.up + (acceleration * banking), Time.deltaTime * 3.0f);
            transform.LookAt(transform.position + velocity, tempUp);

            transform.position += velocity * Time.deltaTime;
            velocity *= (1.0f - (damping * Time.deltaTime));
        }
    }
}
