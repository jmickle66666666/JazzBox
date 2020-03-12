using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadSplitter : MonoBehaviour
{

    [Header("Settings")]
    public Vector2 size;
    public int steps;
    [Range(0,1)] public float sizeWeight;
    [Range(0,1)] public float shiftChance;
    [Range(0,1)] public float shiftDistance;
    [Range(0,1)] public float singleDouble;
    [Range(0,5)] public float funScale;
    [Range(0,1)] public float flatChance;
    [Range(0,1)] public float minHeight;
    public float buildingHeight;
    public float relaxation;
    public Material material;
    public Material flatMaterial;

    List<Vector2> points;
    List<int[]> shapes;
    List<Vector2[]> bakedShapes;
    float[] heights;

    [Header("Make it work")]
    public bool work = false;

    void Update()
    {
        if (work) {
            work = false;
            Build();
        }

        if (bakedShapes != null) {
            // PreviewBakedShapes();
        }
    }
    
    void PreviewBakedShapes() {
        float p = 0f;
        for (int i = 0; i < shapes.Count; i++) {
            // p = (float)i / shapes.Count;
            p = funScale;
            for (int j = 0; j < shapes[i].Length; j++) {
                Vector3 a = bakedShapes[i][j];
                Vector3 b = bakedShapes[i][(j+1)%shapes[i].Length];
                bool edge = PointOnEdge(a) && PointOnEdge(b);
                a.z = a.y; a.y = heights[i] * p;
                b.z = b.y; b.y = heights[i] * p;

                a.x -= size.x/2;
                a.z -= size.y/2;
                b.x -= size.x/2;
                b.z -= size.y/2;

                Vector3 mp = (a+b)/2f;
                Vector3 normal = new Vector3(b.z - a.z, p, b.x - a.x).normalized;

                Debug.DrawLine(a,b, edge?Color.green:Color.red, 1f/60f, false);
            }
        }
    }

    void PreviewShapes() {
        for (int i = 0; i < shapes.Count; i++) {
            for (int j = 0; j < shapes[i].Length; j++) {
                Vector3 a = points[shapes[i][j]];
                Vector3 b = points[shapes[i][(j+1)%shapes[i].Length]];
                bool edge = PointOnEdge(a) && PointOnEdge(b);
                a.z = a.y; a.y = 0f;
                b.z = b.y; b.y = 0f;

                a.x -= size.x/2;
                a.z -= size.y/2;
                b.x -= size.x/2;
                b.z -= size.y/2;

                Debug.DrawLine(a,b, edge?Color.green:Color.red, 1f/60f, false);
            }
        }
    }

    void Build()
    {
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }

        points = new List<Vector2>() {
            new Vector2(0f, 0f),
            new Vector2(size.x, 0f),
            new Vector2(size.x, size.y),
            new Vector2(0f, size.y)
        };

        shapes = new List<int[]>();
        shapes.Add(new int[] {0, 1, 2, 3});

        for (int i = 0; i < steps; i++) {
            int randomChoice = 0;
            while (Random.value < sizeWeight && randomChoice < shapes.Count - 1) {
                randomChoice += 1;
            }
            if (Random.value < singleDouble) {
                DoubleDivide(randomChoice);
            } else {
                Subdivide(randomChoice);
            }
        }

        BakeShapes();
        ShrinkBakedShapes(Mathf.Min(size.x, size.y) / relaxation);
        SetHeights();

        for (int i = 0; i < shapes.Count; i++) {
            GameObject newBuilding = new GameObject();
            Mesh mesh = BuildShape(i);
            newBuilding.AddComponent<MeshFilter>().sharedMesh = mesh;
            newBuilding.AddComponent<MeshRenderer>().sharedMaterial = heights[i]==0f?flatMaterial:material;
            newBuilding.transform.parent = transform;
            newBuilding.transform.Translate(-size.x/2, 0f, -size.y/2, Space.World);
        }
    }

    void DoubleDivide(int shapeIndex)
    {
        int[] shape = shapes[shapeIndex];
        float shiftX = 0.5f;
        float shiftY = 0.5f;
        if (Random.value < shiftChance) {
            shiftX += (Random.value - 0.5f) * shiftDistance;
            shiftY += (Random.value - 0.5f) * shiftDistance;
        }

        Vector2 P01 = Vector2.Lerp(points[shape[0]], points[shape[1]], shiftX);
        Vector2 P12 = Vector2.Lerp(points[shape[1]], points[shape[2]], shiftY);
        Vector2 P23 = Vector2.Lerp(points[shape[3]], points[shape[2]], shiftX);
        Vector2 P30 = Vector2.Lerp(points[shape[0]], points[shape[3]], shiftY);
        Vector2 PCenter = (P01 + P12 + P23 + P30)/4;

        points.Add(P01);
        points.Add(P12);
        points.Add(P23);
        points.Add(P30);
        points.Add(PCenter);

        int p01Index = points.Count - 5;
        int p12Index = points.Count - 4;
        int p23Index = points.Count - 3;
        int p30Index = points.Count - 2;
        int pCenterIndex = points.Count - 1;

        int[] shapeA = new int[] { shape[0], p01Index, pCenterIndex, p30Index };
        int[] shapeB = new int[] { shape[1], p12Index, pCenterIndex, p01Index };
        int[] shapeC = new int[] { shape[2], p23Index, pCenterIndex, p12Index };
        int[] shapeD = new int[] { shape[3], p30Index, pCenterIndex, p23Index };

        shapes.Add(shapeA);
        shapes.Add(shapeB);
        shapes.Add(shapeC);
        shapes.Add(shapeD);

        shapes.RemoveAt(shapeIndex);
    }

    void Subdivide(int shapeIndex)
    {
        int[] shape = shapes[shapeIndex];

        int offset = Random.Range(0, shape.Length);

        float shift = 0.5f;
        if (Random.value < shiftChance) {
            shift += (Random.value - 0.5f) * shiftDistance;
        }

        Vector2 A = Vector2.Lerp(
            points[shape[(0 + offset) % shape.Length]],
            points[shape[(1 + offset) % shape.Length]],
            shift
        );

        Vector2 B = Vector2.Lerp(
            points[shape[(2 + offset) % shape.Length]],
            points[shape[(3 + offset) % shape.Length]],
            shift
        );
        
        points.Add(A);
        points.Add(B);

        int[] newShape = new int[4];

        newShape[(0 + offset) % shape.Length] = points.Count-2;
        newShape[(1 + offset) % shape.Length] = points.Count-1;
        newShape[(2 + offset) % shape.Length] = shape[(3 + offset) % shape.Length];
        newShape[(3 + offset) % shape.Length] = shape[(0 + offset) % shape.Length];

        shapes.Add(
            newShape
        );

        shape[(0 + offset) % shape.Length] = points.Count - 2;
        shape[(3 + offset) % shape.Length] = points.Count - 1;
        shapes.Add(shape);
        shapes.RemoveAt(shapeIndex);
    }

    bool PointOnEdge(Vector2 point)
    {
        return (point.x == 0f || point.x == size.x || point.y == 0f || point.y == size.y);
    }

    void BakeShapes()
    {
        bakedShapes = new List<Vector2[]>();
        shapes.ForEach(s => {
            bakedShapes.Add(new Vector2[] {
                points[s[0]],
                points[s[1]],
                points[s[2]],
                points[s[3]]
            });
        });
    }

    void ShrinkBakedShapes(float distance)
    {
        for (int i = 0; i < bakedShapes.Count; i++) {
            Vector2 center = (bakedShapes[i][0] + bakedShapes[i][1] + bakedShapes[i][2] + bakedShapes[i][3]) / 4f;
            for (int j = 0; j < bakedShapes[i].Length; j++) {
                int xEdge = -1;
                int yEdge = -1;
                if (bakedShapes[i][j].x == 0f) xEdge = 0;
                if (bakedShapes[i][j].x == size.x) xEdge = 1;
                if (bakedShapes[i][j].y == 0f) yEdge = 0;
                if (bakedShapes[i][j].y == size.y) yEdge = 1;

                Vector2 diff = center - bakedShapes[i][j];
                bakedShapes[i][j] += diff.normalized * distance;

                if (xEdge == 0) bakedShapes[i][j].x = 0f;
                if (xEdge == 1) bakedShapes[i][j].x = size.x;
                if (yEdge == 0) bakedShapes[i][j].y = 0;
                if (yEdge == 1) bakedShapes[i][j].y = size.y;
            }
        }
    }

    void SetHeights()
    {
        heights = new float[shapes.Count];
        for (int i = 0; i < shapes.Count; i++) {
            if (Random.value > flatChance) {
                heights[i] = minHeight + (Random.value * (1f - minHeight));
            } else {
                heights[i] = 0f;
            }
        }
    }

    Mesh BuildShape(int shapeIndex)
    {
        Vector2[] shape = bakedShapes[shapeIndex];
        float height = heights[shapeIndex] * buildingHeight;

        Vector3[] mainVertices = new Vector3[shape.Length * 2];
        for (int i = 0; i < shape.Length; i++) {
            mainVertices[i] = new Vector3(
                shape[i].x, 0f, shape[i].y
            );

            mainVertices[i + shape.Length] = new Vector3(
                shape[i].x, height, shape[i].y
            );
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        if (height == 0f) {
            vertices.AddRange(new Vector3[4] {
                mainVertices[0],
                mainVertices[1],
                mainVertices[2],
                mainVertices[3]
            });

            triangles.AddRange(new int[6] {
                0, 2, 1, 0, 3, 2, //top
            });
        } else {

            vertices.Add(mainVertices[4]);
            vertices.Add(mainVertices[5]);
            vertices.Add(mainVertices[6]);
            vertices.Add(mainVertices[7]);
            triangles.AddRange(new [] {0,2,1,0,3,2});
            int n = vertices.Count;
            vertices.Add(mainVertices[0]);
            vertices.Add(mainVertices[1]);
            vertices.Add(mainVertices[4]);
            vertices.Add(mainVertices[5]);
            triangles.AddRange(new [] {n+0,n+2,n+3,n+0,n+3,n+1});
            n = vertices.Count;
            vertices.Add(mainVertices[1]);
            vertices.Add(mainVertices[2]);
            vertices.Add(mainVertices[5]);
            vertices.Add(mainVertices[6]);
            triangles.AddRange(new [] {n+0,n+2,n+3,n+0,n+3,n+1});
            n = vertices.Count;
            vertices.Add(mainVertices[2]);
            vertices.Add(mainVertices[3]);
            vertices.Add(mainVertices[6]);
            vertices.Add(mainVertices[7]);
            triangles.AddRange(new [] {n+0,n+2,n+3,n+0,n+3,n+1});
            n = vertices.Count;
            vertices.Add(mainVertices[3]);
            vertices.Add(mainVertices[0]);
            vertices.Add(mainVertices[7]);
            vertices.Add(mainVertices[4]);
            triangles.AddRange(new [] {n+0,n+2,n+3,n+0,n+3,n+1});
        }

        Mesh output = new Mesh();
        output.vertices = vertices.ToArray();
        output.triangles = triangles.ToArray();
        output.RecalculateNormals();
        return output;
    }
}
