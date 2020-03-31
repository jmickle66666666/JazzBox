using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ActionButton : MonoBehaviour
{
    public string buttonName = "Run";
    public bool edit = true;
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
        var actionButton = (target as ActionButton);

        if (actionButton.edit) {
            DrawDefaultInspector();
            if (GUILayout.Button(actionButton.buttonName))
            {
                actionButton.Run();
            }
        } else {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(actionButton.buttonName))
            {
                actionButton.Run();
            }

            if (GUILayout.Button("Edit"))
            {
                actionButton.edit = true;
            }

            GUILayout.EndHorizontal();
        }

    }
}
#endif