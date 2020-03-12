using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MeshTriangle {
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
}

public static class MeshExtension {
    public static void RecalculateSharpNormals(this Mesh mesh) {
        List<MeshTriangle> meshTriangles = new List<MeshTriangle>();
        for (int i = 0; i < mesh.triangles.Length; i+=3) {
            meshTriangles.Add(new MeshTriangle() {
                A = mesh.vertices[mesh.triangles[i]],
                B = mesh.vertices[mesh.triangles[i+1]],
                C = mesh.vertices[mesh.triangles[i+2]]
            });
        }

        Vector3[] vertices = new Vector3[meshTriangles.Count * 3];
        int[] triangles = new int[meshTriangles.Count * 3];
        for (int i = 0; i < meshTriangles.Count; i++) {
            vertices[(i*3) + 0] = meshTriangles[i].A;
            triangles[(i*3) + 0] = (i*3) + 0;
            vertices[(i*3) + 1] = meshTriangles[i].B;
            triangles[(i*3) + 1] = (i*3) + 1;
            vertices[(i*3) + 2] = meshTriangles[i].C;
            triangles[(i*3) + 2] = (i*3) + 2;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}

public class QuantizeMesh : MonoBehaviour
{
    public Bounds worldBounds;
    public int subdivisions;
    public bool work = false;
    public Material material;

    void Update()
    {
        worldBounds.center = transform.position;
        if (work) {

            work = false;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Build();
            Debug.Log(stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
        }   
    }

    void OnValidate()
    {
        if (subdivisions < 1) subdivisions = 1;

        // Build();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
    }

    void Build()
    {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        Bounds meshBounds = new Bounds();
        meshBounds.SetMinMax(worldBounds.min, worldBounds.min + (worldBounds.size / subdivisions));
        
        var points = new List<List<List<bool>>>();

        Vector3 pos = new Vector3();
        for (int i = 0; i < subdivisions; i++) {
            for (int j = 0; j < subdivisions; j++) {
                for (int k = 0; k < subdivisions; k++) {
                    pos.x = worldBounds.min.x + (meshBounds.size.x * i);
                    pos.y = worldBounds.min.y + (meshBounds.size.y * j);
                    pos.z = worldBounds.min.z + (meshBounds.size.z * k);
                    meshBounds.SetMinMax(pos, pos + meshBounds.size);

                    if (FilledPoint(meshBounds)) {
                        Mesh mesh = BuildCube(meshBounds);
                        GameObject obj = new GameObject("Cub");
                        obj.AddComponent<MeshFilter>().mesh = mesh;
                        obj.AddComponent<MeshRenderer>().sharedMaterial = material;
                        obj.transform.parent = transform;
                    }

                }  
            }
        }   
    }

    bool FilledPoint(Bounds bounds) {
        // Vector3 A = bounds.min;
        // Vector3 B = bounds.max;
        // Vector3 C = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        // Vector3 D = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        return Physics.CheckBox(bounds.center, bounds.extents/2);
        // return Physics.BoxCast(bounds.center, bounds.extents/2, Vector3.zero);
        // if (Physics.Raycast(A, B - A, Vector3.Distance(A, B)) || Physics.Raycast(C, D - C, Vector3.Distance(C, D))) {
        //     return true;
        // }
        // return false;
    }

    Mesh BuildCube(Bounds bounds)
    {
        Vector3[] vertices = new Vector3[] { 
            new Vector3( bounds.min.x, bounds.min.y, bounds.min.z ),
            new Vector3( bounds.max.x, bounds.min.y, bounds.min.z ),
            new Vector3( bounds.min.x, bounds.max.y, bounds.min.z ),
            new Vector3( bounds.max.x, bounds.max.y, bounds.min.z ),
            new Vector3( bounds.min.x, bounds.min.y, bounds.max.z ),
            new Vector3( bounds.max.x, bounds.min.y, bounds.max.z ),
            new Vector3( bounds.min.x, bounds.max.y, bounds.max.z ),
            new Vector3( bounds.max.x, bounds.max.y, bounds.max.z )
        };

        int[] triangles = new int[] {
            0, 2, 1, 2, 3, 1,
            4, 5, 6, 6, 5, 7,
            2, 6, 3, 3, 6, 7,
            0, 1, 4, 5, 4, 1,
            0, 4, 2, 2, 4, 6,
            1, 3, 5, 3, 7, 5

        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateSharpNormals();
        return mesh;
    }
}
