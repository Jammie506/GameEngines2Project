using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidMaker : MonoBehaviour
{
    public GameObject Xwing;

    [SerializeField] private float ShipCount;

    [SerializeField] private float maxX;
    [SerializeField] private float maxY;
    [SerializeField] private float maxZ;
    
    void Start()
    {
        for (int i = 0; i < ShipCount; i++)
        {
            Instantiate(Xwing,
                new Vector3(Random.Range(-maxX, maxX), Random.Range(-maxY, maxY), Random.Range(-maxY, maxZ)),
                Quaternion.Euler(0, 0, Random.Range(0, 360)));
        }
    }
}
