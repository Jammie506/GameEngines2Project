using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reveal : MonoBehaviour
{
    [SerializeField] private float waitTime;
    private void Start()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
        this.gameObject.transform.GetChild(2).gameObject.SetActive(false);
        this.gameObject.transform.GetChild(3).gameObject.SetActive(false);

        StartCoroutine(RevealTime(waitTime));
    }

    IEnumerator RevealTime(float time)
    {
        yield return new WaitForSeconds(time);
        
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
        this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
        this.gameObject.transform.GetChild(2).gameObject.SetActive(true);
        this.gameObject.transform.GetChild(3).gameObject.SetActive(true);
    }
}
