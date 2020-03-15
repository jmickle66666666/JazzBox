using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;

public class FontGrab : EditorWindow
{
    public static string apiKey;

    // Path data
    static string fontOutputPath = "Assets/Fonts/";
    static string databasePath = "Assets/Fonts/font_database.json";
    static string tempPath = "Assets/Fonts/tempFont.ttf";

    // Download data
    static UnityWebRequest webRequest;
    static byte[] fontData;
    static GoogleFontResponse fontDatabase;

    // Temporary font info
    string outputPath;
    string fontName;
    GUIStyle previewStyle;
    Font currentFont;

    [MenuItem ("Tools/Font Grab")]
    static void OpenWindow() {

        FontGrab window = EditorWindow.GetWindow<FontGrab>();

        // Clear all data when opening the window to make sure we're at a clear, knowable state
        webRequest = null;
        fontData = null;
        fontDatabase = null;

        // You must provide you're own API key! They're rate limited but easy to pick up
        if (File.Exists("font_apikey.txt")) {
            apiKey = File.ReadAllText("font_apikey.txt");
        }

        // If the font database was already loaded then load the cache, cuts down on API calls
        if (File.Exists(databasePath)) {
            var text = File.ReadAllText(databasePath);
            fontDatabase = JsonUtility.FromJson<GoogleFontResponse>(text);
        }

        window.Show();
    }

    void OnGUI()
    {
        if (!File.Exists("font_apikey.txt")) {
            // TODO: provide a url so people can easily get an API key
            // TODO: provide an easy way to save the API key
            GUILayout.Label("No API key found! place a file font_apikey.txt in the root of your project with the api key inside!");
            return;
        }
    
        if (fontDatabase == null) {

            // State 1 : Download the database

            if (webRequest == null) {
                WebRequestButton();
            } else {
                if (fontDatabase == null) {
                    GUILayout.Label("Building font library...");

                    EditorGUI.ProgressBar(new Rect(0, this.position.height - EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight), webRequest.downloadProgress, "Progress");

                    if (webRequest.isDone) {
                        string database = webRequest.downloadHandler.text;
                        File.WriteAllText(databasePath, database);
                        fontDatabase = JsonUtility.FromJson<GoogleFontResponse>(database);
                        webRequest.Dispose();
                        webRequest = null;
                        fontData = null;
                    }
                }
            }

        } else if (fontData == null) {

            // State 2 : no current font

            if (webRequest == null) {
                if (GUILayout.Button("Grab a random font!")) {
                    GetRandomFont();
                }

                if (GUILayout.Button("Rebuild Database")) {
                    webRequest = UnityWebRequest.Get("https://www.googleapis.com/webfonts/v1/webfonts?key=" + apiKey);
                    webRequest.SendWebRequest();  
                    fontDatabase = null;
                }
            } else {
                if (!webRequest.isDone) {
                    GUILayout.Label("Downloading!");
                    GUILayout.Label($"Response code: {webRequest.responseCode}");
                    EditorGUI.ProgressBar(new Rect(0, this.position.height - EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight), webRequest.downloadProgress, "Progress");
                } else {
                    fontData = webRequest.downloadHandler.data;
                    
                    File.WriteAllBytes(tempPath, fontData);
                    AssetDatabase.Refresh();

                    currentFont = AssetDatabase.LoadAssetAtPath<Font>(tempPath);
                    previewStyle = new GUIStyle(GUI.skin.label);
                    previewStyle.fontSize *= 2;

                    previewStyle.font = currentFont;
                }
            }

        } else {

            // State 3 : current font, reroll, save
            
            GUILayout.Label("How's this cool font?", previewStyle);        

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save it!")) {
                File.WriteAllBytes(fontOutputPath + outputPath, fontData);
            }

            if (GUILayout.Button("Reroll")) {
                GetRandomFont();
            }

            GUILayout.EndHorizontal();
        }
    }

    void OnDestroy()
    {
        // We store a temporary asset for the preview font so delete it if its still around
        if (File.Exists(tempPath)) {
            File.Delete(tempPath);
            File.Delete(tempPath+".meta");
            AssetDatabase.Refresh();
        }
    }

    void GetRandomFont()
    {
        fontData = null;

        if (fontDatabase == null) Debug.LogError("What");
        if (fontDatabase.items == null) Debug.LogError("What???");

        int fontIndex = Random.Range(0, fontDatabase.items.Length);
        var fontInfo = fontDatabase.items[fontIndex];
        string fontURL = fontInfo.files.regular;
        webRequest = UnityWebRequest.Get(fontURL);

        int fileIndex = fontURL.LastIndexOf('.');
        outputPath = fontInfo.family + fontURL.Substring(fileIndex);
        fontName = fontInfo.family;

        webRequest.SendWebRequest();
    }

    void WebRequestButton()
    {
        if (GUILayout.Button("Build font database")) {
            webRequest = UnityWebRequest.Get("https://www.googleapis.com/webfonts/v1/webfonts?key=" + apiKey);
            webRequest.SendWebRequest();  
            fontDatabase = null;
        } 
    }

}

[System.Serializable]
public class GoogleFontResponse
{
    public string kind;
    public GoogleFontItem[] items;
}

[System.Serializable]
public class GoogleFontItem
{
    public string kind;
    public string family;
    public string[] variants;
    public string[] subsets;
    public string version;
    public string lastModified;
    public GoogleFontFile files;
}

[System.Serializable]
public class GoogleFontFile
{
    public string regular;
    public string italic;
}
