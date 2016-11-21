using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class TableExampleWindow : EditorWindow {
	// state
	int state_defaultWidth = 150;
	Vector2 state_scrollPos = new Vector2();
	int state_selectedItem = 0;

	int items = 10;

	bool hadLastFocus = false;

	List<Rect> itemRects = new List<Rect>();
	bool stateChanged = false;

	[MenuItem("BattleKit/Examples/TableWindow")]
	public static void ShowWindow() {
		GetWindow<TableExampleWindow>();
	}

	void OnInspectorUpdate() {
		//Debug.Log("paint");
		//Repaint();
	}

	void Update() {

		bool onUnfocus = hadLastFocus && focusedWindow != this;

		if (stateChanged || onUnfocus) {
			Repaint();
		}

		if (focusedWindow == this) {
			hadLastFocus = true;
		}
		if (onUnfocus) {
			hadLastFocus = false;
		}
	}

	void OnGUI() {
		var scrollPos = state_scrollPos;
		var defaultWidth = state_defaultWidth;
		var selectedItem = state_selectedItem;

		stateChanged = false;

		// test up and down arrows for selection
		if (keyDown(KeyCode.DownArrow) && state_selectedItem < items - 1) {
			state_selectedItem++;
			stateChanged = true;
		} else if (keyDown(KeyCode.UpArrow) && state_selectedItem != 0) {
			state_selectedItem--;
			stateChanged = true;
		}

		state_scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		
		for (int i = 0; i < items; i++) {
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < items; j++) {
				

				if (ClickableLabel("model_" + i, i == selectedItem, i)) {
					state_selectedItem = i;
					stateChanged = true;
				}

			}

			EditorGUILayout.EndHorizontal();
		}
		
		EditorGUILayout.EndScrollView();
	}

	bool keyUp(KeyCode k) {
		return Event.current.isKey && Event.current.type == EventType.KeyUp && Event.current.keyCode == k;
	}

	bool keyDown(KeyCode k) {
		return Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == k;
	}

	bool mouseDown(int b) {
		return Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == b;
	}

	bool mouseUp(int b) {
		return Event.current.isMouse && Event.current.type == EventType.MouseUp && Event.current.button == b;
	}

	public bool ClickableLabel(string text, bool selected, int index) {
		var backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = selected ? focusedWindow == this ? (Color)(new Color32(62, 125, 231, 255)) : (Color)(new Color32(143, 143, 143, 255)) : Color.clear;

		var style = selected ? StyleStore.TextureStyleWhiteText : StyleStore.TextureStyle;

		GUIContent selectableLabel = new GUIContent(text);
		GUILayout.Label(selectableLabel, style, GUILayout.Height(16));
		bool clicked = false;
		if (Event.current.type == EventType.Repaint) {
			if (index == 0) {
				itemRects = new List<Rect>();
			}
			itemRects.Add(GUILayoutUtility.GetLastRect());
		} else if (mouseDown(0) && itemRects[index].Contains(Event.current.mousePosition)) {
			clicked = true;
		}

		GUI.backgroundColor = backgroundColor;
		return clicked;
	}
}