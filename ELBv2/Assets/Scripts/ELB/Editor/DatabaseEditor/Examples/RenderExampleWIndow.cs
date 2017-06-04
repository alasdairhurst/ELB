using UnityEngine;
using UnityEditor;
public class MeshPreviewTest : EditorWindow {

	Mesh renderedMesh;
	Material renderedMaterial;

	Vector2 scrollPos;
	Vector2 previewDimensions = new Vector2(256, 256);

	private GameObject cam = null;
	private Camera Camera {
		get {
			if (cam == null) {
				cam = new GameObject();
				cam.hideFlags = HideFlags.HideAndDontSave;
				cam.transform.position = new Vector3(0.5f, 2.0f, -5.0f);
				cam.transform.eulerAngles = new Vector3(19.0f, -5.0f, 0.0f);
				cam.AddComponent<Camera>();
				cam.GetComponent<Camera>().fieldOfView = 19;
			}
			return cam.GetComponent<Camera>();
		}
	}

	private Mesh bgMesh = null;
	private Mesh BackgroundMesh {
		get {
			if (bgMesh == null) {
				bgMesh = new Mesh();
				bgMesh.vertices = new Vector3[28] {
                     // Front-facing:
                     Vector3.zero, Vector3.up, new Vector3(1, 1, 0), Vector3.right,
					 Vector3.forward, Vector3.up, Vector3.zero, new Vector3(0, -1, 1),
					 Vector3.zero, Vector3.right, new Vector3(1, -1, 1), new Vector3(0, -1, 1),
                     // In the middle:
                     Vector3.zero, Vector3.forward, new Vector3(1, 0, 1), Vector3.right,
                     // Rear-facing:
                     Vector3.right, new Vector3(1, -1, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 0),
					 Vector3.up, new Vector3(1, 1, 0), new Vector3(1, 0, 1), Vector3.forward,
					 Vector3.forward, new Vector3(1, 0, 1), new Vector3(1, -1, 1), new Vector3(0, -1, 1)
				 };
				bgMesh.SetIndices(new int[28] {
					 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27
				 }, MeshTopology.Quads, 0);
				bgMesh.RecalculateNormals();
			}
			return bgMesh;
		}
	}

	private Material bgmat = null;
	private Material BackgroundMaterial {
		get {
			if (bgmat == null) {
				bgmat = new Material(Shader.Find("Transparent/VertexLit"));
				bgmat.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
			}
			return bgmat;
		}
	}

	[MenuItem("BattleKit/Examples/ExampleRender")]
	public static void ShowWindow() {
		// Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow<MeshPreviewTest>();
	}

	void OnGUI() {
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.BeginVertical();
		renderedMesh = EditorGUILayout.ObjectField("Mesh:", renderedMesh, typeof(Mesh), false) as Mesh;
		renderedMaterial = EditorGUILayout.ObjectField("Material:", renderedMaterial, typeof(Material), false) as Material;

		Quaternion previewRotation = Quaternion.identity;
		previewRotation.eulerAngles = new Vector3(0.0f, -45.0f, 0.0f);
		Graphics.DrawMesh(BackgroundMesh, Vector3.zero, previewRotation, BackgroundMaterial, 0, Camera);
		if (renderedMesh != null && renderedMaterial != null) {
			Graphics.DrawMesh(renderedMesh, Vector3.zero, previewRotation, renderedMaterial, 0, Camera);
		}
		// Draw the camera to the window.
		GUI.BeginGroup(GUILayoutUtility.GetRect(previewDimensions.x, previewDimensions.y));
		Handles.BeginGUI();
		Handles.DrawCamera(new Rect(0, 0, previewDimensions.x, previewDimensions.y), Camera, DrawCameraMode.TexturedWire);
		Handles.EndGUI();
		GUI.EndGroup();

		GUILayout.EndVertical();
		GUILayout.EndScrollView();
	}
}