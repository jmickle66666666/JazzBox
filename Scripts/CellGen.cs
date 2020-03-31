using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CellGen : MonoBehaviour
{
    public bool[,] tiles;
    public int width = 1;
    public int height = 1;
    public int seed;
    public bool randomizeSeed = false;
    [Range(0,1)]public float initialDensity = 0.5f;

    public string[] methods;
    public static string[] methodNames;

    System.Func<int, int, bool>[] posRules;
    System.Func<bool[,]>[] passRules;
    
    public Texture2D outputTexture;
    public float genTime;

    public bool previewGizmo = true;

    void InitData()
    {
        if (methods == null) methods = new string[0];
        if (posRules == null) posRules = new System.Func<int, int, bool>[] {
            Smooth, Square, Invert, Decay, Feed, Circle
        };
        if (passRules == null) passRules = new System.Func<bool[,]>[] {
            Contiguous
        };
        if (methodNames == null) {
            methodNames = new string[posRules.Length + passRules.Length];

            for (int i = 0; i < posRules.Length; i++) {
                methodNames[i] = posRules[i].Method.Name;
            }

            for (int i = 0; i < passRules.Length; i++) {
                methodNames[posRules.Length + i] = passRules[i].Method.Name;
            }
        }
    }

    void OnValidate()
    {
        InitData();   

        if (width < 1) width = 1;
        if (height < 1) height = 1;

        Generate();
    }

    public void Generate()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();

        tiles = new bool[width, height];
        if (!randomizeSeed) Random.InitState(seed);

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                tiles[i, j] = Random.value < initialDensity;
            }
        }

        foreach (var m in methods) {
            foreach (var r in posRules) {
                if (r.Method.Name == m) {
                    tiles = Pass(r);
                }
            }

            foreach (var r in passRules) {
                if (r.Method.Name == m) {
                    tiles = Pass(r);
                }
            }
        }

        outputTexture = new Texture2D(width, height);
        outputTexture.filterMode = FilterMode.Point;
        Color32[] colors = new Color32[width * height];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                colors[(j * width) + i] = tiles[i, j]?Colors.black:Colors.white;
            }
        }
        outputTexture.SetPixels32(colors);
        outputTexture.Apply();

        timer.Stop();
        genTime = (float) timer.Elapsed.TotalMilliseconds;
    }

    bool[,] Pass(System.Func<int, int, bool> rule)
    {
        bool[,] output = new bool[tiles.GetLength(0),tiles.GetLength(1)];
        for (int i = 0; i < tiles.GetLength(0); i++) {
            for (int j = 0; j < tiles.GetLength(1); j++) {
                output[i, j] = rule.Invoke(i, j);
            }
        }
        return output;
    }

    bool[,] Pass(System.Func<bool[,]> rule)
    {
        return rule.Invoke();
    }

    bool[,] Contiguous()
    {
        int[,] data = new int[width, height];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (IsWall(i, j)) {
                    data[i, j] = -1;
                } else {
                    data[i, j] = 0;
                }
            }
        }

        void FloodTile(int x, int y, int id)
        {
            if (x < 0) return;
            if (y < 0) return;
            if (x >= width) return;
            if (y >= height) return;
            if (data[x, y] != 0) return;

            data[x, y] = id;

            FloodTile(x + 1, y, id);
            FloodTile(x - 1, y, id);
            FloodTile(x, y + 1, id);
            FloodTile(x, y - 1, id);
        }

        int index = 1;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (data[i, j] == 0) {
                    FloodTile(i, j, index);
                    index += 1;
                }
            }
        }

        int[] counts = new int[index];

        for (int i = 0; i < index; i++) {
            counts[i] = 0;
        }

        int maxCount = 0;
        int maxIndex = 0;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (data[i, j] > 0) {
                    counts[data[i, j]] += 1;
                    if (counts[data[i, j]] > maxCount) {
                        maxCount = counts[data[i, j]];
                        maxIndex = data[i, j];
                    } 
                }
            }
        }

        bool[,] output = new bool[width, height];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                output[i, j] = data[i, j] != maxIndex;
            }
        }

        return output;
    }

    bool Smooth(int x, int y)
    {
        int count = Get8Count(x, y);
        if (IsWall(x, y)) return count >= 4; else return count >= 5;
    }

    bool Square(int x, int y)
    {
        int count = Get4Count(x, y);
        if (IsWall(x, y)) return count >= 3; else return count >= 4;
    }

    bool Invert(int x, int y)
    {
        return !IsWall(x, y);
    }

    bool Decay(int x, int y)
    {
        if (!IsWall(x, y)) return false;
        int count = Get8Count(x, y);
        if (count < 6) return Random.value < 0.5f;
        return IsWall(x, y);
    }

    bool Feed(int x, int y)
    {
        if (IsWall(x, y)) return true;
        int count = Get8Count(x, y);
        if (count > 2) return Random.value < 0.5f;
        return IsWall(x, y);
    }

    bool Circle(int x, int y)
    {
        int halfWidth = tiles.GetLength(0)/2;
        int halfHeight = tiles.GetLength(1)/2;
        if ((Mathf.Pow(x - halfWidth, 2) + Mathf.Pow(y - halfHeight,2)) < halfWidth*halfWidth) return IsWall(x, y);
        return true;
    }

    int Get8Count(int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) continue;
                if (IsWall(x + i, y + j, true)) {
                    count += 1;
                }
            }
        }
        return count;
    }

    int Get4Count(int x, int y)
    {
        int count = 0;
        if (IsWall(x-1, y)) count += 1;
        if (IsWall(x+1, y)) count += 1;
        if (IsWall(x, y-1)) count += 1;
        if (IsWall(x, y+1)) count += 1;
        return count;
    }

    bool IsWall(int x, int y, bool offCanvas = false)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return offCanvas;
        return tiles[x, y];
    }

    void OnDrawGizmosSelected()
    {
        if (!previewGizmo) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (tiles[i, j]) {
                    Gizmos.DrawCube(
                        new Vector3(i, 0, j),
                        Vector3.one
                    );
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CellGen))]
public class CellGenEditor : Editor
{
    static Texture2D blackTexture;

