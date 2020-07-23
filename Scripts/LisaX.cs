using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

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

    public bool virtualMethod;
    public string[] parameterNames;
}

public struct LisaXLine
{
    public string methodName;
    public LisaXMethodHook methodHook;
    public LisaXToken[] tokens;
    public string[] stringTokens;
    public string unparsedLine;

    public static LisaXLine NONE = new LisaXLine() {
        methodName = "",
        tokens = new LisaXToken[0],
        unparsedLine = "",
        stringTokens = new string[0]
    };

    public bool hasValueOf;
}

public struct LisaXToken
{
    public enum TokenType {
        Number,
        String,
        ValueOf
    }

    public TokenType type;
    public float numberValue;
    public string stringValue;
    public string valueName;
}

public class LisaX
{
    Dictionary<string, List<LisaXLine>> script;
    public Dictionary<string, string> properties;
    public Dictionary<string, LisaXMethodHook> methodHooks; 

    public bool running;

    Stack<(string label, int line)> callStack;
    (string label, int line) currentPosition;

    static string errorFilePath = "it broke.txt";

    public List<(string type, int id)> parameterTypes = new List<(string type, int id)>() {
        ("Method", 0),
        ("Label", 1),
        ("Variable", 2),
        ("Value", 3),
        ("String", 4),
        ("Float", 5),
        ("Comment", 6),
        ("LabelIdent", 7)
    };

