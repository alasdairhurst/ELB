using System;
using System.Collections.Generic;
using System.Linq;	 
using UnityEditor;
using UnityEngine;

namespace BattleKit.Editor {
	class ItemList {
		private readonly Dictionary<int, int> _idIndexes = new Dictionary<int, int>();
		private readonly Dictionary<string, bool> _foldoutExpanded = new Dictionary<string, bool>();
		private Stack<string> _indentIDStack = new Stack<string>();
		private bool _hasFocus = true;
		private int _keyboardControl;
		private string _currentFoldoutID;
		private int _selectedIndex;
		private int _drawIndex;
		private const int INDENT_WIDTH = 14;
		private const int ARROW_WIDTH = 14;
		private int _dragUpdatedOverId;
		private double _foldoutDestTime;
		private bool _selectionSet = false;
		private string _preselection;

		private int NextId( ) {
			_selectedIndex++;
			return _idIndexes[_selectedIndex];
		}

		private void SetSelected(int controlID) {
			_selectedIndex = _idIndexes.FirstOrDefault(x => x.Value == controlID).Key;
		}

		private void SetIndex(int controlID, string text) {
			int index = _drawIndex++;
			_idIndexes[index] = controlID;
			if(!_selectionSet && (_preselection == null || text == _preselection)) {
				_keyboardControl = controlID;
				_selectedIndex = index;
				_selectionSet = true;
			}
		}

		private bool HasNext( ) {
			return _idIndexes.ContainsKey(_selectedIndex + 1);
		}

		private bool HasPrevious( ) {
			return _idIndexes.ContainsKey(_selectedIndex - 1);
		}

		private int PreviousId( ) {
			_selectedIndex--;
			return _idIndexes[_selectedIndex];
		}

		private int GetPreviousId( ) {
			return _idIndexes[_selectedIndex - 1];
		}

		private int GetNextId( ) {
			return _idIndexes[_selectedIndex + 1];
		}

		public void Preselect(string title) {
			_preselection = title;
		}

		public void Start( ) {
			_drawIndex = 0;
		}

		public void End( ) {
			_idIndexes.Clear();
		}

		public bool FoldoutGroup(string text) {
			if(_indentIDStack.Count != 0 && !_foldoutExpanded[_indentIDStack.Peek()]) {
				return false;
			}
			_indentIDStack.Push(text);
			return foldoutItem(text);
		}

		public void EndFoldoutGroup( ) {
			_indentIDStack.Pop();
		}

		public bool ListItem(string text) {
			if(_indentIDStack.Count != 0 && !_foldoutExpanded[_indentIDStack.Peek()]) {
				return false;
			}
			return listItem(text);
		}

		private bool listItem(string text) {
			var controlID = GUIUtility.GetControlID(FocusType.Keyboard);
			SetIndex(controlID, text);
			var style = _hasFocus ? StyleStore.LabelFocus : StyleStore.LabelNoFocus;
			var itemPosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 17f, 17f,
				style);
			var e = Event.current;
			var selected = _keyboardControl == controlID;
			switch(e.type) {
				case EventType.Repaint:
					style.Draw(itemPosition, GUIContent.none, false, GUIUtility.hotControl == controlID, false,
						selected);
					var textPosition = itemPosition;
					textPosition.x = INDENT_WIDTH * _indentIDStack.Count;
					style.Draw(textPosition, new GUIContent(text), false, GUIUtility.hotControl == controlID, false,
						selected);
					break;
				case EventType.MouseDown:
					if((e.button == 0) && itemPosition.Contains(e.mousePosition)) {
						_hasFocus = true;
						GUIUtility.hotControl = controlID;
						_keyboardControl = controlID;
						Event.current.Use();
						SetSelected(controlID);
					}
					break;
				case EventType.MouseUp:
					if((e.button == 0) && (GUIUtility.hotControl == controlID)) {
						GUIUtility.hotControl = 0;
						Event.current.Use();
					}
					break;
				case EventType.KeyDown:
					if(_hasFocus && e.keyCode == KeyCode.UpArrow && HasPrevious() && (controlID == GetPreviousId())) {
						_keyboardControl = PreviousId();
						Event.current.Use();
						//SetSelected(controlID);
					}
					if(_hasFocus && e.keyCode == KeyCode.DownArrow && HasNext() && (controlID == GetNextId())) {
						_keyboardControl = NextId();
						Event.current.Use();
						//SetSelected(controlID);
					}
					break;
			}
			return _keyboardControl == controlID;
		}

