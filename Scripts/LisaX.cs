using System.Collections.Generic;
using UnityEngine;
using System.IO;

public struct LisaXParameter
{
    public const int Method = 0;
    public const int Label = 1;
    public const int Variable = 2;
    public const int Value = 3;
    public const int String = 4;
    public const int Float = 5;
    public const int Comment = 6;
    public const int LabelIdent = 7;
}

public struct LisaXMethodHook
{
    public string name;
    public string description;

    public System.Action<string[]> method;
    public bool continues;

    public int[] parameters;
}

public class LisaX
{
    Dictionary<string, List<string>> script;
    public Dictionary<string, string> properties;
    public Dictionary<string, LisaXMethodHook> methodHooks; 

    public bool running;

    Stack<(string label, int line)> callStack;
    (string label, int line) currentPosition;

    static string errorFilePath = "it broke.txt";

    public LisaX()
    {
        methodHooks = new Dictionary<string, LisaXMethodHook>();
        callStack = new Stack<(string, int)>();

        // Built-in methods

        // Script Control

        AddMethodHook(
            "end", 
            () => {}, 
            false, 
            "Stops execution! Scripts will already stop when they run out of commands but this can be used to add clarity or add a breakpoint."
        );
        
        AddMethodHook(
            "goto", 
            (data) => {
                callStack.Push(currentPosition);
                RunLabel(data[1]);
            }, 
            false, 
            "Stops current label and runs the given one. Stores the current position, which can be returned to using `return`",
            LisaXParameter.Label
        );

        AddMethodHook(
            "return", 
            (data) => {
                if (callStack.Count > 0) {
                    var pos = callStack.Pop();
                    RunLabel(pos.label, pos.line);
                }
            }, 
            "Returns to the last time execution was jumped, from an `if` or `goto`. Used for running some code, then continuing where you left off."
        );
        
        // Mathematics

        AddMethodHook("set", 
            (data) => {
                if (properties.ContainsKey(data[1])) {
                    properties[data[1]] = data[2];
                } else {
                    properties.Add(data[1], data[2]);
                }
            }, 
            true, 
            "Usage: `set [value] [amount]`\nSets the given value to [amount]. If it doesn't exist it is created first.`",
            LisaXParameter.Variable, LisaXParameter.Value
        );
        
        AddMethodHook(
            "add", 
            (data) => {
                if (properties.ContainsKey(data[1])) {
                    properties[data[1]] = (float.Parse(properties[data[1]]) + float.Parse(data[2])).ToString();
                } else {
                    properties.Add(data[1], float.Parse(data[2]).ToString());
                }
            }, 
            true,
            "Adds amount to value. If value doesn't exist, it is created and set to [amount].",
            LisaXParameter.Variable, LisaXParameter.Float
        );
        
        AddMethodHook(
            "sub", 
            (data) => {
                if (properties.ContainsKey(data[1])) {
                    properties[data[1]] = (float.Parse(properties[data[1]]) - float.Parse(data[2])).ToString();
                } else {
                    properties.Add(data[1], (-float.Parse(data[2])).ToString());
                }
            },
            "Subtracts amount from value. If value doesn't exist, it is created and set to -[amount].",
            LisaXParameter.Variable, LisaXParameter.Float
        );
        
        AddMethodHook(
            "mul", 
            (data) => {
                properties[data[1]] = (float.Parse(properties[data[1]]) * float.Parse(data[2])).ToString();
            }, 
            "Multiplies a variable by a given amount",
            LisaXParameter.Variable, LisaXParameter.Float
        );
        
        AddMethodHook(
            "div", 
            (data) => {
                properties[data[1]] = (float.Parse(properties[data[1]]) / float.Parse(data[2])).ToString();
            }, 
            "Divide a varible by a given amount",
            LisaXParameter.Variable, LisaXParameter.Float
        );
        
        AddMethodHook(
            "pow", 
            (data) => {
                properties[data[1]] = Mathf.Pow(float.Parse(properties[data[1]]), float.Parse(data[2])).ToString();
            }, 
            "powerful",
            LisaXParameter.Variable, LisaXParameter.Float
        );
        
        AddMethodHook(
            "random", 
            (data) => {
                float min = float.Parse(data[2]);
                float max = float.Parse(data[3]);
                var random = Random.Range(min, max);
                if (properties.ContainsKey(data[1])) {
                    properties[data[1]] = random.ToString();
                } else {
                    properties.Add(data[1], random.ToString());
                }
            }, 
            "Generates a random value between two values, and stores it in a given value. Both min and max are inclusive.",
            LisaXParameter.Variable, LisaXParameter.Float, LisaXParameter.Float
        );

        AddMethodHook(
            "sine", 
            (data) => {
                float output = Mathf.Sin(float.Parse(data[2]) * Mathf.Deg2Rad );
                if (properties.ContainsKey(data[1])) {
                    properties[data[1]] = output.ToString();
                } else {
                    properties.Add(data[1], output.ToString());
                }
            }, 
            "Calculates sin for the given angle in degrees and stores it in `[value]`",
            LisaXParameter.Variable, LisaXParameter.Float
        );

        AddMethodHook(
            "floor", 
            (data) => {
                properties[data[1]] = Mathf.Floor(float.Parse(properties[data[1]])).ToString();
            },
            "Rounds off a value to the next lowest whole number. e.g 1.9 becomes 1.0",
            LisaXParameter.Variable
        );
    }


