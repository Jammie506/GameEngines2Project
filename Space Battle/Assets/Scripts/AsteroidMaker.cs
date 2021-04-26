using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidMaker : MonoBehaviour
{
    public GameObject SpaceRock1;
    public GameObject SpaceRock2;
    public GameObject SpaceRock3;

    [SerializeField] private float SpaceRock1Count;
    [SerializeField] private float SpaceRock2Count;
    [SerializeField] private float SpaceRock3Count;

    [SerializeField] private float maxX;
    [SerializeField] private float maxY;
    [SerializeField] private float maxZ;
    
    void Start()
    {
        for (int i = 0; i < SpaceRock1Count; i++)
        {
            Instantiate(SpaceRock1,
                new Vector3(Random.Range(-maxX, maxX), Random.Range(-maxY, maxY), Random.Range(-maxY, maxZ)),
                Quaternion.Euler(0, 0, Random.Range(0, 360)));
        }
        
        for (int j = 0; j < SpaceRock2Count; j++)
        {
            Instantiate(SpaceRock2,
                new Vector3(Random.Range(-maxX, maxX), Random.Range(-maxY, maxY), Random.Range(-maxZ, maxZ)),
                Quaternion.Euler(0, 0, Random.Range(0, 360)));
        }
        
        for (int k = 0; k < SpaceRock3Count; k++)
        {
            Instantiate(SpaceRock3,
                new Vector3(Random.Range(-maxX, maxX), Random.Range(-maxY, maxY), Random.Range(-maxZ, maxZ)),
                Quaternion.Euler(0, 0, Random.Range(0, 360)));
        }
    }
}
