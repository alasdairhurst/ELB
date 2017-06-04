using Engine.Data;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BattleKit.Editor {

	public class ModelWizard : EditorWindow {

		private Model _selection;
		private PropertyInfo[] _props;
		private bool _editMode;
		private Vector2 _scrollPos;

		private static void ShowWizard<T>(T instance, bool editMode) where T : Model {
			var title = editMode ? "Edit " : "New ";
			title += instance.GetType().Name;
			var window = CreateInstance<ModelWizard>();
			window.titleContent = new GUIContent(title);
			window.SetData(instance, editMode);
			window.Show();
		}

		private void SetData(Model selection, bool editMode) {
			_selection = selection;
			_editMode = editMode;
			_props = _selection.GetType().GetProperties().OrderBy(prop => prop.Name).ToArray();
		}

		public static void ShowWizard<T>(T instance) where T : Model {
			ShowWizard(instance, true);
		}

		public static void ShowWizard(Type t) {
			ShowWizard(Activator.CreateInstance(t) as Model, false);
		}

		void OnGUI( ) {
			if(_selection == null) {
				return;
			}
			var enableSave = true;
			_scrollPos = GUILayout.BeginScrollView(_scrollPos);
			{
				foreach(var propertyInfo in _props) {
					if(propertyInfo.PropertyType.IsSubclassOf(typeof(Model))) {
						var currentVal = propertyInfo.GetValue(_selection, null) as Model;
						var val = CustomFields.ModelField(new GUIContent(propertyInfo.Name), currentVal, propertyInfo.PropertyType);
						if(currentVal != val) {
							propertyInfo.SetValue(_selection, val, null);
						}
					} else {
						switch(propertyInfo.PropertyType.Name) {
							case "String": {
									GUI.enabled = propertyInfo.Name != "_Id";
									// input default values
									var val = EditorGUILayout.TextField(new GUIContent(propertyInfo.Name), (string)propertyInfo.GetValue(_selection, null));
									GUI.enabled = true;

									propertyInfo.SetValue(_selection, val, null);

									// check for required and duplicates
									if(val == null || val.Trim() == "" && propertyInfo.Name == "_EditorId") {
										enableSave = false;
									}
									break;
								}
							case "Int32": {
									EditorGUILayout.IntField(new GUIContent(propertyInfo.Name), (int)propertyInfo.GetValue(_selection, null));
									break;
								}
							case "Single": {
									EditorGUILayout.FloatField(new GUIContent(propertyInfo.Name), (float)propertyInfo.GetValue(_selection, null));
									break;
								}
							default: {
									GUI.enabled = false;
									EditorGUILayout.TextField(new GUIContent(propertyInfo.Name), "No editor for type: " + propertyInfo.PropertyType.Name);
									GUI.enabled = true;
									break;
								}
						}
					}
				}


				GUILayout.BeginHorizontal();
				{
					GUI.enabled = enableSave;
					if(GUILayout.Button("Save")) {
						OnSave();
					}
					GUI.enabled = true;
					if(GUILayout.Button("Cancel")) {
						Close();
					}
				}

				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}

		void OnDestroy( ) {
			_selection = null;
		}

		void OnSave( ) {
			Close();
		}
	}
}
