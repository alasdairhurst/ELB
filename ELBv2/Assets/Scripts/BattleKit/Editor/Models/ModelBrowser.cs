using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using BattleKit.Engine;
using System.IO;
using System.Collections.Generic;

namespace BattleKit.Editor {
	public sealed class ModelBrowser : EditorWindow {
		[MenuItem("BattleKit/Model Browser")]
		public static void ShowWindow() {
			GetWindow<ModelBrowser>("Models");
		}

		private static ILookup<Type, Type> _models;

		private ItemList _itemList = new ItemList();
		private Vector2 _scrollPos;
		private Type _selectedType;
		private Type _loadedInfoForType;
		private IList _collection;
		private TableHeader[] _headers;
		private IOrderedEnumerable<FieldInfo> _props;
		private float _listWidth = 200;
		private bool _listHasFocus = true;

		private void LoadInfo(bool force = false) {
			if (_selectedType != _loadedInfoForType || force) {
				_collection = Resources.FindObjectsOfTypeAll(_selectedType).Where(t => t.GetType() == _selectedType).ToList();

				SerializedObject selection;
				if(_collection.Count > 0) {
					selection = new SerializedObject(_collection[0] as Model);
				} else {
					ScriptableObject objectForType = CreateInstance(_selectedType);
					selection = new SerializedObject(objectForType);
					DestroyImmediate(objectForType);
				}

				_props = _selectedType.GetFields().Where(prop => selection.FindProperty(prop.Name) != null).OrderBy(prop => prop.Name);

				System.Collections.Generic.List<TableHeader> headers = _props.Select(prop => {
					return new TableHeader { Label = prop.Name, Width = 100 };
				}).ToList();
				headers.Insert(0, new TableHeader { Label = "Asset ID", Width = 100 });
				_headers = headers.ToArray();
				_loadedInfoForType = _selectedType;
				
			}
		}

		public ModelBrowser() {
			if(_models == null)
				_models = typeof(Model).Assembly.GetTypes().Where(
					type => type.IsSubclassOf(typeof(Model))
				).ToLookup(
					model => model.BaseType, model => model
				);
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
				st = _itemList.ListItem(title, true);
			}
			switch(st) {
				case ItemList.SelectionType.Focus:
					_listHasFocus = true;
					_selectedType = t;
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
				_itemList.SetFocus(_listHasFocus);
				_itemList.Start();
					DrawChildren(typeof(Model));
				_itemList.End();
			}
			EditorGUILayout.EndScrollView();

			_listWidth += Controls.ResizeControl(position.height, _listWidth);

			if (_selectedType == null) {
				GUILayout.EndHorizontal();
				return;
			}
			LoadInfo();
			Table.StartTable(!_listHasFocus);
			{
				
				Table.Headers(_headers);
				Table.StartBody();
				foreach (Model instance in _collection) {
					HandleRowSelection(Table.StartRow(), instance as Model);
					{
						Table.Cell((instance as Model).name);
						foreach (var propertyInfo in _props) {
							var val = propertyInfo.GetValue(instance);
							if(val != null && propertyInfo.FieldType.IsSubclassOf(typeof(Model))) {
								val = (val as Model).ToString(true);
							}
							Table.Cell(val as string);
						}
					}
					Table.EndRow();
				}
				if (_collection.Count == 0) {
					// kinda hacky but ok... add empty row to enable right click on tables that dont have any data
					HandleRowSelection(Table.StartRow(), null);
					Table.EndRow();
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
			if (instance != null) {
				menu.AddItem(new GUIContent("Edit"), false, ShowEditWindow, instance);
			} else {
				menu.AddDisabledItem(new GUIContent("Edit"));
			}
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

		private void New( ) {
			createAsset(_selectedType, null);
		}

		private void createAsset(Type t, Model copy = null) {
			if (copy == null) {
				copy = CreateInstance(t) as Model;
				copy.name = "New " + t.Name;
			} else {
				copy = Instantiate(copy) as Model;
			}
			string assetDir = "Assets/Data/" + t.Name;
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

			AssetDatabase.CreateAsset(copy, assetPathAndName);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = copy;
			LoadInfo(true);

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
			LoadInfo(true);
		}

		private void ShowEditWindow(object instance) {
			if (instance == null) {
				return;
			}
			Selection.objects = new UnityEngine.Object[] { instance as Model };
		}

		private void Duplicate(object instance) {
			createAsset(_selectedType, instance as Model);
		}
	}
}
