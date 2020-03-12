using System.Collections.Generic;
using UnityEngine;

public class LisaX
{
    Dictionary<string, List<string>> script;
    public Dictionary<string, string> properties;
    public Dictionary<string, (System.Action<string[]> method, bool continues)> methodHooks; 

    public bool running;

    public LisaX(string[] data)
    {
        methodHooks = new Dictionary<string, (System.Action<string[]> method, bool continues)>();
        LoadScript(data);
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
    }

    public void RunLabel(string label) {
        if (!script.ContainsKey(label)) return;
        int line = 0;
        while (line < script[label].Count && RunLine(script[label][line])) {
            line += 1;
        }
    }

    public string[] ParseTokens(string line)
    {
        List<string> output = new List<string>();
        bool inQuotes = false;

        foreach (var kvp in properties) {
            line = line.Replace($"${kvp.Key};", kvp.Value);
        }

        line = line.Replace("|", "\n");

        string currentToken = "";
        for (int i = 0; i < line.Length; i++) {
            if (!inQuotes && line[i] == ' ') {
                output.Add(currentToken);
                currentToken = "";
            } else if (line[i] == '"') {
                inQuotes = !inQuotes;
            } else {
                currentToken += line[i];
            }
        }
        output.Add(currentToken);

        return output.ToArray();
    }

    public bool RunLine(string line)
    {
        if (line.Length == 0) return true;
        string[] data = ParseTokens(line);

        if (line.StartsWith("end")) {
            return false;
        } else if (line.StartsWith("inc")) {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (int.Parse(properties[data[1]]) + 1).ToString();
            } else {
                properties.Add(data[1], "1");
            }
            return true;
        } else if (line.StartsWith("dec")) {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (int.Parse(properties[data[1]]) - 1).ToString();
            } else {
                properties.Add(data[1], "-1");
            }
            return true;
        } else if (line.StartsWith("add")) {
            if (properties.ContainsKey(data[1]) && properties.ContainsKey(data[2])) {
                properties[data[1]] = (int.Parse(properties[data[1]]) + int.Parse(properties[data[2]])).ToString();
            } else {
                properties.Add(data[1], "1");
            }
            return true;
        } else if (line.StartsWith("goto")) {
            RunLabel(data[1]);
            return false;
        } else if (line.StartsWith("sub")) {
            if (properties.ContainsKey(data[1]) && properties.ContainsKey(data[2])) {
                properties[data[1]] = (int.Parse(properties[data[1]]) - int.Parse(properties[data[2]])).ToString();
            } else {
                properties.Add(data[1], "1");
            }
            return true;
        } else if (line.StartsWith("set")) {

            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = data[2];
            } else {
                properties.Add(data[1], data[2]);
            }
            return true;

        } else if (line.StartsWith("if")) {

            if (properties.ContainsKey(data[1])) {

                if (properties[data[1]] == data[2]) {
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