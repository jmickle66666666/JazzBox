using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ActionButton : MonoBehaviour
{
    public UnityEvent action;

    public void Run()
    {
        action.Invoke();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ActionButton))]
public class ActionButtonInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var actionButton = (target as ActionButton);
        if (GUILayout.Button("Run"))
        {
            actionButton.Run();
        }
    }
}
#endif