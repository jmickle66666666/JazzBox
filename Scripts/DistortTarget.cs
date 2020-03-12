using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistortTarget : MonoBehaviour
{
    public Distorter distorter;
    public MeshFilter meshFilter;
    Mesh mesh;
    Vector3[] initialVerts;
    public bool redo = false;

    void Start()
    {
        initialVerts = meshFilter.mesh.vertices;
        mesh = meshFilter.mesh;
    }

    // Update is called once per frame
    void Update()
    {
        // if (redo)
        // {
            // redo = false;
            UpdateMesh();
        // }
    }

    void UpdateMesh()
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++) {

            
            vertices[i] = transform.localToWorldMatrix.inverse.MultiplyPoint(distorter.TransformPoint(transform.localToWorldMatrix.MultiplyPoint(initialVerts[i])));
        }
        mesh.vertices = vertices;
    }
}