    public LisaX()
    {
        methodHooks = new Dictionary<string, LisaXMethodHook>();
        callStack = new Stack<(string, int)>();

        // Built-in methods

        // Script Control

        AddMethodHook(
            "if",
            (data) => {
                callStack.Push(currentPosition);
                if (properties.ContainsKey(data[1])) {
                    if (properties[data[1]] == data[2]) {
                        RunLabel(data[3]);
                    } else {
                        var pos = callStack.Pop();
                        RunLabel(pos.label, pos.line);
                    }
                } else {
                    var pos = callStack.Pop();
                    RunLabel(pos.label, pos.line);
                }
            },
            false,
            "check stuff",
            LisaXParameter.Variable, LisaXParameter.Value, LisaXParameter.Label
        );

        AddMethodHook(
            "ifgreater",
            (data) => {
                callStack.Push(currentPosition);
                if (properties.ContainsKey(data[1])) {
                    if (float.Parse(properties[data[1]]) > float.Parse(data[2])) {
                        RunLabel(data[3]);
                    } else {
                        var pos = callStack.Pop();
                        RunLabel(pos.label, pos.line);
                    }
                } else {
                    var pos = callStack.Pop();
                    RunLabel(pos.label, pos.line);
                }
            },
            false,
            "check stuff",
            LisaXParameter.Variable, LisaXParameter.Value, LisaXParameter.Label
        );

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

        AddMethodHook(
            "set", 
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
            "length",
            (data) => {
                if (properties.ContainsKey(data[1])) {
                    properties[data[1]] = data[2].Length.ToString();
                } else {
                    properties.Add(data[1], data[2].Length.ToString());
                }
            },
            true,
            "Sets the length of arg 2 to arg 1",
            LisaXParameter.Variable, LisaXParameter.String
        );
        
        // AddMethodHook(
        //     "sub", 
        //     (data) => {
        //         if (properties.ContainsKey(data[1])) {
        //             properties[data[1]] = (float.Parse(properties[data[1]]) - float.Parse(data[2])).ToString();
        //         } else {
        //             properties.Add(data[1], (-float.Parse(data[2])).ToString());
        //         }
        //     },
        //     "Subtracts amount from value. If value doesn't exist, it is created and set to -[amount].",
        //     LisaXParameter.Variable, LisaXParameter.Float
        // );
        
        AddMethodHook(
            "mul", 
            (data) => {
                // Debug.Log($"{data[0]} {data[1]} {data[2]}");
                // if (!properties.ContainsKey(data[1])) {
                //     Debug.Log("Heck");
                // } else {
                //     Debug.Log($"prop {properties[data[1]]}");
                // }
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
        script = new Dictionary<string, List<LisaXLine>>();
        properties = new Dictionary<string, string>();
        string currentLabel = "START";
        script.Add(currentLabel, new List<LisaXLine>());

        for (int i = 0; i < data.Length; i++) {
            if (data[i].StartsWith("#")) {
                continue;
            } else if (data[i].StartsWith(":")) {

                string[] tokens = data[i].Split(' ');
                if (tokens.Length == 2) {
                    if (script.ContainsKey(tokens[1])) {
                        // Debug.LogWarning($"Parse error at line {i}: label {tokens[1]} already exists.  {data[i]}");
                        currentLabel = tokens[1];
                    } else {
                        currentLabel = tokens[1];
                        script.Add(tokens[1], new List<LisaXLine>());
                    }
                }

            } else if (data[i].StartsWith("!")) {

                string[] tokens = data[i].Split(' ');

                string[] paramNames = new string[tokens.Length-2];
                int[] paramTypes = new int[tokens.Length-2];
                for (int t = 2; t < tokens.Length; t++) {
                    string[] param = tokens[t].Substring(0, tokens[t].Length-1).Split('<');
                    string paramName = param[0];
                    string paramType = param[1];
                    paramNames[t-2] = paramName;
                    paramTypes[t-2] = FindParameterIndex(paramType);
                }

                var virtualMethod = new LisaXMethodHook() {
                    parameterNames = paramNames,
                    virtualMethod = true,
                    parameters = paramTypes,
                    name = tokens[1],
                    continues = false
                };

                methodHooks.Add(tokens[1], virtualMethod);

                currentLabel = tokens[1];
                script.Add(tokens[1], new List<LisaXLine>());

            } else {
                script[currentLabel].Add(ParseLine(data[i]));
            }
        }

        RunLabel("START");
    }

    public LisaXLine ParseLine(string line)
    {
        string[] tokens = ParseTokens(line.Trim(' '), true);

        string methodName = tokens[0];
        if (methodHooks.ContainsKey(methodName) || methodName == "if" || methodName == "ifgreater") {
            var methodHook = methodHooks[methodName];

            var ltokens = new LisaXToken[methodHook.parameters.Length + 1];
            ltokens[0] = new LisaXToken() { type = LisaXToken.TokenType.String, stringValue = methodName };
            bool hasValueOf = false;
            for (int i = 0; i < methodHook.parameters.Length; i++)
            {
                LisaXToken token;
                if (float.TryParse(tokens[i+1], out float result)) {
                    token = new LisaXToken() {
                        type = LisaXToken.TokenType.Number,
                        numberValue = result
                    };
                } else {
                    if (tokens[i+1].Contains("$") && tokens[i+1].Contains(";")) {
                        hasValueOf = true;
                        token = new LisaXToken() {
                            type = LisaXToken.TokenType.ValueOf,
                            valueName = tokens[i+1]
                        };
                    } else {
                        token = new LisaXToken() {
                            type = LisaXToken.TokenType.String,
                            stringValue = tokens[i+1]
                        };
                    }
                }

                ltokens[i + 1] = token;
            }

            LisaXLine output = new LisaXLine() {
                methodHook = methodHook,
                methodName = methodName,
                tokens = ltokens,
                stringTokens = tokens,
                hasValueOf = hasValueOf,
                unparsedLine = line
            };

            return output;
        }

        return new LisaXLine() {
            hasValueOf = line.Contains("$") && line.Contains(";"),
            methodName = methodName,
            stringTokens = tokens,
            unparsedLine = line
        };
        // return LisaXLine.NONE;
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
                        currentLabel = tokens[1];
                        // Debug.LogWarning($"Parse error at line {i}: label {tokens[1]} already exists.  {data[i]}");
                    } else {
                        currentLabel = tokens[1];
                        script.Add(tokens[1], new List<LisaXLine>());
                    }
                }
            } else if (data[i].StartsWith("!")) {

                string[] tokens = data[i].Split(' ');

                string[] paramNames = new string[tokens.Length-2];
                int[] paramTypes = new int[tokens.Length-2];
                for (int t = 2; t < tokens.Length; t++) {
                    string[] param = tokens[t].Substring(0, tokens[t].Length-1).Split('<');
                    string paramName = param[0];
                    string paramType = param[1];
                    paramNames[t-2] = paramName;
                    paramTypes[t-2] = FindParameterIndex(paramType);
                }

                var virtualMethod = new LisaXMethodHook() {
                    parameterNames = paramNames,
                    virtualMethod = true,
                    parameters = paramTypes,
                    name = tokens[1],
                    continues = false
                };

                methodHooks.Add(tokens[1], virtualMethod);
                // Debug.Log($"Including virtual method {tokens[1]}");
                currentLabel = tokens[1];
                script.Add(tokens[1], new List<LisaXLine>());

            } else {
                script[currentLabel].Add(ParseLine(data[i]));
            }
        }

        // Debug.Log($"{methodHooks["add2"].virtualMethod}");
    }

    int FindParameterIndex(string type)
    {
        foreach (var p in parameterTypes) {
            if (p.type == type) return p.id;
        }
        return -1;
    }

    public void RunLabel(string label, int start = 0) {
        if (!script.ContainsKey(label)) {
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

    public string[] ParseTokens(string line, bool skipProps = false)
    {
        List<StringBuilder> output = new List<StringBuilder>();
        bool inQuotes = false;

        line = line.Replace("|", "\n");


        StringBuilder currentToken = new StringBuilder("");
        for (int i = 0; i < line.Length; i++) {
            if ((!inQuotes) && line[i] == ' ') {
                output.Add(currentToken);
                currentToken = new StringBuilder("");
            } else if (line[i] == '"') {
                inQuotes = !inQuotes;
            } else {
                currentToken.Append(line[i]);
            }
        }
        output.Add(currentToken);

        string[] outputStrings = new string[output.Count];
        for (int i = 0; i < output.Count; i++) {
            outputStrings[i] = output[i].ToString();
        }

        if (properties != null && !skipProps) {
            bool replaced = true;
            foreach (var kvp in properties) {
                var keystring = "$"+kvp.Key+";";
                replaced = true;
                while (replaced) {
                    replaced = false;
                    for (int i = 0; i < outputStrings.Length; i++) {
                        var prev = outputStrings[i].GetHashCode();
                        outputStrings[i] = outputStrings[i].Replace(keystring, kvp.Value);
                        if (prev != outputStrings[i].GetHashCode()) replaced = true;
                    }
                }
            }
        }

        return outputStrings;
    }

    public string ParseToken(string token)
    {
        // new
        if (token.Contains("$") && token.Contains(";")) {
            int start = token.LastIndexOf('$');
            int end = token.IndexOf(';');
            string innerToken = token.Substring(
                start+1,
                end - start - 1
            );
            if (properties.ContainsKey(innerToken)) {
                return ParseToken(token.Replace("$"+innerToken+";", properties[innerToken]));
            }
        }
        return token;
    }

    public bool RunLine(LisaXLine line)
    {
        if (line.unparsedLine.StartsWith("#")) return true;
        if (line.unparsedLine.Length == 0) return true;
        if (line.methodName == "") return true;

        if (line.methodName != line.methodHook.name) {
            // Debug.Log("fruck");
            line.methodHook = methodHooks[line.methodName];
    
        }

        string[] tokens = new List<string>(line.stringTokens).ToArray();

        if (line.hasValueOf) {
            // tokens = ParseTokens(line.unparsedLine);
            for(int i = 0; i < tokens.Length; i++) {
                tokens[i] = ParseToken(tokens[i]);
            }
        }

        if (line.methodHook.virtualMethod) {
            // Debug.Log("Calling virtual method");
            for (int i = 0; i < line.methodHook.parameterNames.Length; i++)
            {
                // Debug.Log($"Setting Param: {line.methodHook.parameterNames[i]} to {line.stringTokens[i+1]}");
                if (properties.ContainsKey(line.methodHook.parameterNames[i])) {
                    properties[line.methodHook.parameterNames[i]] = tokens[i+1];
                } else {
                    properties.Add(line.methodHook.parameterNames[i], tokens[i+1]);
                }
            }
            callStack.Push(currentPosition);
            RunLabel(line.methodHook.name);
            return false;
        } else {
            line.methodHook.method.Invoke(tokens);
        }
        
        return line.methodHook.continues;
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