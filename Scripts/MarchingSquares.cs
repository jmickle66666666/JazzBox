using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MarchingSquares : MonoBehaviour
{
    public bool[,] tiles;

    public void SetTiles(bool[,] tiles) {
        this.tiles = tiles;
    }

    Vector2[][] squaresData;
    List<Vector2[]> squares;
    [HideInInspector] public Mesh outputMesh;
    

    public void Generate(float scaleX = 1f, float scaleY = 1f, float scaleZ = 1f)
    {
        squares = new List<Vector2[]>();
        // StandardTiles();
        CurveTiles();

        for (int i = 0; i < tiles.GetLength(0)-1; i++)
        {
            for(int j = 0; j < tiles.GetLength(1)-1; j++)
            {
                int squareIndex = 0;
                if (IsTile(i, j, true)) squareIndex += 1;
                if (IsTile(i+1, j, true)) squareIndex += 2;
                if (IsTile(i, j+1, true)) squareIndex += 4;
                if (IsTile(i+1, j+1, true)) squareIndex += 8;

                var lines = squaresData[squareIndex];
                var offset = new Vector2(i, j);
                squares.Add(WithOffset(lines, offset));
            }
        }

        outputMesh = BuildMesh(new Vector3(scaleX, scaleY, scaleZ));
    }

    public Mesh BuildMesh(Vector3 scale)
    {
        Mesh output = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        foreach (var s in squares)
        {
            for (int i = 0; i < s.Length; i++) 
            {
                if (i != s.Length-1) {
                    int vCount = vertices.Count;
                    triangles.AddRange(new [] { vCount + 0, vCount + 1, vCount + 2, vCount + 1, vCount + 3, vCount+2 });
                }

                vertices.Add(new Vector3(s[i].x * scale.x, 0f * scale.y, s[i].y * scale.z));
                vertices.Add(new Vector3(s[i].x * scale.x, 1f * scale.y, s[i].y * scale.z));

                uvs.Add(new Vector2(s[i].x + s[i].y, 0f));
                uvs.Add(new Vector2(s[i].x + s[i].y, 1f));
            }
        }

        output.SetVertices(vertices);
        output.SetUVs(0, uvs);
        output.SetTriangles(triangles, 0);
        output.RecalculateNormals();
        output.RecalculateBounds();
        return output;
    }

    Vector2[] WithOffset(Vector2[] points, Vector2 offset)
    {
        Vector2[] output = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            output[i] = points[i] + offset;
        }
        return output;
    }

    bool IsTile(int x, int y, bool offMap)
    {
        if (x < 0) return offMap;
        if (y < 0) return offMap;
        if (x >= tiles.GetLength(0)) return offMap;
        if (y >= tiles.GetLength(1)) return offMap;
        return tiles[x, y];
    }

    void StandardTiles() {
        squaresData = new Vector2[16][];
        squaresData[0] = new Vector2[0];
        squaresData[1] = new Vector2[2] { topPoint, leftPoint };
        squaresData[3] = new Vector2[2] { rightPoint, leftPoint };
        squaresData[7] = new Vector2[2] { rightPoint, bottomPoint };
        squaresData[15] = new Vector2[0];
        FillTiles();
    }

    void SquareTiles() {
        squaresData = new Vector2[16][];
        squaresData[0] = new Vector2[0];
        squaresData[1] = new Vector2[4] {  topPoint, centerPoint, centerPoint, leftPoint };
        squaresData[3] = new Vector2[2] { rightPoint, leftPoint };
        squaresData[7] = new Vector2[4] {  rightPoint, centerPoint, centerPoint, bottomPoint };
        squaresData[15] = new Vector2[0];
        FillTiles();
    }

    void CurveTiles() {
        squaresData = new Vector2[16][];
        squaresData[0] = new Vector2[0];
        squaresData[1] = new Vector2[4] { topPoint, sq00, sq00, leftPoint };
        squaresData[3] = new Vector2[2] { rightPoint, leftPoint };
        squaresData[7] = new Vector2[4] {  rightPoint, sq11, sq11, bottomPoint };
        squaresData[15] = new Vector2[0];
        FillTiles();
    }

    Vector2 topPoint = new Vector2(0.5f, 0f);
    Vector2 bottomPoint = new Vector2(0.5f, 1f);
    Vector2 leftPoint = new Vector2(0f, 0.5f);
    Vector2 rightPoint = new Vector2(1f, 0.5f);
    Vector2 centerPoint = new Vector2(0.5f, 0.5f);
    Vector2 sq00 = new Vector2(0.36f, 0.36f);
    Vector2 sq01 = new Vector2(0.64f, 0.36f);
    Vector2 sq10 = new Vector2(0.36f, 0.64f);
    Vector2 sq11 = new Vector2(0.64f, 0.64f);

    void FillTiles() {
        squaresData[2] = RotateDataCW(squaresData[1]);
        squaresData[4] = RotateDataCCW(squaresData[1]);
        squaresData[5] = RotateDataCCW(squaresData[3]);
        squaresData[6] = squaresData[2].Concat(squaresData[4]).ToArray();
        squaresData[8] = RotateDataCW(squaresData[2]);
        squaresData[9] = squaresData[1].Concat(squaresData[8]).ToArray();
        squaresData[10] = RotateDataCW(squaresData[3]);
        squaresData[11] = RotateDataCW(squaresData[7]);
        squaresData[12] = RotateDataCW(squaresData[10]);
        squaresData[13] = RotateDataCCW(squaresData[7]);
        squaresData[14] = RotateDataCCW(squaresData[13]);
    }

    Vector2[] RotateDataCW(Vector2[] data) {
        Vector2[] output = new Vector2[data.Length];
        for (int i = 0; i < data.Length; i++) {
            output[i] = Rotate(0.5f, 0.5f, data[i].x, data[i].y, -90f);
        }
        return output;
    }

    Vector2[] RotateDataCCW(Vector2[] data) {
        Vector2[] output = new Vector2[data.Length];
        for (int i = 0; i < data.Length; i++) {
            output[i] = Rotate(0.5f, 0.5f, data[i].x, data[i].y, 90f);
        }
        return output;
    }

    Vector2 Rotate(float cx, float cy, float x, float y, float angle) {
        float radians = (Mathf.PI / 180) * angle;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        float nx = (cos * (x - cx)) + (sin * (y - cy)) + cx;
        float ny = (cos * (y - cy)) - (sin * (x - cx)) + cy;
        return new Vector2(nx, ny);
    }
}
