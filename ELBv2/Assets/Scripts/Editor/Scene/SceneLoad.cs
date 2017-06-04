using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class LatestScenes {
	private static Scene currentScene;
	static LatestScenes() {
		currentScene = SceneManager.GetActiveScene();
		EditorApplication.hierarchyWindowChanged += hierarchyWindowChanged;
	}
	private static void hierarchyWindowChanged() {
		if (currentScene != SceneManager.GetActiveScene()) {
			//a scene change has happened
			Debug.Log("Last Scene: " + currentScene.name);
			currentScene = SceneManager.GetActiveScene();
		}
	}
}
