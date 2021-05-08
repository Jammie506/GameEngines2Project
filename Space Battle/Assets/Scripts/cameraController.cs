using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{ 
    public Transform Target1;
    public Transform Target2;

    [SerializeField] Vector3 offsetPos;
    [SerializeField] private float Timer = 0;
    [SerializeField] private float rotSpeed = 1;
    
    void Update()
    {
        Timer += Time.deltaTime;

        if (Timer < 30)
        {
            transform.position = Vector3.Slerp(transform.position, Target1.position+offsetPos, Time.deltaTime);
            //transform.LookAt(Target1); 
            
            Vector3 lTargetDir = Target1.position - transform.position;
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lTargetDir), Time.time * rotSpeed);
        }

        if (Timer > 30 && Timer < 60)
        {
            transform.position = Vector3.Slerp(transform.position, Target2.position+offsetPos, Time.deltaTime);
            transform.LookAt(Target2); 
        }
        
        if (Timer > 60 && Timer < 90)
        {
            transform.position = Vector3.Slerp(transform.position, Target1.position+offsetPos, Time.deltaTime);
            transform.LookAt(Target1); 
        }
        
    }
}
