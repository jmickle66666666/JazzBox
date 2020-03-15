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

        for (int i = 0; i < baseColorCount; i++) {
            float satRange = 0.1f + Random.value * 0.2f;
            float valRange = 0.4f + Random.value * 0.4f;
            float hueRange = 0.1f + Random.value * 0.1f;
            output.AddRange(ColorRange(RandomBaseColor(hueStart + (i * hueOffset)), shadeCount, hueRange, satRange, valRange));
        }

        colors = output.ToArray();
    }

    Color RandomBaseColor(float hue)
    {
        return Color.HSVToRGB(hue, 0.5f + Random.value * 0.5f, 0.8f + Random.value * 0.2f);
    }

    Color[] ColorRange(Color baseColor, int count, float hueRange, float satRange, float valRange)
    {
        Color[] output = new Color[count];

        for (int i = 0; i < count; i++) {
            float hueDegrees = ((float) i / count) * hueRange;

            float dist = (float)(i - (count/2)) / count;

            float satDegrees = dist * satRange;
            float valDegrees = dist * valRange;

            hueDegrees -= hueRange / 2f;

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
