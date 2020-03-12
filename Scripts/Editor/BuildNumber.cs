using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


using UnityEditor;
public class BuildNumber : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
        File.WriteAllText(Application.dataPath + "/buildnum.txt", Mathf.FloorToInt(Random.value * 100000f).ToString("X"));
    }
#endif

    public static string GetBuildNum()
    {
        return File.ReadAllText(Application.dataPath + "/buildnum.txt");
    }
}