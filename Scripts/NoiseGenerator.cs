using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public Vector2Int size;
    public bool work = false;

    void Update()
    {
        if (work) {
            work = false;
            Build();
        }
    }

    void Build() 
    {
        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", LayeredNoise(size.x, size.y));
    }

    public static float[,] LayeredNoise(int width, int height, int octaves, float influence)
    {
        float[,] output = new float[width, height];

        List<float[,]> layers = new List<float[,]>();
        
        for (int l = 0; l < octaves; l++)
        {
            int octavePower = (int) Mathf.Pow(2, l);
            int w = width / octavePower;
            int h = height / octavePower;

            float[,] layer = new float[w+1,h+1];
            for (int i = 0; i < w+1; i++)
            {
                for (int j = 0; j < h+1; j++)
                {
                    layer[i, j] = Random.value;
                }
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float xf = ((float) i / width) * w;
                    float yf = ((float) j / height) * h;
                    int xi = Mathf.FloorToInt(xf);
                    int yi = Mathf.FloorToInt(yf);

                    float p00 = layer[xi, yi];
                    float p10 = layer[xi+1, yi];
                    float p01 = layer[xi, yi+1];
                    float p11 = layer[xi+1, yi+1];

                    float value = Blerp(p00, p10, p01, p11, xf - xi, yf - yi);
                    output[i, j] = Mathf.Lerp(output[i, j], value, influence);

                }
            }
        }

        float maxVal = 0f;
        float minVal = 1f;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                output[i, j] /= octaves;
                float v = output[i, j];
                if (v < minVal) minVal = v;
                if (v > maxVal) maxVal = v;
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                output[i, j] = (output[i, j] - minVal) / (maxVal - minVal);
            }
        }

        return output;
    }

    // float BilinearPoint(float[,] source, float u, float v)
    // {

    // }

    private static float Blerp(float c00, float c10, float c01, float c11, float tx, float ty) {
        return Mathf.Lerp(Mathf.Lerp(c00, c10, tx), Mathf.Lerp(c01, c11, tx), ty);
    }

    public static Texture2D LayeredNoise(int width, int height)
    {
        Texture2D noiseTexture = new Texture2D(width, height);

        int octaves = 0;
        float m = Mathf.Min(width, height);
        while (Mathf.Pow(2, octaves) < m) {
            octaves += 1;
        }
        octaves -= 1;

        Texture2D[] textures = new Texture2D[octaves];
        for (int o = 0; o < octaves; o++) {
            int spread = (int) Mathf.Pow(2, o);

            textures[o] = new Texture2D(width/spread, height/spread);
            Color32[] colors = textures[o].GetPixels32();
            for (int i = 0; i < colors.Length; i++)
            {
                byte val = (byte) Random.Range(0, 256);
                colors[i].r = val;
                colors[i].g = val;
                colors[i].b = val;
            }
            textures[o].SetPixels32(colors);
            textures[o].Apply();
        }

        float div = 0f;
        for (int o = 0; o < octaves; o++) {
            float depth = 1f / (((octaves - o))+1);
            div += depth;
        }


        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {


                float val = 0f; 
                for (int o = 0; o < octaves; o++) {
                    float depth = 1f / (((octaves - o))+1);

                    val += textures[o].GetPixelBilinear((float)i / width, (float)j/height).r * depth;
                }
                val /= div;
                noiseTexture.SetPixel(i, j, new Color(val, val, val, 1f));
            }
        }

        noiseTexture.Apply();
        return noiseTexture;
    }
}
