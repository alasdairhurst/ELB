using BattleKit.Engine;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace BattleKit.Editor {
	[CustomEditor(typeof(Model), true)]
	class ModelInspector : UnityEditor.Editor {

		private bool _isRenaming;
		private const int RENAME_BUTTON_WIDTH = 50;
		private string _name = string.Empty;
		private string _renameError = string.Empty;
		private string _currentAssetPath = string.Empty;

		private void ShowRename() {
			_isRenaming = true;
		}

		private void HideRename() {
			// stop the text field retaining control/content
			GUIUtility.keyboardControl = 0;
			_renameError = string.Empty;
			_name = target.name;
			CreateInstance(target.GetType());
			_isRenaming = false;
		}

		private void Rename(string name) {
			_renameError = AssetDatabase.RenameAsset(_currentAssetPath, name);
			if (string.IsNullOrEmpty(_renameError)) {
				target.name = name;
				_currentAssetPath = AssetDatabase.GetAssetPath(target);
				(target as Model).InspectorOnChange.Invoke();
				HideRename();
			}
		}


		void OnEnable() {
			_currentAssetPath = AssetDatabase.GetAssetPath(target);
			_name = target.name;
			(target as Model).InspectorOnChange.AddListener(ModelBrowser.RepaintWindow);
		}

		void OnDestroy() {
			(target as Model).InspectorOnChange.RemoveListener(ModelBrowser.RepaintWindow);
		}

		private bool ValidateRename(string name) {
			return name != target.name;
		}

		private void RenderRenameAsset() {
			using (new EditorGUILayout.HorizontalScope()) {
				if (!_isRenaming) {
					if (GUILayout.Button("Rename")) {
						GUI.FocusControl(_name);
						ShowRename();
					}
				} else {
					_name = EditorGUILayout.TextField(_name);
					_name = _name.Trim();

					using (new EditorGUI.DisabledScope(!ValidateRename(_name))) {
						if (GUILayout.Button("Ok", GUILayout.Width(RENAME_BUTTON_WIDTH))) {
							Rename(_name);
						}
					}

					if (GUILayout.Button("Cancel", GUILayout.Width(RENAME_BUTTON_WIDTH))) {
						HideRename();
					}
				}
			}
			if (_isRenaming && !string.IsNullOrEmpty(_renameError)) {
				EditorGUILayout.HelpBox(_renameError, MessageType.Error);
			}
		}

		public override void OnInspectorGUI() {
			if (!string.IsNullOrEmpty(_currentAssetPath)) {
				RenderRenameAsset();
				EditorGUILayout.Separator();
			} else {
				var name = EditorGUILayout.TextField("Asset Name", target.name);
				if (name != target.name) {
					target.name = name;
					(target as Model).InspectorOnChange.Invoke();
				}
			}
			DrawDefaultInspector();
		}
	}
}
