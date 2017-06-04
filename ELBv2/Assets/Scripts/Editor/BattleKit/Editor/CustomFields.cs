using System;
using Engine.Data;
using Engine.String;
using UnityEngine;
using UnityEditor;
using BattleKit.Editor.Utils;

namespace BattleKit.Editor {

	public class CustomFields {

		//static int s_SearchFieldHash = "EditorSearchField".GetHashCode();

		public static string SearchField(string text) {
			Rect pos = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, 28f);
			pos.x = 5f;
			pos.width -= 10f;
			pos.y = 5f;
			return DoNotDoThisAtHomeKids.call(typeof(EditorGUI), "SearchField", pos, text) as string;
		}

		private static readonly int s_ModelFieldHash = "s_ModelFieldHash".GetHashCode();

		public static Model ModelField(GUIContent label, Model model, Type modelType, EditorWindow caller, params GUILayoutOption[] options) {
			Model selection = model;

			Rect position = EditorGUILayout.GetControlRect(true, 16f, options);

			int id = GUIUtility.GetControlID(s_ModelFieldHash, FocusType.Passive, position);

			EditorGUI.PrefixLabel(position, id, label);

			position.xMin = EditorGUIUtility.labelWidth + 4;

			Event current = Event.current;
			EventType eventType = current.type;
			if(!GUI.enabled && Event.current.rawType == EventType.MouseDown) {
				eventType = Event.current.rawType;
			}
			Vector2 iconSize = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(new Vector2(12f, 12f));
			EventType eventType2 = eventType;
			switch(eventType2) {
				case EventType.KeyDown:
					if(GUIUtility.keyboardControl == id) {
						if(current.keyCode == KeyCode.Backspace || current.keyCode == KeyCode.Delete) {
							model = null;
							GUI.changed = true;
							current.Use();
						}
						if(current.keyCode == KeyCode.Return) {
							if(EditorUtility.DisplayDialog("find model", "find model", "OK")) {
								GUIUtility.ExitGUI();
							}
							current.Use();
						}
					}
					goto IL_67D;
				case EventType.KeyUp:
				case EventType.ScrollWheel:
				case EventType.Layout:
				case EventType.Ignore:
				case EventType.Used:
				case EventType.ValidateCommand:
					goto IL_11F;
				case EventType.Repaint: {
						var name = string.Format("{0} ({1})",
							model == null ? "None" : model.ToString(StringOpts.Identifier),
							modelType.Name);
						GUIContent gUIContent = new GUIContent(name);
						EditorStyles.objectField.Draw(position, gUIContent, id, DragAndDrop.activeControlID == id);
						goto IL_67D;
					}
				case EventType.ExecuteCommand: {
						if(current.commandName == "ModelPickerUpdated") {
							GUI.changed = true;
							current.Use();
							selection = ModelPicker.instance.GetSelection();
						} else if (current.commandName == "ModelPickerClosed") {
							caller.Focus();
						}
						break;
					}
				case EventType.DragExited:
					if(GUI.enabled) {
						HandleUtility.Repaint();
					}
					goto IL_67D;
			}
			IL_11F:
			if(eventType2 != EventType.MouseDown) {
				goto IL_67D;
			}
			if(Event.current.button != 0) {
				goto IL_67D;
			}
			if(position.Contains(Event.current.mousePosition)) {
				Rect rect = new Rect(position.xMax - 15f, position.y, 15f, position.height);
				EditorGUIUtility.editingTextField = false;
				if(GUI.enabled) {
					GUIUtility.keyboardControl = id;
					if(rect.Contains(Event.current.mousePosition)) {
						if(GUI.enabled) {
							if(model != null) {
								ModelPicker.ShowWizard(model, caller);
							} else {
								ModelPicker.ShowWizard(modelType, caller);
							}
						}
					}
					current.Use();
				}

			}
			IL_67D:
			EditorGUIUtility.SetIconSize(iconSize);
			return selection;
		}
	}
}
