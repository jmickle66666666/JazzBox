using System.Collections.Generic;
using UnityEngine;
using System.IO;

public struct LisaXMethodHook
{
    public string name;
    public string description;

    public System.Action<string[]> method;
    public bool continues;
}

public class LisaX
{
    Dictionary<string, List<string>> script;
    public Dictionary<string, string> properties;
    public Dictionary<string, LisaXMethodHook> methodHooks; 

    public bool running;

    Stack<(string label, int line)> callStack;
    (string label, int line) currentPosition;

    public LisaX()
    {
        methodHooks = new Dictionary<string, LisaXMethodHook>();
        callStack = new Stack<(string, int)>();

        // Built-in methods
        AddMethodHook("end", () => {}, false, "Stops execution! Scripts will already stop when they run out of commands but this can be used to add clarity or add a breakpoint.");
        AddMethodHook("inc", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (float.Parse(properties[data[1]]) + 1).ToString();
            } else {
                properties.Add(data[1], "1");
            }
        }, "Usage: `inc [value]`\nIncreases a value by one. Useful for counters or loops. Equivalent to `add val 1`. If value doesn't exist, it is created and set to 1.");
        AddMethodHook("dec", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (float.Parse(properties[data[1]]) - 1).ToString();
            } else {
                properties.Add(data[1], "-1");
            }
        }, "Usage: `dec [value]`\nDecreases a value by one. Useful for counters or loops. Equivalent to `sub val 1`. If value doesn't exist, it is created and set to -1.");
        AddMethodHook("add", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (float.Parse(properties[data[1]]) + float.Parse(data[2])).ToString();
            } else {
                properties.Add(data[1], float.Parse(data[2]).ToString());
            }
        }, "Usage: `add [value] [amount]`\nAdds amount to value. If value doesn't exist, it is created and set to [amount].");
        AddMethodHook("sub", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = (float.Parse(properties[data[1]]) - float.Parse(data[2])).ToString();
            } else {
                properties.Add(data[1], (-float.Parse(data[2])).ToString());
            }
        }, "Usage: `sub [value] [amount]`\nSubtracts amount from value. If value doesn't exist, it is created and set to -[amount].");
        AddMethodHook("mul", (data) => {
            properties[data[1]] = (float.Parse(properties[data[1]]) * float.Parse(data[2])).ToString();
        }, "Usage: `mul [value] [amount]`");
        AddMethodHook("div", (data) => {
            properties[data[1]] = (float.Parse(properties[data[1]]) / float.Parse(data[2])).ToString();
        }, "Usage: `div [value] [amount]`");
        AddMethodHook("pow", (data) => {
            properties[data[1]] = Mathf.Pow(float.Parse(properties[data[1]]), float.Parse(data[2])).ToString();
        }, "Usage: `pow [value] [amount]`");
        AddMethodHook("goto", (data) => {
            callStack.Push(currentPosition);
            RunLabel(data[1]);
        }, false, "Usage: `goto [label]`\nStops current label and runs the given one. Stores the current position, which can be returned to using `return`");
        AddMethodHook("set", (data) => {
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = data[2];
            } else {
                properties.Add(data[1], data[2]);
            }
        }, "Usage: `set [value] [amount]`\nSets the given value to [amount]. If it doesn't exist it is created first.`");
        AddMethodHook("return", (data) => {
            if (callStack.Count > 0) {
                var pos = callStack.Pop();
                RunLabel(pos.label, pos.line);
            }
        }, "Usage: `return`\nReturns to the last time execution was jumped, from an `if` or `goto`. Used for running some code, then continuing where you left off.");
        AddMethodHook("include", (data) => {
            Include(File.ReadAllLines(data[1]));
        }, "Usage: `include [path_to_script]`\nAdds all the labels from another script into this script, so they can be called. Code outside of a label is not run automatically. Make sure the labels in the included script don't already exist in the current one.");
        AddMethodHook("random", (data) => {
            float min = float.Parse(data[2]);
            float max = float.Parse(data[3]);
            var random = Random.Range(min, max);
            if (properties.ContainsKey(data[1])) {
                properties[data[1]] = random.ToString();
            } else {
                properties.Add(data[1], random.ToString());
            }
        }, "Usage: `random [value] [min] [max]`\nGenerates a random value between min and max, and stores it in [value]. Both min and max are inclusive, meaning they can be generated.");
    }

    public void AddMethodHook(string methodName, System.Action<string[]> method, string description)
    {
        AddMethodHook(methodName, method, true, description);
    }

    public void AddMethodHook(string methodName, System.Action<string[]> method, bool continues = true, string description = "")
    {
        methodHooks.Add(methodName, new LisaXMethodHook() {
            name = methodName,
            method = method,
            continues = continues,
            description = description
        });
    }

    public void AddMethodHook(string methodName, System.Action method, string description)
    {
        AddMethodHook(methodName, method, true, description);
    }

    public void AddMethodHook(string methodName, System.Action method, bool continues = true, string description = "")
    {
        methodHooks.Add(methodName, new LisaXMethodHook() {
            name = methodName,
            method = (data) => { method.Invoke(); },
            continues = continues,
            description = description
        });
    }

    public string GenerateDocs()
    {
        string output = "";
        output += "# LisaX Script Documentation\n";
        output += "## Auto-Generated\n";
        foreach (var m in methodHooks) {
            output += $"\n## {m.Value.name}\n";
            if (!m.Value.continues) {
                output += "Stops the script\n";
            }
            if (m.Value.description == "") {
                Debug.LogWarning($"No description for method {m.Value.name}");
                output += $"No description available\n";
            } else {
                output += $"Description: {m.Value.description}\n";
            }
        }
        return output;
    }

    public void LoadScript(string[] data)
    {
        script = new Dictionary<string, List<string>>();
        properties = new Dictionary<string, string>();
        string currentLabel = "START";
        script.Add(currentLabel, new List<string>());

        for (int i = 0; i < data.Length; i++) {
            if (data[i].StartsWith("#")) {
                continue;
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

    public void Include(string[] data)
    {
        string currentLabel = "START";
        for (int i = 0; i < data.Length; i++) {
            if (data[i].StartsWith("#")) {
                continue;
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

    public void RunLabel(string label, int start = 0) {
        if (!script.ContainsKey(label)) {
            // Debug.Log($"Can't find label {label}");
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

    public static float GetFloatValue(string value)
    {
        return (float) float.Parse(value);
    }
}