    SerializedProperty genMethods;
    SerializedProperty widthProp;
    SerializedProperty heightProp;
    SerializedProperty densityProp;
    SerializedProperty seedProp;
    SerializedProperty randomizeProp;
    SerializedProperty previewGizmoProp;

    void OnEnable()
    {
        genMethods = serializedObject.FindProperty("methods");
        widthProp = serializedObject.FindProperty("width");
        heightProp = serializedObject.FindProperty("height");
        densityProp = serializedObject.FindProperty("initialDensity");
        seedProp = serializedObject.FindProperty("seed");
        randomizeProp = serializedObject.FindProperty("randomizeSeed");
        previewGizmoProp = serializedObject.FindProperty("previewGizmo");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update ();

        if (blackTexture == null) {
            blackTexture = new Texture2D(2,2);
            blackTexture.SetPixels32(new Color32[] { new Color32(0,0,0,255), new Color32(0,0,0,255), new Color32(0,0,0,255), new Color32(0,0,0,255)});
            blackTexture.Apply();
        }

        CellGen cellGen = target as CellGen;
        if (GUILayout.Button("Generate"))
        {
            cellGen.Generate();
        
        }

        EditorGUILayout.PropertyField(previewGizmoProp);
        EditorGUILayout.PropertyField(randomizeProp);
        if (!randomizeProp.boolValue) {
            EditorGUILayout.PropertyField(seedProp);
        }
        EditorGUILayout.PropertyField(widthProp);
        EditorGUILayout.PropertyField(heightProp);
        EditorGUILayout.PropertyField(densityProp);

        for (int i = 0; i < genMethods.arraySize; i++) 
        {
            GUILayout.BeginHorizontal();

            int currentIndex = System.Array.IndexOf(CellGen.methodNames, genMethods.GetArrayElementAtIndex(i).stringValue);
            int newIndex = EditorGUILayout.Popup(currentIndex, CellGen.methodNames);
            if (newIndex != currentIndex) {
                genMethods.GetArrayElementAtIndex(i).stringValue = CellGen.methodNames[newIndex];
            }
            if (GUILayout.Button(new GUIContent("+", "Duplicate"), GUILayout.Width(20))) {
                genMethods.InsertArrayElementAtIndex(i);
                genMethods.GetArrayElementAtIndex(i).stringValue = genMethods.GetArrayElementAtIndex(i+1).stringValue;
            }

            if (i != 0) {
                if (GUILayout.Button(new GUIContent("⇑", "move up"), GUILayout.Width(20))) {
                    genMethods.MoveArrayElement(i, i-1);
                }
            } else {
                GUILayout.Space(23f);
            }

            if (i != genMethods.arraySize-1) {
                if (GUILayout.Button(new GUIContent("⇓", "move down"), GUILayout.Width(20))) {
                    genMethods.MoveArrayElement(i, i+1);
                }
            } else {
                GUILayout.Space(23f);
            }

            if (GUILayout.Button(new GUIContent("☓", "delete"), GUILayout.Width(20))) genMethods.DeleteArrayElementAtIndex(i);

            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent("+", "add new method"))) {
            genMethods.InsertArrayElementAtIndex(genMethods.arraySize);
            genMethods.GetArrayElementAtIndex(genMethods.arraySize-1).stringValue = CellGen.methodNames[0];
        }
        GUILayout.EndHorizontal();

        if (cellGen.tiles != null) {

            int width = cellGen.tiles.GetLength(0);
            int height = cellGen.tiles.GetLength(1);
            var rect = GUILayoutUtility.GetAspectRect((float) width / height);
            float tileSize = rect.width / width;

            Vector2 start = new Vector2(rect.x, rect.y);

            GUI.DrawTexture(rect, cellGen.outputTexture);
        }

        GUILayout.Label($"Generation time: {cellGen.genTime.ToString("0.00")}ms");

        serializedObject.ApplyModifiedProperties ();
    }
}
#endif