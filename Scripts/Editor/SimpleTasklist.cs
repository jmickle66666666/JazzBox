using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SimpleTask
{
    public bool done;
    public string description;
    public static SimpleTask Deserialize(string data)
    {
        return new SimpleTask() {
            done = data[0] == '1',
            description = data.Substring(1)
        };
    }
    public string Serialize()
    {
        string doneText = done?"1":"0";
        return $"{doneText}{description}\n";
    }
}

public class SimpleTasklist : EditorWindow
{
    public List<SimpleTask> tasks;
    public List<SimpleTask> toRemove;
    string savePath = "Assets/tasklist.txt";

    bool addNew = false;
    string newContent = "";

    [MenuItem("Tools/Tasklist")]
	static void Init() {
		SimpleTasklist taskList = (SimpleTasklist)EditorWindow.GetWindow(typeof(SimpleTasklist));
        taskList.toRemove = new List<SimpleTask>();
        taskList.LoadTasks();
        taskList.addNew = false;
		taskList.Show();
	}

    void OnGUI()
    {
        if (tasks == null) {
            LoadTasks();
            toRemove = new List<SimpleTask>();
        }
        
        if (addNew) {
            GUILayout.BeginHorizontal();
            newContent = GUILayout.TextField(newContent);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add")) {
                tasks.Add(new SimpleTask() {
                    done = false,
                    description = newContent
                });
                SaveTasks();
                addNew = false;
            }

            if (GUILayout.Button("Cancel")) {
                addNew = false;
            }

            GUILayout.EndHorizontal();
        } else {
            if (GUILayout.Button("new")) {
                addNew = true;
                newContent = "";
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Todo:");
        GUILayout.Space(5);
        foreach(var task in tasks)
        {
            if (!task.done) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(task.description);
                if (GUILayout.Button("Done", GUILayout.Width(40))) {
                    task.done = true;
                    SaveTasks();
                }
                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    toRemove.Add(task);
                }
                GUILayout.EndHorizontal();
            }
        }

        

        GUILayout.Space(10);
        GUILayout.Label("Done:");
        GUILayout.Space(5);

        foreach(var task in tasks)
        {
            if (task.done) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(task.description);
                if (GUILayout.Button("Undo", GUILayout.Width(40))) {
                    task.done = false;
                    SaveTasks();
                }
                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    toRemove.Add(task);
                }
                GUILayout.EndHorizontal();
            }
        }

        while (toRemove.Count > 0)
        {
            tasks.Remove(toRemove[0]);
            toRemove.RemoveAt(0);
            SaveTasks();
        }
    }

    void LoadTasks() {
        tasks = new List<SimpleTask>();
        if (File.Exists(savePath)) {
            Debug.Log("Hello!");
            string[] lines = File.ReadAllLines(savePath);
            foreach (var l in lines)
            {
                tasks.Add(SimpleTask.Deserialize(l));
            }
        }
    }

    void SaveTasks() {
        if (File.Exists(savePath)) {
            File.Delete(savePath);
        }

        string output = "";
        foreach (var task in tasks) {
            output += task.Serialize();
        }

        File.WriteAllText(savePath, output);
    }

}