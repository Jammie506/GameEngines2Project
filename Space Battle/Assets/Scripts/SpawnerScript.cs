using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnerScript : MonoBehaviour
{
    public GameObject Prefab;

    [SerializeField] private float prefabCount;

    [SerializeField] private float maxX;
    [SerializeField] private float maxY;
    [SerializeField] private float maxZ;

    [SerializeField] private float waitTime;

    private void Start()
    {
        StartCoroutine(ExecuteAfterTime(waitTime));
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
 
        for (int i = 0; i < prefabCount; i++)
        { 
            Instantiate(Prefab,
                new Vector3(Random.Range(-maxX, maxX), Random.Range(-maxY, maxY), Random.Range(-maxY, maxZ)),
                Quaternion.Euler(0, 0, Random.Range(0, 360)));
        }
    }
}