    public void AddMethodHook(string methodName, System.Action method, string description, params int[] parameters)
    {
        AddMethodHook(methodName, method, true, description, parameters);
    }
    public void AddMethodHook(string methodName, System.Action method, bool continues = true, string description = "", params int[] parameters)
    {
        methodHooks.Add(methodName, new LisaXMethodHook() {
            name = methodName,
            method = (data) => { method.Invoke(); },
            continues = continues,
            description = description,
            parameters = parameters
        });
    }
    public void AddMethodHook(string methodName, System.Action<string[]> method, string description, params int[] parameters)
    {
        AddMethodHook(methodName, method, true, description, parameters);
    }
    public void AddMethodHook(string methodName, System.Action<string[]> method, bool continues = true, string description = "", params int[] parameters)
    {
        methodHooks.Add(methodName, new LisaXMethodHook() {
            name = methodName,
            method = method,
            continues = continues,
            description = description,
            parameters = parameters
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

        if (properties != null) {
            for (int i = 0; i < output.Count; i++) {
                foreach (var kvp in properties) {
                    output[i] = output[i].Replace($"${kvp.Key};", kvp.Value);
                }
            }
        }

        return output.ToArray();
    }

    public bool RunLine(string line)
    {
        if (line.Length == 0) return true;
        string[] data = ParseTokens(line.Trim(' '));

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
        
        } else if (data[0] == "ifgreater") {

            if (properties.ContainsKey(data[1])) {

                if (float.Parse(properties[data[1]]) > float.Parse(data[2])) {
                    callStack.Push(currentPosition);
                    RunLabel(data[3]);
                    return false;
                }

            } else {
                return true;
            }
        
        } else if (methodHooks.ContainsKey(data[0])) {

            if (data.Length != methodHooks[data[0]].parameters.Length + 1) {
                WriteError(currentPosition.label, currentPosition.line, data, $"Incorrect number of arguments for method {data[0]}. Expected {methodHooks[data[0]].parameters.Length} things but got {data.Length - 1} !! :(");
                return false;
            }

            for (int i = 0; i < data.Length-1; i++) {
                string numbered = i.ToString() + "th";
                if (i == 1) numbered = "1st";
                if (i == 2) numbered = "2nd";
                if (i == 3) numbered = "3rd";
                switch (methodHooks[data[0]].parameters[i]) {
                    case LisaXParameter.Float:
                        if (!float.TryParse(data[i+1], out float _)) {
                            WriteError(currentPosition.label, currentPosition.line, data, $"Hey i was trying to run this line of code and kind of expected the {numbered} parameter to be a number but it wasn't! Can you double check that you wrote the right thing? Not sure how to deal with {data[i+1]}.");
                            return false;
                        }
                        break;
                    case LisaXParameter.Label:
                        if (!script.ContainsKey(data[1])) {
                            WriteError(currentPosition.label, currentPosition.line, data, $"I was looking at the {numbered} parameter, hoping it'd be a label, but it wasn't! At least, I can't find a label called {data[i+1]}.");
                            return false;
                        }
                        break;
                }
            }

            methodHooks[data[0]].method.Invoke(data);
            return methodHooks[data[0]].continues;

        } else {
            Debug.LogWarning($"Unknown command {line}");
            WriteError(currentPosition.label, currentPosition.line, data, $"Unknown command {line}");
        }

        return true;
    }

    public void WriteError(string label, int line, string[] tokens, string error)
    {
        string currError;
        if (File.Exists(errorFilePath))
        {
            currError = File.ReadAllText(errorFilePath);
        } else {
            currError = "";
        }
        currError += $"[{label} {line}]\n{string.Join(" ",tokens)}\n{error}\n";
        File.WriteAllText(errorFilePath, currError);
    }

    public static float GetFloatValue(string value)
    {
        return (float) float.Parse(value);
    }
}