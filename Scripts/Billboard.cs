using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    new public Camera camera;

    // Update is called once per frame
    void Update()
    {
        if (camera != null) {
            transform.rotation = Quaternion.LookRotation(camera.transform.position - transform.position, Vector3.up);
        }
    }
}
