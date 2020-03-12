using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public enum SpeechGeneratorVoice {
	Alex,
	Daniel,
	Fiona,
	Fred,
	Samantha,
	Victoria
}

public class SpeechGenerator : EditorWindow {

	string textString = "Hello!";
	static string outputFolder = "Speech";
	SpeechGeneratorVoice voice = SpeechGeneratorVoice.Samantha;

	[MenuItem("Util/Speech Gen")]
	static void Init() {
		SpeechGenerator speechGen = (SpeechGenerator)EditorWindow.GetWindow(typeof(SpeechGenerator));
		speechGen.Show();
	}

	void OnGUI() {
		GUILayout.Label("Jazz Mickle's Automatic Speech Generator");
		GUILayout.Space(10);
		//GUILayout.Label("Jazz Mickle's Automatic Speech Generator");
		outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
		textString = EditorGUILayout.TextField("Speech", textString);
		voice = (SpeechGeneratorVoice) EditorGUILayout.EnumPopup("Voice: ", voice);
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("►")) {
			SayText(textString, voice.ToString());
		}
		if (GUILayout.Button("Save")) {
			GenText(textString, voice.ToString());
		}
		EditorGUILayout.EndHorizontal();
	}

	static string ParseText(string text) {
		return text.Replace("'", "\\'");
	}

	static string ParseFilename(string text) {
		return text.Replace("'", "");
	}

	public static void SayText(string text, string voice) {
		if (!Directory.Exists("Assets/"+outputFolder)) {
			Directory.CreateDirectory("Assets/"+outputFolder);
		}

		var psi = new System.Diagnostics.ProcessStartInfo(); 
    	psi.WorkingDirectory = "Assets/"+outputFolder;
		psi.FileName = "/usr/bin/say";
		psi.UseShellExecute = true; 
		psi.Arguments = ParseText(text)+" -v "+voice;
		System.Diagnostics.Process.Start(psi); 
	}

	public static void GenText(string text, string voice) {
		var psi = new System.Diagnostics.ProcessStartInfo(); 
    	psi.WorkingDirectory = "Assets/"+outputFolder;
		psi.FileName = "/usr/bin/say";
		psi.UseShellExecute = true; 
		psi.Arguments = ParseText(text)+" -v "+voice+" -o \""+ParseFilename(text)+"\"";

		var p = System.Diagnostics.Process.Start(psi); 
		p.WaitForExit(); 

		if (File.Exists("/usr/local/bin/sox")) {
			psi.FileName = "/usr/local/bin/sox";
			psi.Arguments = "\""+ParseFilename(text)+".aiff\" \""+ParseFilename(text)+".mp3\"";
			p = System.Diagnostics.Process.Start(psi); 
			p.WaitForExit();
			File.Delete("Assets/"+outputFolder+"/"+ParseFilename(text)+".aiff");
		}

		AssetDatabase.Refresh();
	}

}
