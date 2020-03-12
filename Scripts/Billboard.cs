using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    public Camera camera;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(camera.transform.position - transform.position, Vector3.up);
    }
}
