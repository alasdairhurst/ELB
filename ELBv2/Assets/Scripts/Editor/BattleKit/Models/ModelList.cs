using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using Engine.Data;

public sealed class ModelList : EditorWindow {
	[MenuItem("BattleKit/Models")]
	public static void ShowWindow() {
		GetWindow<ModelList>("Models");
	}

	private static ILookup<Type, Type> _models;
	private int _drawIndex;
	private Vector2 _scrollPos;
	private readonly Dictionary<string, bool> _foldoutExpanded = new Dictionary<string, bool>();
	private int _selectedIndex;
	private readonly Dictionary<int, int> _idIndexes = new Dictionary<int, int>();
	private const int INDENT_WIDTH = 14;
	private const int ARROW_WIDTH = 14;
	private int _dragUpdatedOverId;
	private double _foldoutDestTime;
	private int _currentIndent;

	public ModelList() {
		if (_models == null)
			_models = GameState.GetModelTypes().ToLookup(
				model => model.BaseType,
				model => model);
	}

	private void DrawChildren(Type t) {
		foreach (var child in _models[t])
			if (_models[child].Any()) {
				if (FoldoutItem(child.Name)) {
					Debug.Log("selected " + child.Name);
				}
				if (_foldoutExpanded[child.Name]) {
					StartIndent();
					DrawChildren(child);
					EndIndent();
				}
			} else if (ListItem(child.Name)) {
				Debug.Log("selected " + child.Name);
			}
	}

	private void OnGUI() {
		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

		_drawIndex = 0;
		_idIndexes.Clear();

		DrawChildren(typeof(Model));
		EditorGUILayout.EndScrollView();
	}
	private void StartIndent() {
		_currentIndent++;
	}

	private void EndIndent() {
		_currentIndent--;
	}

	private int NextId() {
		_selectedIndex++;
		return _idIndexes[_selectedIndex];
	}

	private bool HasNext() {
		return _idIndexes.ContainsKey(_selectedIndex + 1);
	}

	private bool HasPrevious() {
		return _idIndexes.ContainsKey(_selectedIndex - 1);
	}

	private int PreviousId() {
		_selectedIndex--;
		return _idIndexes[_selectedIndex];
	}

	private int GetPreviousId() {
		return _idIndexes[_selectedIndex - 1];
	}

	private int GetNextId() {
		return _idIndexes[_selectedIndex + 1];
	}

