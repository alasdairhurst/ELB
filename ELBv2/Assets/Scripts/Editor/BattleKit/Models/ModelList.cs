using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using Engine.Data;
using BattleKit.Editor;
using System.Collections;
using System.Reflection;
using System.Text;

namespace BattleKit.Models {
	public sealed class ModelList : EditorWindow {
		[MenuItem("BattleKit/Model Browser")]
		public static void ShowWindow() {
			GetWindow<ModelList>("Models");
		}

		private static ILookup<Type, Type> _models;
		private int _drawIndex;
		private Vector2 _scrollPos;
		private readonly Dictionary<string, bool> _foldoutExpanded = new Dictionary<string, bool>();
		private int _selectedIndex;
		private int _keyboardControl;
		private readonly Dictionary<int, int> _idIndexes = new Dictionary<int, int>();
		private const int INDENT_WIDTH = 14;
		private const int ARROW_WIDTH = 14;
		private int _dragUpdatedOverId;
		private double _foldoutDestTime;
		private int _currentIndent;
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

		public ModelList() {
			if (_models == null)
				_models = GameState.GetModelTypes().ToLookup(model => model.BaseType, model => model);
		}

		private void DrawChildren(Type t) {
			foreach (var child in _models[t])
				if (_models[child].Any()) {
					if (FoldoutItem(child.Name)) {
						_selectedType = child;
					}
					if (_foldoutExpanded[child.Name]) {
						StartIndent();
						DrawChildren(child);
						EndIndent();
					}
				} else if (ListItem(child.Name)) {
					_selectedType = child;
				}
		}

		private void OnGUI() {

			GUILayout.BeginHorizontal();

			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(_listWidth));
			if (Event.current.isMouse) {
				_listHasFocus = true;
			}
			DrawChildren(typeof(Model));
			_idIndexes.Clear();
			_drawIndex = 0;
			EditorGUILayout.EndScrollView();

			ResizeControl();

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
							Table.Cell(propertyInfo.GetValue(instance, null) as string);
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

		private void ResizeControl() {
			GUILayout.Box(GUIContent.none, StyleStore.Border, GUILayout.Width(1), GUILayout.Height(position.height));

			var resizeHandle = new Rect {
				x = _listWidth,
				y = 0,
				width = 6,
				height = position.height
			};
			var controlID = GUIUtility.GetControlID(FocusType.Passive, resizeHandle);

			EditorGUIUtility.AddCursorRect(resizeHandle, MouseCursor.ResizeHorizontal);

			switch (Event.current.type) {
				case EventType.MouseDown:
					if (Event.current.button == 0
						&& resizeHandle.Contains(Event.current.mousePosition)
					) {
						GUIUtility.hotControl = controlID;
						Event.current.Use();
					}
					break;
				case EventType.MouseDrag:
					if (!Event.current.delta.x.Equals(0) &&
						GUIUtility.hotControl == controlID) {
						_listWidth = _listWidth + Event.current.delta.x;
						Event.current.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID) {
						GUIUtility.hotControl = 0;
						Event.current.Use();
					}
					break;
			}
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

		private void SetSelected(int controlID) {
			_selectedIndex = _idIndexes.FirstOrDefault(x => x.Value == controlID).Key;
		}

		private void SetIndex(int controlID) {
			_idIndexes[_drawIndex++] = controlID;
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

			SetIndex(controlID);
			var style = _listHasFocus ? StyleStore.LabelFocus : StyleStore.LabelNoFocus;
			var itemPosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 17f, 17f,
				style);
			var e = Event.current;
			var selected = _keyboardControl == controlID;
			switch (e.type) {
				case EventType.Repaint:
					style.Draw(itemPosition, GUIContent.none, false, GUIUtility.hotControl == controlID, false,
						selected);
					var textPosition = itemPosition;
					textPosition.x = INDENT_WIDTH * _currentIndent;
					style.Draw(textPosition, new GUIContent(text), false, GUIUtility.hotControl == controlID, false,
						selected);
					break;
				case EventType.MouseDown:
					if ((e.button == 0) && itemPosition.Contains(e.mousePosition)) {
						_listHasFocus = true;
						GUIUtility.hotControl = controlID;
						_keyboardControl = controlID;
						Event.current.Use();
						SetSelected(controlID);
					}
					break;
				case EventType.MouseUp:
					if ((e.button == 0) && (GUIUtility.hotControl == controlID)) {
						GUIUtility.hotControl = 0;
						Event.current.Use();
					}
					break;
				case EventType.KeyDown:
					if (_listHasFocus && e.keyCode == KeyCode.UpArrow && HasPrevious() && (controlID == GetPreviousId())) {
						_keyboardControl = PreviousId();
						Event.current.Use();
						//SetSelected(controlID);
					}
					if (_listHasFocus && e.keyCode == KeyCode.DownArrow && HasNext() && (controlID == GetNextId())) {
						_keyboardControl = NextId();
						Event.current.Use();
						//SetSelected(controlID);
					}
					break;
			}
			return _keyboardControl == controlID;
		}