		private bool foldoutItem(string text) {
			if(!_foldoutExpanded.ContainsKey(text))
				_foldoutExpanded[text] = true;
			var foldout = _foldoutExpanded[text];
			var itemPosition = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 17f, 17f,
				StyleStore.LabelFocus);
			var content = new GUIContent(text);
			var style = StyleStore.Foldout;
			var controlID = GUIUtility.GetControlID(FocusType.Keyboard, itemPosition);
			SetIndex(controlID, text);
			var eventType = Event.current.type;
			if(!GUI.enabled &&
				((Event.current.rawType == EventType.MouseDown) || (Event.current.rawType == EventType.MouseDrag) ||
				 (Event.current.rawType == EventType.MouseUp)))
				eventType = Event.current.rawType;
			var eventType2 = eventType;
			switch(eventType2) {
				case EventType.MouseDown:
					if(itemPosition.Contains(Event.current.mousePosition) && (Event.current.button == 0)) {
						GUIUtility.hotControl = controlID;
						_keyboardControl = controlID;
						SetSelected(controlID);
						Event.current.Use();
					}
					break;
				case EventType.MouseUp:
					if(GUIUtility.hotControl == controlID) {
						GUIUtility.hotControl = 0;
						Event.current.Use();
						var rect2 = itemPosition;
						rect2.width = style.padding.left;
						rect2.x += INDENT_WIDTH * _indentIDStack.Count;
						if(rect2.Contains(Event.current.mousePosition)) {
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
					if(GUIUtility.hotControl == controlID)
						Event.current.Use();
					break;
				case EventType.KeyDown:
					var keyCode = Event.current.keyCode;
					if(_keyboardControl == controlID) {
						if(((keyCode == KeyCode.LeftArrow) && foldout) || ((keyCode == KeyCode.RightArrow) && !foldout)) {
							Event.current.Use();
							_foldoutExpanded[text] = !foldout;
						}
					} else if(keyCode == KeyCode.UpArrow && HasPrevious() && (controlID == GetPreviousId())) {
						_keyboardControl = PreviousId();
						Event.current.Use();
						SetSelected(controlID);
					} else if(keyCode == KeyCode.DownArrow && HasNext() && (controlID == GetNextId())) {
						_keyboardControl = NextId();
						Event.current.Use();
						SetSelected(controlID);
					}
					break;
				case EventType.Repaint: {
						var bgStyle = _hasFocus ? StyleStore.LabelFocus : StyleStore.LabelNoFocus;
						bgStyle.Draw(itemPosition, GUIContent.none, false, controlID == GUIUtility.hotControl, foldout,
							controlID == _keyboardControl);
						var position2 = new Rect(INDENT_WIDTH * _indentIDStack.Count, itemPosition.y,
							EditorGUIUtility.labelWidth - ARROW_WIDTH, itemPosition.height);
						style.Draw(position2, content, false, controlID == GUIUtility.hotControl, foldout,
							controlID == _keyboardControl);
						break;
					}
				case EventType.DragUpdated:
					if(_dragUpdatedOverId == controlID) {
						if(itemPosition.Contains(Event.current.mousePosition)) {
							if(Time.realtimeSinceStartup > _foldoutDestTime) {
								_foldoutExpanded[text] = true;
								Event.current.Use();
							}
						} else {
							_dragUpdatedOverId = 0;
						}
					} else if(itemPosition.Contains(Event.current.mousePosition)) {
						_dragUpdatedOverId = controlID;
						_foldoutDestTime = Time.realtimeSinceStartup + 0.7;
						Event.current.Use();
					}
					break;
			}
			return _keyboardControl == controlID;
		}
	}

}
