using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BBQ.Editor.Tools
{
    public class TexturesToMaterials : UnityEditor.EditorWindow
    {
        [MenuItem("Assets/Create Material For Texture(s)")]
        static void CreateMaterial()
        {
            // get all selected textures in the asset browser.
            Texture2D[] textures = Array.ConvertAll(Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets), item => (Texture2D)item);

            // iterate through each selected texture:
            foreach (Texture2D texture in textures)
            {
                // create a material asset for the texture.
                Material material = new Material(Shader.Find("Standard"));
                material.SetTexture("_MainTex", texture);

                // get the directory path.
                string path = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(texture)) + "\\Materials\\";
                string file = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(texture)) + ".mat";

                AssetDatabase.CreateAsset(material, path + file);
            }
        }
    }
}
