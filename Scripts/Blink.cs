using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour
{
    public GameObject target;
    public float onTime;
    public float offTime;
    float timer;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer > onTime) {
            target.SetActive(false);
        }

        if (timer > onTime + offTime) {
            target.SetActive(true);
            timer -= onTime + offTime;
        }
    }
}
