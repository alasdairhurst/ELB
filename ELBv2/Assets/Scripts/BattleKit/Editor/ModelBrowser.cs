using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using BattleKit.Engine;
using System.IO;

namespace BattleKit.Editor {
	public sealed class ModelBrowser : EditorWindow {
		[MenuItem("BattleKit/Model Browser")]
		public static ModelBrowser ShowWindow() {
			return GetWindow<ModelBrowser>("Models");
		}
		private static ModelBrowser _instance;

		private static ILookup<Type, Type> _models;
		private static ItemList _itemList = new ItemList();
		private static Vector2 _scrollPos;
		private static Type _selectedType;
		private static Type _loadedInfoForType;
		private static List<SerializedObject> _collection;
		private static Dictionary<Type, TableHeader[]> _headers;
		private static IOrderedEnumerable<FieldInfo> _props;
		private static float _listWidth = 200; 
		private static bool _listHasFocus = true;
		

		private void LoadInfo() {
			if (_collection != null) {
				foreach (var item in _collection) {
					item.Dispose();
				}
				_collection.Clear();
			}
			_collection = Resources.LoadAll("", _selectedType).OrderBy(item => item.name).Select(item => new SerializedObject(item)).ToList();
			SerializedObject selection;
			if (_collection.Count > 0) {
				selection = _collection[0];
			}

			if (!_headers.ContainsKey(_selectedType)) {
				List<TableHeader> headers = new List<TableHeader> {
					new TableHeader { Label = "Asset Name", Width = 100 }
				};
				ScriptableObject objectForType = CreateInstance(_selectedType);
				selection = new SerializedObject(objectForType);
				DestroyImmediate(objectForType);
				var prop = selection.GetIterator();
				prop.NextVisible(true);
				while (prop.NextVisible(false)) {
					headers.Add(new TableHeader { Label = prop.displayName, Width = 100 });
				}

				_headers[_selectedType] = headers.ToArray();
			}
		}

		public static void ReloadWindow() {
			if (_instance != null) {
				_instance.LoadInfo();
				_instance.Repaint();
			}
		}

		public static void RepaintWindow() {
			if (_instance != null) {
				_instance.Repaint();
			}
		}

		void OnEnable() {
			_instance = this;
			if (_headers == null) {
				_headers = new Dictionary<Type, TableHeader[]>();
			}
			if (_models == null) {
				_models = typeof(Model).Assembly.GetTypes().Where(
					type => type.IsSubclassOf(typeof(Model))
				).ToLookup(
					model => model.BaseType, model => {
						if (_selectedType == null) {
							_selectedType = model;
						}
						return model;
					}
				);
			}
			LoadInfo();
		}

		private void DrawChildren(Type t) {
			foreach(var child in _models[t]) {
				if(_models[child].Any()) {
					ListItem(child, true);
					{
						DrawChildren(child);
					}
					_itemList.EndFoldoutGroup();
				} else {
					ListItem(child);
				}
			}
		}

		private void ListItem(Type t, bool isFoldout = false) {
			var title = t.Name;
			ItemList.SelectionType st;
			if(isFoldout) {
				st = _itemList.FoldoutGroup(title);
			} else {
				st = _itemList.ListItem(title);
			}
			switch(st) {
				case ItemList.SelectionType.Focus:
				case ItemList.SelectionType.Select:
					_listHasFocus = true;
					if (_selectedType != t) {
						_selectedType = t;
						ReloadWindow();
					}
					break;
			}
		}

		private void OnGUI() {
			if (_models == null) {
				return;
			}
			GUILayout.BeginHorizontal();

			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(_listWidth));
			{
				if(Event.current.isMouse) {
					_listHasFocus = true;
				}
				_itemList.Start(_listHasFocus);
					DrawChildren(typeof(Model));
				_itemList.End();
			}
			EditorGUILayout.EndScrollView();

			_listWidth += Controls.ResizeControl(position.height, _listWidth);

