using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{

    public Vector2Int size;
    public bool work = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
