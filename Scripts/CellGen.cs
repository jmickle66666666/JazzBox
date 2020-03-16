using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CellGen : MonoBehaviour
{
    public bool[,] tiles;
    public int width;
    public int height;
    public int seed;
    public bool randomizeSeed = false;
    [Range(0,1)]public float initialDensity = 0.5f;

    public string[] methods;
    public static string[] methodNames = {
        "Smooth", "Square", "Decay", "Feed", "Circle", "Invert"
    };

    void OnValidate()
    {
        if (methods == null) methods = new string[0];
    }

    public void Generate()
    {
        tiles = new bool[width, height];
        if (!randomizeSeed) Random.InitState(seed);

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                tiles[i, j] = Random.value < initialDensity;
            }
        }

        foreach (var m in methods) {
            if (m == "Smooth") tiles = Pass(Smooth);
            if (m == "Square") tiles = Pass(Square);
            if (m == "Decay") tiles = Pass(Decay);
            if (m == "Feed") tiles = Pass(Feed);
            if (m == "Circle") tiles = Pass(Circle);
            if (m == "Invert") tiles = Pass(Invert);
        }
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
        return false;
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

    void OnEnable()
    {
        genMethods = serializedObject.FindProperty("methods");
        widthProp = serializedObject.FindProperty("width");
        heightProp = serializedObject.FindProperty("height");
        densityProp = serializedObject.FindProperty("initialDensity");
        seedProp = serializedObject.FindProperty("seed");
        randomizeProp = serializedObject.FindProperty("randomizeSeed");
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

            rect.width = tileSize;
            rect.height = tileSize;

            for (int i = 0; i < width; i++) {
                rect.x = start.x + tileSize * i;
                for (int j = 0; j < height; j++) {
                    rect.y = start.y + tileSize * j;
                    GUI.DrawTexture(
                        rect, cellGen.tiles[i,j]?blackTexture:Texture2D.whiteTexture
                    );
                }
            }
        }

        serializedObject.ApplyModifiedProperties ();
    }
}
#endif