			if (_selectedType == null) {
				GUILayout.EndHorizontal();
				return;
			}
			Table.StartTable(!_listHasFocus);
			{
				Table.StartHeaders();
				{
					foreach (var header in _headers[_selectedType]) {
						Table.Header(header.Label, header.Width);
						header.Width += Table.HeaderResizeControl();
					}
				}
				Table.EndHeaders();
				Table.StartBody();
				if (_collection.Count == 0) {
					// kinda hacky but ok... add empty row to enable right click on tables that dont have any data
					HandleRowSelection(Table.StartRow(), null);
					Table.EndRow();
				} else {
					foreach (var instance in _collection) {
						HandleRowSelection(Table.StartRow(), instance.targetObject as Model);
						{
							var index = 0;
							Table.Cell(instance.targetObject.name, _headers[_selectedType][index].Width -1);
							var prop = instance.GetIterator();
							prop.NextVisible(true);

							while (prop.NextVisible(false)) {
								index++;
								var val = Utils.GetTargetObjectOfProperty(prop);
								string str = string.Empty;
								if (val != null && val.GetType().IsSubclassOf(typeof(Model))) {
									var name = (val as Model).name;
									str = string.IsNullOrEmpty(name) ? val.GetType().ToString() : name;
								} else if (val != null) {
									str = val.ToString();
								}
								Table.Cell(str, _headers[_selectedType][index].Width -1);
							}
						}
						Table.EndRow();
					}
				}
				Table.EndBody();
			}
			Table.EndTable();
			GUILayout.EndHorizontal();
		}

		private void HandleRowSelection(Table.SelectionType t, Model instance) {

			if (t != Table.SelectionType.None) {
				_listHasFocus = false;
			}

			switch (t) {
				case Table.SelectionType.ContextSelect:
					ContextMenu(instance);
					break;
				case Table.SelectionType.ContextOutside:
					ContextMenu(null);
					break;
				case Table.SelectionType.Delete:
					Delete(instance);
					break;
				case Table.SelectionType.Focus:
					ShowEditWindow(instance);
					break;
			}
		}

		private void ContextMenu(Model instance) {
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("New"), false, New);
			menu.AddSeparator("");
			if (instance != null) {
				menu.AddItem(new GUIContent("Duplicate"), false, Duplicate, instance);
			} else {
				menu.AddDisabledItem(new GUIContent("Duplicate"));
			}
			if (instance != null) {
				menu.AddItem(new GUIContent("Delete"), false, Delete, instance);
			} else {
				menu.AddDisabledItem(new GUIContent("Delete"));
			}
			menu.ShowAsContext();
		}

		private void New() {
			createAsset(_selectedType, null);	
		}

		private void createAsset(Type t, Model copy = null) {
			if (copy == null) {
				copy = CreateInstance(t) as Model;
				copy.name = "New " + t.Name;
			} else {
				copy = Instantiate(copy) as Model;
			}
			string assetDir = "Assets/Resources/" + t.Name;
			if(!Directory.Exists(assetDir)) {
				Directory.CreateDirectory(assetDir);
			}

			string assetName = copy.name;
			string assetPathAndName;
			int number = 1;
			while(File.Exists(assetPathAndName = assetDir + "/" + assetName + ".asset")) {
				if(number > 1) {
					assetName = "New " + t.Name + " " + number;
				}
				number++;
			}
			copy.name = assetName;
			AssetDatabase.CreateAsset(copy, assetPathAndName);
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = copy;
			ReloadWindow();
		}

		private void Delete(object instance) {
			if (instance == null) {
				return;
			}
			var path = AssetDatabase.GetAssetPath(instance as Model);
			if (path != null) {
				AssetDatabase.DeleteAsset(path);
			}
			DestroyImmediate(instance as Model);
			ReloadWindow();
		}

		private void ShowEditWindow(object instance) {
			if (instance == null) {
				return;
			}
			Selection.activeObject = instance as Model;
		}

		private void Duplicate(object instance) {
			createAsset(_selectedType, instance as Model);
		}
	}
}
