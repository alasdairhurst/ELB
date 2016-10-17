using ELB.Data.Models;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LatestScenes {
	private static string currentScene;
	static LatestScenes() {
		currentScene = EditorApplication.currentScene;
		EditorApplication.hierarchyWindowChanged += hierarchyWindowChanged;
	}
	private static void hierarchyWindowChanged() {
		if (currentScene != EditorApplication.currentScene) {
			//a scene change has happened
			Debug.Log("Last Scene: " + currentScene);
			currentScene = EditorApplication.currentScene;
		}
	}
}