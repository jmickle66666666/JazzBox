using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PaletteGenerator : MonoBehaviour
{
    public int baseColorCount;
    public int shadeCount;
    
    public Color[] colors;

    public void GenerateColors()
    {
        List<Color> output = new List<Color>();

        float hueStart = Random.value;
        float hueOffset = (1f / baseColorCount);
        hueOffset *= 0.6f + Random.value * 0.4f;
        float satRange = 0.1f + Random.value * 0.3f; // 0.1 - 0.4
        float hueRange = 0.1f + Random.value * 0.1f; // 0.1 - 0.2
        float valRange = 0.4f + Random.value * 0.5f; // 0.4 - 0.9

        for (int i = 0; i < baseColorCount; i++) {
            output.AddRange(ColorRange(RandomBaseColor(hueStart + (i * hueOffset)), shadeCount, hueRange, satRange, valRange));
        }

        colors = output.ToArray();
    }

    Color RandomBaseColor(float hue)
    {
        return Color.HSVToRGB(
            hue, 
            Random.value,               // 0 - 1
            0.8f + Random.value * 0.2f  // 0.8 - 0.1
        );
    }

    Color[] ColorRange(Color baseColor, int count, float hueRange, float satRange, float valRange)
    {
        Color[] output = new Color[count];

        for (int i = 0; i < count; i++) {
            float hueDegrees = ((float) i / count) * hueRange;
            float valDegrees = ((float) i / count) * valRange;

            float dist = (float)(i - (count/2)) / count;

            float satDegrees = dist * satRange;

            hueDegrees -= hueRange / 2f;
            valDegrees -= valRange / 2f;

            output[i] = Shift(baseColor, hueDegrees, -satDegrees, -valDegrees);
        }

        return output;
    }

    Color Shift(Color color, float hueDegrees, float satDegrees, float valDegrees)
    {
        float hue;
        float sat;
        float val;
        Color.RGBToHSV(color, out hue, out sat, out val);

        hue += hueDegrees;
        if (hue < 0f) hue += 1f;
        if (hue > 1f) hue -= 1f;

        sat += satDegrees;
        val += valDegrees;
        
        sat = Mathf.Clamp01(sat);
        val = Mathf.Clamp01(val);

        return Color.HSVToRGB(hue, sat, val);
    }
}

// Simple way to add a button to a thing
#if UNITY_EDITOR
[CustomEditor(typeof(PaletteGenerator))]
public class PaletteGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate")) {
            ((PaletteGenerator) target).GenerateColors();
        }
        DrawDefaultInspector();
    }
}
#endif
