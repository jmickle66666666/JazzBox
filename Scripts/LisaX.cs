using System.Collections.Generic;
using UnityEngine;

public class LisaX
{
    Dictionary<string, List<string>> script;
    public Dictionary<string, string> properties;
    public Dictionary<string, (System.Action<string[]> method, bool continues)> methodHooks; 

    public bool running;

    Stack<(string label, int line)> callStack;
    (string label, int line) currentPosition;

    public LisaX()
    {
        methodHooks = new Dictionary<string, (System.Action<string[]> method, bool continues)>();
        callStack = new Stack<(string, int)>();

        // Built-in methods
        AddMethodHook("end", () => {}, false);
        AddMethodHook("inc", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (int.Parse(properties[data[1]]) + 1).ToString();
            } else {
                properties.Add(data[1], "1");
            }
        });
        AddMethodHook("dec", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (int.Parse(properties[data[1]]) - 1).ToString();
            } else {
                properties.Add(data[1], "-1");
            }
        });
        AddMethodHook("add", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (int.Parse(properties[data[1]]) + int.Parse(data[2])).ToString();
            } else {
                properties.Add(data[1], "1");
            }
        });
        AddMethodHook("goto", (data) => {
            callStack.Push(currentPosition);
            RunLabel(data[1]);
        }, false);
        AddMethodHook("sub", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (int.Parse(properties[data[1]]) - int.Parse(data[2])).ToString();
            } else {
                properties.Add(data[1], "1");
            }
        });
        AddMethodHook("set", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = data[2];
            } else {
                properties.Add(data[1], data[2]);
            }
        });
        AddMethodHook("return", (data) => {
            if (callStack.Count > 0) {
                var pos = callStack.Pop();
                RunLabel(pos.label, pos.line);
            }
        });
    }

    public void AddMethodHook(string methodName, System.Action<string[]> method, bool continues = true)
    {
        methodHooks.Add(methodName, (method, continues));
    }

    public void AddMethodHook(string methodName, System.Action method, bool continues = true)
    {
        methodHooks.Add(methodName, ((data) => {method.Invoke();}, continues));
    }

    public void LoadScript(string[] data)
    {
        script = new Dictionary<string, List<string>>();
        properties = new Dictionary<string, string>();
        string currentLabel = "START";
        script.Add(currentLabel, new List<string>());

        for (int i = 0; i < data.Length; i++) {
            if (data[i].StartsWith("!")) {
                string[] tokens = data[i].Split(' ');
                if (tokens.Length >= 3) {
                    properties.Add(tokens[1], tokens[Random.Range(2, tokens.Length)]);
                } else {
                    Debug.LogWarning($"Parse error at line {i}: expected >=3 tokens, got {tokens.Length}.  {data[i]}");
                }
            } else if (data[i].StartsWith(":")) {
                string[] tokens = data[i].Split(' ');
                if (tokens.Length == 2) {
                    if (script.ContainsKey(tokens[1])) {
                        Debug.LogWarning($"Parse error at line {i}: label {tokens[1]} already exists.  {data[i]}");
                    } else {
                        currentLabel = tokens[1];
                        script.Add(tokens[1], new List<string>());
                    }
                }
            } else {
                script[currentLabel].Add(data[i]);
            }
        }

        RunLabel("START");
    }

    public void RunLabel(string label, int start = 0) {
        if (!script.ContainsKey(label)) {
            Debug.Log($"Can't find label {label}");
            return;
        }

        int line = start;
        currentPosition.label = label;
        currentPosition.line = line+1;
        while (line < script[label].Count && RunLine(script[label][line])) {
            line += 1;
            currentPosition.line = line+1;
        }
    }

    public string[] ParseTokens(string line)
    {
        List<string> output = new List<string>();
        bool inQuotes = false;


        line = line.Replace("|", "\n");

        string currentToken = "";
        for (int i = 0; i < line.Length; i++) {
            if ((!inQuotes) && line[i] == ' ') {
                output.Add(currentToken);
                currentToken = "";
            } else if (line[i] == '"') {
                inQuotes = !inQuotes;
            } else {
                currentToken += line[i];
            }
        }
        output.Add(currentToken);

        for (int i = 0; i < output.Count; i++) {
            foreach (var kvp in properties) {
                output[i] = output[i].Replace($"${kvp.Key};", kvp.Value);
            }
        }

        return output.ToArray();
    }

    public bool RunLine(string line)
    {
        if (line.Length == 0) return true;
        // Debug.Log($"running line {line}");
        string[] data = ParseTokens(line);

        if (data[0] == "if") {

            if (properties.ContainsKey(data[1])) {

                if (properties[data[1]] == data[2]) {
                    callStack.Push(currentPosition);
                    RunLabel(data[3]);
                    return false;
                }

            } else {
                return true;
            }
        
        } else if (methodHooks.ContainsKey(data[0])) {

            methodHooks[data[0]].method.Invoke(data);
            return methodHooks[data[0]].continues;

        } else {
            Debug.LogWarning($"Unknown command {line}");
        }

        return true;
    }
}