	private bool ListItem(string text) {
		var controlID = GUIUtility.GetControlID(FocusType.Keyboard);
		var style = this == focusedWindow ? StyleStore.LabelFocus : StyleStore.LabelNoFocus;
		var itemPosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 17f, 17f, style);
		var e = Event.current;
		switch (e.type) {
			case EventType.Repaint:
				_idIndexes[_drawIndex++] = controlID;
				style.Draw(itemPosition, GUIContent.none, false, GUIUtility.hotControl == controlID, false,
				GUIUtility.keyboardControl == controlID);
				var textPosition = itemPosition;
				textPosition.x = INDENT_WIDTH * _currentIndent;
				style.Draw(textPosition, new GUIContent(text), false, GUIUtility.hotControl == controlID, false,
				GUIUtility.keyboardControl == controlID);
				return false;
			case EventType.MouseDown:
				if ((e.button == 0) && itemPosition.Contains(e.mousePosition)) {
					GUIUtility.hotControl = controlID;
					GUIUtility.keyboardControl = controlID;
					Event.current.Use();
					_selectedIndex = _idIndexes.FirstOrDefault(x => x.Value == controlID).Key;
					return true;
				}
				return false;
			case EventType.MouseUp:
				if ((e.button == 0) && (GUIUtility.hotControl == controlID)) {
					GUIUtility.hotControl = 0;
					Event.current.Use();
				}
				return false;
			case EventType.KeyDown:
				if ((e.keyCode == KeyCode.UpArrow) && HasPrevious() && (controlID == GetPreviousId())) {
					GUIUtility.keyboardControl = PreviousId();
					Event.current.Use();
					return true;
				}
				if ((e.keyCode == KeyCode.UpArrow) && HasNext() && (controlID == GetNextId())) {
					GUIUtility.keyboardControl = NextId();
					Event.current.Use();
					return true;
				}
				return false;
		}
		return false;
	}

	private bool FoldoutItem(string text) {
		if (!_foldoutExpanded.ContainsKey(text))
			_foldoutExpanded[text] = true;
		var foldout = _foldoutExpanded[text];
		var itemPosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 17f, 17f, StyleStore.LabelFocus);
		var content = new GUIContent(text);
		var style = StyleStore.Foldout;
		var controlID = GUIUtility.GetControlID(FocusType.Keyboard, itemPosition);
		var eventType = Event.current.type;
		if (!GUI.enabled && ((Event.current.rawType == EventType.MouseDown) || (Event.current.rawType == EventType.MouseDrag) || (Event.current.rawType == EventType.MouseUp)))
			eventType = Event.current.rawType;
		var eventType2 = eventType;
		switch (eventType2) {
			case EventType.MouseDown:
				if (itemPosition.Contains(Event.current.mousePosition) && (Event.current.button == 0)) {
					GUIUtility.hotControl = controlID;
					GUIUtility.keyboardControl = controlID;
					_selectedIndex = _idIndexes.FirstOrDefault(x => x.Value == controlID).Key;
					Event.current.Use();
					return true;
				}
				return false;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID) {
					GUIUtility.hotControl = 0;
					Event.current.Use();
					var rect2 = itemPosition;
					rect2.width = style.padding.left;
					rect2.x += INDENT_WIDTH * _currentIndent;
					if (rect2.Contains(Event.current.mousePosition)) {
						GUI.changed = true;
						_foldoutExpanded[text] = !foldout;
					}
				}
				return false;
			case EventType.MouseMove:
			case EventType.KeyUp:
			case EventType.ScrollWheel:
			case EventType.Layout:
				return false;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID)
					Event.current.Use();
				return false;
			case EventType.KeyDown:
				if (GUIUtility.keyboardControl == controlID) {
					var keyCode = Event.current.keyCode;
					if (((keyCode == KeyCode.LeftArrow) && foldout) || ((keyCode == KeyCode.RightArrow) && !foldout)) {
						GUI.changed = true;
						Event.current.Use();
						_foldoutExpanded[text] = !foldout;
					} else if ((keyCode == KeyCode.UpArrow) && HasPrevious() && (controlID == GetPreviousId())) {
						GUIUtility.keyboardControl = PreviousId();
						GUI.changed = true;
						Event.current.Use();
						return true;
					} else if ((keyCode == KeyCode.DownArrow) && HasNext() && (controlID == GetNextId())) {
						GUIUtility.keyboardControl = NextId();
						GUI.changed = true;
						Event.current.Use();
						return true;
					}
				}
				return false;
			case EventType.Repaint: {
				var bgStyle = this == focusedWindow ? StyleStore.LabelFocus : StyleStore.LabelNoFocus;
				bgStyle.Draw(itemPosition, GUIContent.none, false, controlID == GUIUtility.hotControl, foldout, controlID == GUIUtility.keyboardControl);
				var position2 = new Rect(INDENT_WIDTH * _currentIndent, itemPosition.y, EditorGUIUtility.labelWidth - ARROW_WIDTH, itemPosition.height);
				style.Draw(position2, content, false, controlID == GUIUtility.hotControl, foldout, controlID == GUIUtility.keyboardControl);
				return false;
			}
			case EventType.DragUpdated:
				if (_dragUpdatedOverId == controlID) {
					if (itemPosition.Contains(Event.current.mousePosition)) {
						if (Time.realtimeSinceStartup > _foldoutDestTime) {
							_foldoutExpanded[text] = true;
							Event.current.Use();
						}
					} else {
						_dragUpdatedOverId = 0;
					}
				} else if (itemPosition.Contains(Event.current.mousePosition)) {
					_dragUpdatedOverId = controlID;
					_foldoutDestTime = Time.realtimeSinceStartup + 0.7;
					Event.current.Use();
				}
				return false;
		}
		return false;
	}
}