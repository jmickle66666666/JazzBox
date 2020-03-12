using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class LevelGenerator : MonoBehaviour
{

    public List<Vector3> points;
    int lastIndex = 0;
    public LineRenderer line;
    public float scale;

    public bool work = false;
    public bool reroll = false;
    public bool add = false;
    public float bruteChance = 0.5f;

    public bool reset = false;
    public int steps = 10;
    [Range(0f, 120f)]
    public float smoothness = 75f;

    public int seed;

    [Range(0f, 1f)] public float pickDistance;
    [Range(0f, 1f)] public float pickVariance;

    public int enemyCount;
    public int decorationCount;

    public bool triangulate;

    public float variance; 
    float vary {
        get {
            return (rng * variance * 2f) - 1f;
        }
    }
    Vector3 varyPoint {
        get {
            return new Vector3(
                vary, 0f, vary
            );
        }
    }

    Vector3 minPoint;
    Vector3 maxPoint;

    void Awake()
    {
        if (line == null) line = GetComponent<LineRenderer>();
    }

    void Start()
    {
        BuildFull();
    }

    private void BuildLevel()
    {
        points = new List<Vector3>();

        // Init square

        points.Add(varyPoint + new Vector3(-1f, 0f, -1f) * scale);
        points.Add(varyPoint + new Vector3(-1f, 0f, 1f) * scale);
        points.Add(varyPoint + new Vector3(1f, 0f, 1f) * scale);
        points.Add(varyPoint + new Vector3(1f, 0f, -1f) * scale);

        lastIndex = 3;

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    int GetNewIndex()
    {

        Vector3[] pList = points.ToArray();
        Vector3 lp = points[lastIndex];
        System.Array.Sort(pList, (x, y) => {
            return (int) Mathf.Sign((x - lp).sqrMagnitude - (y-lp).sqrMagnitude);
        });

        float p = 2f * (rng-0.5f) * pList.Length * pickVariance;
        p += pickDistance * pList.Length;

        int pi = Mathf.FloorToInt(p);
        if (pi < 0) pi = 0;
        if (pi > pList.Length-1) pi = pList.Length - 1;
        return points.IndexOf(pList[pi]);
    }

    private void RandomExtrude()
    {
        float length = RngRange(scale, scale * 3f); 

        int index = GetNewIndex();
        int nextIndex = (index + 1) % points.Count;
        Vector3 A = points[index];
        Vector3 D = points[nextIndex];
        Vector3 diff = D - A;
        Vector3 midPoint = A + diff * 0.5f;
        Vector3 tangent = new Vector3(-diff.z, diff.y, diff.x).normalized;
        Vector3 B = A + tangent * length;
        Vector3 C = D + tangent * length;

        B += varyPoint;
        C += varyPoint;

        if (TryAdd(new Vector3[] {B, C}, (index + 1) % points.Count)) {
            return;
        } else {
            if (rng < bruteChance) {
                RandomExtrude();
            }
        }
    }

    private void RandomTriangle()
    {
        float length = RngRange(scale*0.25f, scale*1.25f);

        int index = GetNewIndex();
        int nextIndex = (index + 1) % points.Count;
        Vector3 A = points[index];
        Vector3 D = points[nextIndex];
        Vector3 diff = D - A;
        Vector3 midPoint = A + diff * 0.5f;
        Vector3 tangent = new Vector3(-diff.z, diff.y, diff.x).normalized;
        Vector3 B = midPoint + tangent * length;

        B += varyPoint;

        if (TryAdd(new Vector3[] {B}, (index + 1) % points.Count)) {
            return;
        } else {
            if (rng < bruteChance) {
                RandomTriangle();
            }
        }
    }

    private void RemoveSharps()
    {
        if (points.Count < 4) return;
        for (int i = 0; i < points.Count; i++) {
            int indexA = i;
            int indexB = (i + 1) % points.Count;
            int indexC = (i + 2) % points.Count;

            Vector3 v1 = points[indexA];
            Vector3 v2 = points[indexB];
            Vector3 v3 = points[indexC];

            float ang = LinesAngle(v1,v2,v3);
            if (ang > -smoothness && ang < smoothness) {
                for (int j = 0; j < points.Count; j++) {
                    if (!LinesIntersect(points[j], points[(j+1)], v1, v3)) {
                        points.RemoveAt(indexB);
                        RemoveSharps();
                        return;
                    }
                }
            }
        }
    }

    private bool TryAdd(Vector3[] addPoints, int index)
    {
        // first check intersections
        // Debug.Log($"{index}, {(index+ points.Count-1) % points.Count}");
        Vector3 fromPoint = points[(index+ points.Count-1) % points.Count];
        Vector3 toPoint = points[index];

        for (int i = 0; i < points.Count; i++) {
            if (LinesIntersect(points[i], points[(i+1)%points.Count], fromPoint, addPoints[0])) {
                // Debug.DrawLine(points[i], points[(i+1)%points.Count], Color.magenta, 5f);
                // Debug.DrawLine(fromPoint, addPoints[0], Color.red, 5f);
                return false;
            }
            if (LinesIntersect(points[i], points[(i+1)%points.Count], addPoints[addPoints.Length-1], toPoint)) {
                // Debug.DrawLine(points[i], points[(i+1)%points.Count], Color.magenta, 5f);
                // Debug.DrawLine(addPoints[addPoints.Length-1], toPoint, Color.red, 5f);
                return false;
            }

            for (int j = 0; j < addPoints.Length-1; j++) {
                if (LinesIntersect(points[i], points[(i+1)%points.Count], addPoints[j], addPoints[j+1])) {
                    // Debug.DrawLine(points[i], points[(i+1)%points.Count], Color.magenta, 5f);
                    // Debug.DrawLine(addPoints[j], addPoints[j+1], Color.red, 5f);
                    return false;
                }
            }
        }

        // points.InsertRange(index, addPoints);
        for (int i = 0; i < addPoints.Length; i++) {
            points.Insert(index + i, addPoints[i]);
        }

        lastIndex = index;

        return true;
    }

    public Vector3 RandomPosition()
    {
        int index = RngRange(0, points.Count-1);
        Vector3 p1 = points[index];
        Vector3 p2 = points[(index + 1) % points.Count];
        Vector3 p3 = points[(index + 2) % points.Count];
        Vector3 average = (p1 + p2 + p3) / 3f;
        if (PointInPolygon(average, points)) {
            return average;
        } else {
            return RandomPosition();
        }
    }

    public static bool PointInPolygon(Vector3 point, List<Vector3> polygon, bool ingoreConnection = false) {
        int crosses = 0;

        if (ingoreConnection) {
            for (int i = 0; i < polygon.Count; i++) {
                if (point.Equals(polygon[i])) return false;
            }
        }

        Vector3 leftPoint = new Vector3(point.x - 10000f, point.y);
        for (int i = 0; i < polygon.Count; i++) {
            int i2 = (i + 1) % polygon.Count;

            if (LinesIntersect(leftPoint, point, polygon[i], polygon[i2])) {
                crosses += 1;
            }
        }

        return (crosses % 2 == 1);
    }

    private static float LinesAngle(Vector3 A, Vector3 B, Vector3 C) {
        Vector2 BA = new Vector2(A.x - B.x, A.z - B.z);
        Vector2 BC = new Vector2(C.x - B.x, C.z - B.z);
        float dot = Vector2.Dot(BA.normalized, BC.normalized);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        return angle;
    }

    private static bool LinesIntersect(Vector3 A, Vector3 B, Vector3 C, Vector3 D) {

        if (A.Equals(C) || A.Equals(D) || B.Equals(C) || B.Equals(D)) {
            return false;
        }

        Vector2 a = new Vector2(A.x, A.z);
        Vector2 b = new Vector2(B.x, B.z);
        Vector2 c = new Vector2(C.x, C.z);
        Vector2 d = new Vector2(D.x, D.z);
        return LinesIntersect(a,b,c,d);
    }

    private static bool LinesIntersect(Vector2 A, Vector2 B, Vector2 C, Vector2 D) {
		return (CCW(A,C,D) != CCW(B,C,D)) && (CCW(A,B,C) != CCW(A,B,D));
    }

    private static bool CCW(Vector2 A, Vector2 B, Vector2 C) {
        return ((C.y-A.y) * (B.x-A.x) > (B.y-A.y) * (C.x-A.x));
    }

    public float meshVary = 1f;
    Vector3 meshVaryPoint {
        get {
            return new Vector3(
                ((-1f + (rng * 2f)) * meshVary),  
                ((-1f + (rng * 2f)) * meshVary),
                ((-1f + (rng * 2f)) * meshVary)  
            );
        }
    }
    public float heightOffset = 3f;
    public Material[] wallMaterials;
    public Material groundMaterial;
    public int heightSteps = 2;

    void BuildMesh()
    {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        List<Vector3[]> pointArrays = new List<Vector3[]>();
        pointArrays.Add(points.ToArray());
        for (int i = 1; i < heightSteps + 1; i++) {
            Vector3[] newPointArray = new List<Vector3>(pointArrays[i-1]).ToArray();
            for (int j = 0; j < newPointArray.Length; j++) {
                newPointArray[j] += meshVaryPoint + Vector3.up * heightOffset;
            }
            pointArrays.Add(newPointArray);
        }


        for (int h = 0; h < heightSteps; h++) {

            List<Vector3> verts = new List<Vector3>();
            verts.AddRange(pointArrays[h]);
            verts.AddRange(pointArrays[h+1]);

            // Vector3[] vertices = new Vector3[points.Count * (heightSteps + 1)];
            Vector3[] vertices = verts.ToArray();
            int[] triangles = new int[points.Count * 6];

            for (int i = 0; i < points.Count; i++) {
                // vertices[i] = points[i];
                // vertices[i + points.Count] = points[i] + meshVaryPoint + Vector3.up * heightOffset;
                int i1 = i;
                int i2 = (i + 1) % points.Count;
                int i3 = i1 + points.Count;
                int i4 = i2 + points.Count;

                triangles[(i * 6) + 0] = i1;
                triangles[(i * 6) + 1] = i4;
                triangles[(i * 6) + 2] = i2;
                triangles[(i * 6) + 3] = i1;
                triangles[(i * 6) + 4] = i3;
                triangles[(i * 6) + 5] = i4;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            GameObject walls = new GameObject("walls");
            walls.AddComponent<MeshFilter>().sharedMesh = mesh;
            walls.AddComponent<MeshRenderer>().sharedMaterial = wallMaterials[h % wallMaterials.Length];
            walls.AddComponent<MeshCollider>().sharedMesh = mesh;
            walls.transform.parent = transform;
            walls.layer = LayerMask.NameToLayer("Walls");
        }

        // lastMesh = walls;
    }

    // Update is called once per frame
    void Update()
    {
        if (reset) {
            reset = false;
            BuildLevel();
        } 

        if (add) {
            add = false;
            if (rng < 0.5f) {
                RandomTriangle();
            } else {
                RandomExtrude();
            }
        } 

        if (reroll) {
            reroll = false;
            seed = Random.Range(0,256);
            work = true;
        }

        if (work)
        {
            work = false;
            BuildFull();
        }

    }

    void LateUpdate() {
        if (ignoreValidate) {
            BuildFull();
            ignoreValidate = false;
        }
    }

    bool ignoreValidate = false;
    void OnValidate()
    {
        if (ignoreValidate) return;
        ignoreValidate = true;
    }

    private void BuildFull()
    {
        rngTablePosition = seed;

        BuildLevel();

        for (int i = 0; i < steps; i++)
        {
            if (rng < 0.5f)
            {
                RandomTriangle();
            }
            else
            {
                RandomExtrude();
            }
        }

        RemoveSharps();

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        BuildMesh();

        minPoint = Vector3.zero;
        maxPoint = Vector3.zero;
        for (int i = 0; i < points.Count; i++) {
            if (points[i].x < minPoint.x) minPoint.x = points[i].x;
            if (points[i].z < minPoint.z) minPoint.z = points[i].z;
            if (points[i].x > maxPoint.x) maxPoint.x = points[i].x;
            if (points[i].z > maxPoint.z) maxPoint.z = points[i].z;
        }

        if (triangulate) {
            Triangulate();
        }
    }

    void Triangulate()
    {
        Vector2[] shape = new Vector2[points.Count + 1];
        for (int i = 0; i < points.Count; i++) {
            shape[i] = new Vector2(points[i].x, points[i].z);
        }
        shape[points.Count] = new Vector2(points[0].x, points[0].z);
        // System.Array.Reverse(shape);

        List<Vector2[]> decomposition = Decomposition.SeidelDecomposer.ConvexPartition(shape);
        
        List<CombineInstance> combines = new List<CombineInstance>();

        decomposition.ForEach(d => {

            CombineInstance ci = new CombineInstance();
            ci.mesh = MeshFrom2DShape(d);
            ci.transform = Matrix4x4.identity;
            combines.Add(ci);

        });

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combines.ToArray());
        GameObject ground = new GameObject("Ground");
        ground.AddComponent<MeshFilter>().sharedMesh = mesh;
        ground.AddComponent<MeshRenderer>().material = groundMaterial;
        ground.AddComponent<MeshCollider>().sharedMesh = mesh;
        ground.transform.parent = transform;

    }

    private static bool IsClockwise(ref Vector2[] polygon) {
        float count = 0;
        for (int i = 0; i < polygon.Length; i++) {
            int i2 = (i + 1) % polygon.Length;
            count += polygon[i].x * polygon[i2].y - polygon[i2].x * polygon[i].y;
        }

        return (count <= 0);
    }

    Mesh MeshFrom2DShape(Vector2[] subsector)
    {
        Vector3[] vertices = new Vector3[subsector.Length];

        for (int i = 0; i < subsector.Length; i++) {
            vertices[i] = new Vector3(
                subsector[i].x,
                0f,
                subsector[i].y
            );
        }

        if (!IsClockwise(ref subsector)) {
            System.Array.Reverse(vertices);
        }

        int[] triangles = new int[vertices.Length * 3];
        for (int i = 0; i < vertices.Length; i++) {
            triangles[(i * 3) + 0] = 0;
            triangles[(i * 3) + 1] = (i + 1) % vertices.Length;
            triangles[(i * 3) + 2] = (i + 2) % vertices.Length;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = subsector;
        mesh.RecalculateNormals();
        return mesh;
    }

    Vector3 GetPoint() {
        Vector3 output = new Vector3(
            minPoint.x + rng * (maxPoint.x - minPoint.x),
            0f,
            minPoint.z + rng * (maxPoint.z - minPoint.z)
        );

        if (PointInPolygon(output, points)) {
            return output;
        } else {
            return GetPoint();
        }
    }

    private static float[] rngTable = new float[] {
        0.097f, 0.318f, 0.724f, 0.251f, 0.861f, 0.64f, 0.426f, 0.657f, 0.872f, 0.079f, 0.925f, 0.426f, 0.903f, 0.164f, 0.457f, 0.946f, 0.449f, 0.218f, 0.842f, 0.953f, 0.461f, 0.783f, 0.912f, 0.933f, 0.83f, 0.739f, 0.335f, 0.639f, 0.978f, 0.461f, 0.63f, 0.319f, 0.636f, 0.391f, 0.279f, 0.785f, 0.75f, 0.878f, 0.36f, 0.793f, 0.379f, 0.613f, 0.802f, 0.564f, 0.343f, 0.97f, 0.364f, 0.951f, 0.247f, 0.348f, 0.185f, 0.11f, 0.446f, 0.985f, 0.729f, 0.845f, 0.165f, 0.202f, 0.808f, 0.132f, 0.107f, 0.45f, 0.311f, 0.165f, 0.237f, 0.512f, 0.841f, 0.983f, 0.425f, 0.767f, 0.027f, 0.263f, 0.444f, 0.706f, 0.941f, 0.624f, 0.298f, 0.396f, 0.002f, 0.419f, 0.664f, 0.084f, 0.764f, 0.891f, 0.427f, 0.881f, 0.603f, 0.638f, 0.64f, 0.288f, 0.313f, 0.901f, 0.207f, 0.099f, 0.002f, 0.61f, 0.472f, 0.185f, 0.198f, 0.568f, 0.833f, 0.496f, 0.172f, 0.461f, 0.629f, 0.607f, 0.868f, 0.479f, 0.516f, 0.41f, 0.782f, 0.582f, 0.168f, 0.803f, 0.494f, 0.377f, 0.721f, 0.198f, 0.56f, 0.018f, 0.055f, 0.634f, 0.577f, 0.61f, 0.475f, 0.127f, 0.409f, 0.439f, 0.928f, 0.284f, 0.423f, 0.585f, 0.207f, 0.043f, 0.48f, 0.786f, 0.078f, 0.846f, 0.602f, 0.503f, 0.244f, 0.259f, 0.489f, 0.892f, 0.932f, 0.478f, 0.03f, 0.619f, 0.983f, 0.324f, 0.034f, 0.165f, 0.657f, 0.562f, 0.256f, 0.117f, 0.041f, 0.595f, 0.731f, 0.337f, 0.444f, 0.391f, 0.481f, 0.996f, 0.183f, 0.96f, 0.155f, 0.048f, 0.353f, 0.589f, 0.953f, 0.948f, 0.264f, 0.992f, 0.873f, 0.81f, 0.349f, 0.553f, 0.584f, 0.322f, 0.974f, 0.919f, 0.952f, 0.42f, 0.742f, 0.852f, 0.601f, 0.81f, 0.195f, 0.492f, 0.919f, 0.846f, 0.988f, 0.308f, 0.23f, 0.974f, 0.082f, 0.837f, 0.537f, 0.934f, 0.297f, 0.257f, 0.802f, 0.268f, 0.66f, 0.931f, 0.386f, 0.863f, 0.134f, 0.745f, 0.148f, 0.923f, 0.78f, 0.367f, 0.958f, 0.803f, 0.027f, 0.216f, 0.845f, 0.85f, 0.109f, 0.126f, 0.101f, 0.299f, 0.211f, 0.204f, 0.399f, 0.108f, 0.995f, 0.517f, 0.721f, 0.185f, 0.751f, 0.173f, 0.846f, 0.152f, 0.975f, 0.524f, 0.357f, 0.551f, 0.996f, 0.661f, 0.071f, 0.345f, 0.56f, 0.919f, 0.773f, 0.791f, 0.784f, 0.231f, 0.236f, 0.221f, 0.604f, 0.486f, 0.702f, 0.185f
    };
    int rngTablePosition = 0;
    private float rng {
        get {
            rngTablePosition = (rngTablePosition + 1) % rngTable.Length;
            return rngTable[rngTablePosition];
        }
    }
    private float RngRange(float min, float max)
    {
        return min + rng * max;
    }
    private int RngRange(int min, int max)
    {
        return min + Mathf.FloorToInt(rng * max);
    }
}
