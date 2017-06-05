using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using BattleKit.Engine;

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
		private IOrderedEnumerable<PropertyInfo> _props;
		private float _listWidth = 200;
		private bool _listHasFocus = true;

		private void LoadInfo() {
			if (_selectedType != _loadedInfoForType) {
				_collection =
					typeof(GameState).GetMethod("FetchAll")
					.MakeGenericMethod(_selectedType)
					.Invoke(null, new object[] { false }) as IList;

				_props = _selectedType.GetProperties().OrderBy(prop => prop.Name);
				_headers = _props.Select(prop => {
					var header = new TableHeader { Label = prop.Name, Width = 100 };
					switch (header.Label) {
						case "_Id":
							header.Label = "ID";
							header.Width = 23;
							break;
						case "_EditorId":
							header.Label = "Editor ID";
							break;
					}
					return header;
				}).ToArray();
				_loadedInfoForType = _selectedType;
			}
		}

		public ModelBrowser() {
			if (_models == null)
				_models = GameState.GetModelTypes().ToLookup(model => model.BaseType, model => model);
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
				foreach (var instance in _collection) {

					HandleRowSelection(Table.StartRow(), instance as Model);
					{
						foreach (var propertyInfo in _props) {
							var val = propertyInfo.GetValue(instance, null);
							if(val != null && propertyInfo.PropertyType.IsSubclassOf(typeof(Model))) {
								val = (val as Model).ToString(Engine.StringOpts.Identifier);
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
				case Table.SelectionType.Select:
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
				menu.AddItem(new GUIContent("Copy"), false, Copy, instance);
			} else {
				menu.AddDisabledItem(new GUIContent("Copy"));
			}
			if (CanPaste()) {
				menu.AddItem(new GUIContent("Paste"), false, Paste);
			} else {
				menu.AddDisabledItem(new GUIContent("Paste"));
			}
			if (instance != null) {
				menu.AddItem(new GUIContent("Delete"), false, Delete, instance);
			} else {
				menu.AddDisabledItem(new GUIContent("Delete"));
			}
			menu.ShowAsContext();
		}

		private void New() {
			ModelWizard.ShowWizard(_selectedType);
		}

		private void Delete(object instance) {
			if (instance == null) {
				return;
			}
			(instance as Model).Delete();
			_collection.Remove(instance);
		}

		private void ShowEditWindow(object instance) {
			if (instance == null) {
				return;
			}
			Selection.objects = new UnityEngine.Object[] { instance as Model };

			//ModelWizard.ShowWizard(instance as Model);
		}

		private bool CanPaste() {
			return copied;
		}

		private bool copied = false;

		private void Copy(object instance) {
			copied = true;
		}

		private void Paste() {
			copied = false;
		}
	}
}
