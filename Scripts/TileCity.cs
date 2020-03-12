using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TileCity : MonoBehaviour
{
    public bool work = false;
    public int seed;
    public int numWidth;
    public int numHeight;
    public float areaWidth;
    public float areaHeight;
    [Range(0,1)] public float jitter;

    GameObject container;
    public Texture2D mapTexture;
    [Range(0,1)] public float threshold;

    public Material[] materials;
    
    public int carveLength;
    [Range(0,1)] public float carveTurnChance;
    public int carvers;

    public float meshHeight;
    public float meshHeightVariance;

    float rVal {
        get {
            return Random.value - 0.5f;
        }
    }

    Vector3[,] basePoints;
    int[,] baseData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (work || ready == false) {
            work = false;
            Build();
        }
    }

    void Clear()
    {
        if (container == null) {
            container = GameObject.Find("Container");
            if (container == null){
                container = new GameObject("Container");
            }
        }
        while (container.transform.childCount > 0) {
            DestroyImmediate(container.transform.GetChild(0).gameObject);
        }
    }

    void Build()
    {
        Clear();

        Random.InitState(seed);

        basePoints = new Vector3[numWidth, numHeight];
        float pOffX = areaWidth / numWidth;
        float pOffY = areaHeight / numHeight;

        float jitterX = pOffX * jitter;
        float jitterY = pOffY * jitter;

        for (int i = 0; i < numWidth; i++) {
            for (int j =0; j < numHeight; j++) {
                basePoints[i,j] = new Vector3(
                    (jitterX*rVal)-(areaWidth/2f) + pOffX * i,
                    0f,
                    (jitterY*rVal)-(areaHeight/2f) + pOffY * j
                );
            }
        }
        
        baseData = new int[numWidth-1, numHeight-1];
        for (int i = 0; i < numWidth-1; i++) {
            for (int j =0; j < numHeight-1; j++) {
                if (ValidPoint(i, j)) {

                    baseData[i,j] = Random.Range(1,3);

                } else {
                    baseData[i, j] = -1;
                }
            }
        }

        Random.InitState(seed);

        for (int i = 0; i < carvers; i++) {
            Carve(Random.Range(0, numWidth-2), Random.Range(0, numHeight-2), 0, carveTurnChance, carveLength);
        }

        Random.InitState(seed);

        for (int i = 0; i < numWidth-1; i++) {
            for (int j =0; j < numHeight-1; j++) {
                if (baseData[i,j] == 0) {
                    if (
                        baseData[i-1, j] == -1 ||
                        baseData[i+1, j] == -1 ||
                        baseData[i, j+1] == -1 ||
                        baseData[i, j-1] == -1
                    ) {
                        baseData[i,j] = Random.Range(1,3);
                    }
                }
            }
        }
        

        for (int i = 0; i < materials.Length; i++)
        {
            BuildMesh(i, i==0?0f:meshHeight, i!=0);
        }
        
        ready = true;
    }

    void Carve(int x, int y, int data, float turnChance, int life, int dir=-1)
    {
        if (baseData[x, y] != -1) {
            baseData[x, y] = data;
        }

        if (dir == -1 || Random.value < turnChance) { 
            dir = Random.Range(0, 4);

            if (x == 0) dir = 0;
            if (y == 0) dir = 1;
            if (x == numWidth-2) dir = 2;
            if (y == numHeight-2) dir = 3;
        }
        if (dir == 0) x += 1;
        if (dir == 1) y += 1;
        if (dir == 2) x -= 1;
        if (dir == 3) y -= 1;

        x = Mathf.Clamp(x, 0, numWidth-2);
        y = Mathf.Clamp(y, 0, numHeight-2);

        if (life > 0) {
            Carve(x, y, data, turnChance, life-1, dir);
        }
    }

    void BuildMesh(int dataIndex, float height, bool sides = false)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        bool freeside;

        for (int i = 1; i < numWidth-2; i++) {
            for (int j = 1; j < numHeight-2; j++) {
                if (baseData[i,j] == dataIndex) {
                    float h = height + rVal * meshHeightVariance;
                    int n = vertices.Count;
                    

                    if (sides) {

                        n = vertices.Count;
                        vertices.Add(basePoints[i,j] + Vector3.up * h);
                        vertices.Add(basePoints[i,j+1] + Vector3.up * h);
                        vertices.Add(basePoints[i+1,j+1] + Vector3.up * h);
                        vertices.Add(basePoints[i+1,j] + Vector3.up * h);
                        triangles.AddRange(new [] {n, n+1, n+2, n, n+2, n+3});

                        freeside = baseData[i - 1,j] == 0;
                        n = vertices.Count;
                        vertices.Add(basePoints[i,j]);
                        vertices.Add(basePoints[i,j+1]);
                        vertices.Add(basePoints[i,j+1] + Vector3.up * h);
                        vertices.Add(basePoints[i,j] + Vector3.up * h);
                        triangles.AddRange(new [] {n, n+1, n+2, n, n+2, n+3});
                        if (freeside) ProcSide(vertices[n], vertices[n+1], vertices[n+2], vertices[n+3]);

                        freeside = baseData[i,j + 1] == 0;
                        n = vertices.Count;
                        vertices.Add(basePoints[i,j+1]);
                        vertices.Add(basePoints[i+1,j+1]);
                        vertices.Add(basePoints[i+1,j+1] + Vector3.up * h);
                        vertices.Add(basePoints[i,j+1] + Vector3.up * h);
                        triangles.AddRange(new [] {n, n+1, n+2, n, n+2, n+3});
                        if (freeside) ProcSide(vertices[n], vertices[n+1], vertices[n+2], vertices[n+3]);

                        freeside = baseData[i + 1,j] == 0;
                        n = vertices.Count;
                        vertices.Add(basePoints[i+1,j+1]);
                        vertices.Add(basePoints[i+1,j]);
                        vertices.Add(basePoints[i+1,j] + Vector3.up * h);
                        vertices.Add(basePoints[i+1,j+1] + Vector3.up * h);
                        triangles.AddRange(new [] {n, n+1, n+2, n, n+2, n+3});
                        if (freeside) ProcSide(vertices[n], vertices[n+1], vertices[n+2], vertices[n+3]);

                        freeside = baseData[i,j - 1] == 0;
                        n = vertices.Count;
                        vertices.Add(basePoints[i+1,j]);
                        vertices.Add(basePoints[i,j]);
                        vertices.Add(basePoints[i,j] + Vector3.up * h);
                        vertices.Add(basePoints[i+1,j] + Vector3.up * h);
                        triangles.AddRange(new [] {n, n+1, n+2, n, n+2, n+3});
                        if (freeside) ProcSide(vertices[n], vertices[n+1], vertices[n+2], vertices[n+3]);

                    } else {

                        vertices.Add(basePoints[i,j]);
                        vertices.Add(basePoints[i,j+1]);
                        vertices.Add(basePoints[i+1,j+1]);
                        vertices.Add(basePoints[i+1,j]);

                        triangles.AddRange(new [] {n, n+1, n+2, n, n+2, n+3});
                    }

                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        // mesh.RecalculateTangents();

        GameObject newMesh = new GameObject("Mesh");
        newMesh.AddComponent<MeshFilter>().sharedMesh = mesh;
        newMesh.AddComponent<MeshCollider>().sharedMesh = mesh;
        newMesh.AddComponent<MeshRenderer>().sharedMaterial = materials[dataIndex];
        newMesh.transform.parent = container.transform;
    }

    public GameObject lightPrefab; 
    void ProcSide(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        if (Random.value < 0.1f && C.y > 3f) {
            Vector3 normal = -new Vector3(-(B.z - A.z), 0f, B.x - A.x);
            Vector3 pos = ((A+B+C+D)/4) - normal * 0.01f;
            pos.y = 3f;
            Instantiate(lightPrefab, pos, Quaternion.LookRotation(normal, Vector3.up)).transform.parent = container.transform;
        }
    }

    // void OnDrawGizmos()
    // {
    //     if (false) {
    //         for (int i = 0; i < numWidth; i++) {
    //             for (int j = 0; j < numHeight; j++) {   
    //                 Gizmos.DrawSphere(basePoints[i,j], 0.1f);

    //                 if (ValidPoint(i, j)) {
    //                     if (i < numWidth - 1 && ValidPoint(i + 1, j)) Gizmos.DrawLine(basePoints[i,j], basePoints[i + 1,j]);
    //                     if (j < numHeight - 1 && ValidPoint(i, j + 1)) Gizmos.DrawLine(basePoints[i,j], basePoints[i,j + 1]);
    //                 }

    //             }
    //         }

    //     }
    // }

    bool ValidPoint(int i, int j, bool side = false)
    {
        float xPos = ((float) i) / numWidth;
        float yPos = ((float) j) / numHeight;
        if (mapTexture != null) {
            return mapTexture.GetPixel(Mathf.FloorToInt(xPos * mapTexture.width), Mathf.FloorToInt(yPos * mapTexture.height)).r >= threshold;
        } else {
            return basePoints[i, j].magnitude < Mathf.Min(areaWidth/2, areaHeight/2);
        }
    }

    bool ready = false;
    void OnValidate()
    {
        ready = false;
        // Build();
    }

    static float SqrDistance(Vector3 a, Vector3 b)
    {
        float diff_x = a.x - b.x;
        float diff_y = a.y - b.y;
        float diff_z = a.z - b.z;
        return (float)(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
    }
}
