using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfollowing : MonoBehaviour
{
    [SerializeField] Vector3 velocity;
    [SerializeField] float speed;
    [SerializeField] Vector3 acceleration;
    [SerializeField] Vector3 force;
    [SerializeField] float maxSpeed = 5;
    [SerializeField] float maxForce = 10;

    [SerializeField] float mass = 1;

    [SerializeField] bool seekEnabled = true;
    [SerializeField] Transform seekTargetTransform;
    [SerializeField] Vector3 seekTarget;

    [SerializeField] bool arriveEnabled = false;
    [SerializeField] Transform arriveTargetTransform;
    [SerializeField] Vector3 arriveTarget;
    [SerializeField] float slowingDistance = 80;

    [SerializeField] Path path;
    [SerializeField] bool pathFollowingEnabled = false;
    [SerializeField] float waypointDistance = 3;

    // Banking
    [SerializeField] float banking = 0.1f; 

    [SerializeField] float damping = 0.1f;

    [SerializeField] bool playerSteeringEnabled = false;
    [SerializeField] float steeringForce = 100;

    [SerializeField] bool pursueEnabled = false;
    [SerializeField] Pathfollowing pursueTarget;

    [SerializeField] Vector3 pursueTargetPos;

    public Vector3 Pursue(Pathfollowing pursueTarget)
    {
        float dist = Vector3.Distance(pursueTarget.transform.position, transform.position);

        float time = dist / maxSpeed;

        pursueTargetPos = pursueTarget.transform.position + pursueTarget.velocity * time;

        return Seek(pursueTargetPos);
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + velocity);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + acceleration);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + force * 10);

        if (arriveEnabled)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(arriveTargetTransform.position, slowingDistance);
        }

        if (pursueEnabled)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, pursueTargetPos);
        }

    }

    public Vector3 PlayerSteering()
    {
        Vector3 force = Vector3.zero;

        force += Input.GetAxis("Vertical") * transform.forward * steeringForce;

        Vector3 projectedRight = transform.right;
        projectedRight.y = 0;
        projectedRight.Normalize();

        force += Input.GetAxis("Horizontal") * projectedRight * steeringForce * 0.2f;

        return force;
    }

    public Vector3 PathFollow()
    {
        Vector3 nextWaypoint = path.NextWaypoint();
        if (!path.looped && path.IsLast())
        {
            return Arrive(nextWaypoint);
        }
        else
        {
            if (Vector3.Distance(transform.position, nextWaypoint) < waypointDistance)
            {
                path.AdvanceToNext();
            }
            return Seek(nextWaypoint);
        }
    }

    public Vector3 Seek(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        Vector3 desired = toTarget.normalized * maxSpeed;

        return (desired - velocity);
    } 

    public Vector3 Arrive(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        float dist = toTarget.magnitude;
        float ramped = (dist / slowingDistance) * maxSpeed;
        float clamped = Mathf.Min(ramped, maxSpeed);
        Vector3 desired = (toTarget / dist) * clamped;

        return desired - velocity;
    }

    public Vector3 CalculateForce()
    {
        Vector3 f = Vector3.zero;
        if (seekEnabled)
        {
            if (seekTargetTransform != null)
            {
                seekTarget = seekTargetTransform.position;
            }
            f += Seek(seekTarget);
        }

        if (arriveEnabled)
        {
            if (arriveTargetTransform != null)
            {
                arriveTarget = arriveTargetTransform.position;                
            }
            f += Arrive(arriveTarget);
        }

        if (pathFollowingEnabled)
        {
            f += PathFollow();
        }
        if (playerSteeringEnabled)
        {
            f += PlayerSteering();
        }
        if (pursueEnabled)
        {
            f += Pursue(pursueTarget);
        }

        return f;
    }

    void Update()
    {
        force = CalculateForce();
        acceleration = force / mass;
        velocity = velocity + acceleration * Time.deltaTime;
        transform.position = transform.position + velocity * Time.deltaTime;
        speed = velocity.magnitude;
        if (speed > 0)
        {
            Vector3 tempUp = Vector3.Lerp(transform.up, Vector3.up + (acceleration * banking), Time.deltaTime * 3.0f);
            transform.LookAt(transform.position + velocity, tempUp);
            velocity -= (damping * velocity * Time.deltaTime);
        }        
    }
}