		private bool FoldoutItem(string text) {
			if (!_foldoutExpanded.ContainsKey(text))
				_foldoutExpanded[text] = true;
			var foldout = _foldoutExpanded[text];
			var itemPosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 17f, 17f,
				StyleStore.LabelFocus);
			var content = new GUIContent(text);
			var style = StyleStore.Foldout;
			var controlID = GUIUtility.GetControlID(FocusType.Keyboard, itemPosition);
			SetIndex(controlID);
			var eventType = Event.current.type;
			if (!GUI.enabled &&
			    ((Event.current.rawType == EventType.MouseDown) || (Event.current.rawType == EventType.MouseDrag) ||
			     (Event.current.rawType == EventType.MouseUp)))
				eventType = Event.current.rawType;
			var eventType2 = eventType;
			switch (eventType2) {
				case EventType.MouseDown:
					if (itemPosition.Contains(Event.current.mousePosition) && (Event.current.button == 0)) {
						GUIUtility.hotControl = controlID;
						_keyboardControl = controlID;
						SetSelected(controlID);
						Event.current.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID) {
						GUIUtility.hotControl = 0;
						Event.current.Use();
						var rect2 = itemPosition;
						rect2.width = style.padding.left;
						rect2.x += INDENT_WIDTH * _currentIndent;
						if (rect2.Contains(Event.current.mousePosition)) {
							_foldoutExpanded[text] = !foldout;
						}
					}
					break;
				case EventType.MouseMove:
				case EventType.KeyUp:
				case EventType.ScrollWheel:
				case EventType.Layout:
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
						Event.current.Use();
					break;
				case EventType.KeyDown:
					var keyCode = Event.current.keyCode;
					if (_keyboardControl == controlID) {
						if (((keyCode == KeyCode.LeftArrow) && foldout) || ((keyCode == KeyCode.RightArrow) && !foldout)) {
							Event.current.Use();
							_foldoutExpanded[text] = !foldout;
						}
					} else if (keyCode == KeyCode.UpArrow && HasPrevious() && (controlID == GetPreviousId())) {
						_keyboardControl = PreviousId();
						Event.current.Use();
						SetSelected(controlID);
					} else if (keyCode == KeyCode.DownArrow && HasNext() && (controlID == GetNextId())) {
						_keyboardControl = NextId();
						Event.current.Use();
						SetSelected(controlID);
					}
					break;
				case EventType.Repaint: {
					var bgStyle = _listHasFocus ? StyleStore.LabelFocus : StyleStore.LabelNoFocus;
					bgStyle.Draw(itemPosition, GUIContent.none, false, controlID == GUIUtility.hotControl, foldout,
						controlID == _keyboardControl);
					var position2 = new Rect(INDENT_WIDTH * _currentIndent, itemPosition.y,
						EditorGUIUtility.labelWidth - ARROW_WIDTH, itemPosition.height);
					style.Draw(position2, content, false, controlID == GUIUtility.hotControl, foldout,
						controlID == _keyboardControl);
					break;
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
					break;
			}
			return _keyboardControl == controlID;
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
			(instance as Model).Delete();
			_collection.Remove(instance);
		}

		private void ShowEditWindow(object instance) {
			ModelWizard.ShowWizard(instance as Model);
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