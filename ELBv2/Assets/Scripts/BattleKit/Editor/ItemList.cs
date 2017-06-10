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
		private int _selectedRowIndex;
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

		public void Select(string title) {
			_selectionSet = false;
			_preselection = title;
		}

		public void Start(bool hasFocus) {
			_hasFocus = hasFocus;
			_drawIndex = 0;
		}

		public void End( ) {
			_idIndexes.Clear();
		}

		public enum SelectionType {
			None,
			Select,
			ContextSelect,
			Focus,
			Delete,
		}

		public SelectionType FoldoutGroup(string text) {
			if(_indentIDStack.Count != 0 && !_foldoutExpanded[_indentIDStack.Peek()]) {
				return SelectionType.None;
			}
			_indentIDStack.Push(text);
			EditorGUI.indentLevel++;
			return listItem(text, true);
		}

		public void EndFoldoutGroup( ) {
			_indentIDStack.Pop();
			EditorGUI.indentLevel--;
		}


		public SelectionType ListItem(string text) {
			if(_indentIDStack.Count != 0 && !_foldoutExpanded[_indentIDStack.Peek()]) {
				return SelectionType.None;
			}
			return listItem(text, false);
		}

		private SelectionType listItem(string text, bool isFoldout) {

			if(isFoldout && !_foldoutExpanded.ContainsKey(text)) {
				_foldoutExpanded[text] = true;
			}
			var foldout = isFoldout && _foldoutExpanded[text];

			var style = isFoldout ? StyleStore.FoldoutStyle() : _hasFocus ? StyleStore.LabelFocusStyle() : StyleStore.LabelUnfocusedStyle();
			var rect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, 17f, 17f,
				isFoldout ? StyleStore.LabelFocusStyle() : style);


			var controlID = GUIUtility.GetControlID(FocusType.Keyboard, rect);
			SetIndex(controlID, text);

			var e = Event.current;
			var selected = _keyboardControl == controlID;
			//SelectionType st = selected && _hasFocus ? SelectionType.Focus : SelectionType.None;
			SelectionType st = SelectionType.None;
			switch (e.type) {
				case EventType.Repaint:
					var textPosition = rect;
					textPosition.x = INDENT_WIDTH * EditorGUI.indentLevel;

					if(isFoldout) {
						var bgStyle = _hasFocus ? StyleStore.LabelFocusStyle() : StyleStore.LabelUnfocusedStyle();
						bgStyle.Draw(rect, GUIContent.none, false, controlID == GUIUtility.hotControl, foldout,
							selected);
						textPosition.width = EditorGUIUtility.labelWidth - ARROW_WIDTH;
					} else {
						style.Draw(rect, GUIContent.none, false, GUIUtility.hotControl == controlID, false,
						selected);
					}
					style.Draw(textPosition, new GUIContent(text), false, GUIUtility.hotControl == controlID, isFoldout && foldout, selected);
					break;
				case EventType.ContextClick:
					if(rect.Contains(e.mousePosition) && selected) {
						e.Use();
						st = SelectionType.ContextSelect;
					}
					break;
				case EventType.MouseDown:
					if((e.button == 0) && rect.Contains(e.mousePosition)) {
						switch(Event.current.clickCount) {
							case 1:
								_hasFocus = true;
								GUIUtility.hotControl = controlID;
								_keyboardControl = controlID;
								e.Use();
								SetSelected(controlID);
								st = SelectionType.Focus;
								break;
							case 2:
								e.Use();
								st = SelectionType.Select;
								break;
						}
					}
					break;
				case EventType.MouseUp:
					if((e.button == 0) && (GUIUtility.hotControl == controlID)) {
						GUIUtility.hotControl = 0;
						e.Use();
						if(isFoldout) {
							var rect2 = rect;
							rect2.width = style.padding.left;
							rect2.x += INDENT_WIDTH * _indentIDStack.Count;
							if(rect2.Contains(e.mousePosition)) {
								_foldoutExpanded[text] = !foldout;
							}
						}
					}
					break;
				case EventType.MouseDrag:
					if(GUIUtility.hotControl == controlID)
						e.Use();
					break;
				case EventType.DragUpdated:
					if(_dragUpdatedOverId == controlID) {
						if(rect.Contains(e.mousePosition)) {
							if(Time.realtimeSinceStartup > _foldoutDestTime) {
								_foldoutExpanded[text] = true;
								e.Use();
							}
						} else {
							_dragUpdatedOverId = 0;
						}
					} else if(rect.Contains(e.mousePosition)) {
						_dragUpdatedOverId = controlID;
						_foldoutDestTime = Time.realtimeSinceStartup + 0.7;
						e.Use();
					}
					break;
				case EventType.KeyDown: {
					if(!_hasFocus) {
						break;
					}
					switch(e.keyCode) {
						case KeyCode.LeftArrow: {
								if(foldout) {
									e.Use();
									_foldoutExpanded[text] = false;
								}
								break;
							}
						case KeyCode.RightArrow: {
								if(!foldout) {
									e.Use();
									_foldoutExpanded[text] = true;
								}
								break;
							}
						case KeyCode.UpArrow: {
								if(HasPrevious() && controlID == GetPreviousId()) {
									_keyboardControl = PreviousId();
									e.Use();
								}
								break;
							}
						case KeyCode.DownArrow: {
								if(HasNext() && controlID == GetNextId()) {
									_keyboardControl = NextId();
									e.Use();
								}
								break;
							}
						case KeyCode.KeypadEnter:
						case KeyCode.Return: {
								if(_keyboardControl == controlID) {
									e.Use();
									st = SelectionType.Select;
								}
								break;
							}
						case KeyCode.Delete: {
								if(_keyboardControl == controlID) {
									e.Use();
									return SelectionType.Delete;
								}
								break;
							}
					}
					break;
				}
			}
			return st;
		}
	}
}
