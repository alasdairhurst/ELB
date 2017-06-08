using UnityEditor;
using UnityEngine;

namespace BattleKit.Editor {

	public class TableHeader {
		public string Label;
		public float MinWidth = 20;
		public float MaxWidth = float.PositiveInfinity;
		private float _width;
		public float Width {
			get { return _width; }
			set {
				_width = (value < MinWidth) ? MinWidth : (value > MaxWidth) ? MaxWidth : value;
			}
		}
	}

	static class Table {
		private const int RESIZE_HANDLE_SIZE = 4;
		private const int LEFT_PADDING = 6;
		private static TableHeader[] _headers;
		private static int _selectedRowIndex;
		private static int _drawIndexCol;
		private static int _drawIndexRow;
		private static Vector2 _scrollPos;
		private static bool _hasFocus;

		public static void StartTable(bool hasFocus = true) {
			_scrollPos = GUILayout.BeginScrollView(_scrollPos);
			_hasFocus = hasFocus;
			
		}

		public static void Headers(TableHeader[] headers) {
			_headers = headers;
			GUILayout.BeginHorizontal("Toolbar");
			foreach (var tableHeader in _headers) {
				Header(tableHeader);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		public static void StartBody() {
			_drawIndexRow = 0;
		}

		public enum SelectionType {
			None,
			Select,
			ContextSelect,
			ContextOutside,
			Delete,
			Focus
		}

		public static SelectionType StartRow() {
			var rect = EditorGUILayout.BeginHorizontal();
			GUILayout.Space(LEFT_PADDING);
			GUILayout.Box(GUIContent.none, StyleStore.BorderStyle(), GUILayout.Width(1));
			_drawIndexCol = 0;

			var controlID = GUIUtility.GetControlID(FocusType.Keyboard);
			var st = SelectionType.None;
			bool rowSelected = _selectedRowIndex == _drawIndexRow;
			if(rowSelected && _hasFocus) {
				GUIUtility.keyboardControl = controlID;
				st = SelectionType.Focus;
			}
			var e = Event.current;
			switch (e.type) {
				case EventType.Repaint:
					var style = _hasFocus ? StyleStore.LabelFocusStyle() : StyleStore.LabelUnfocusedStyle();
					style.Draw(rect, GUIContent.none, false, false, false, rowSelected);
					break;
				case EventType.MouseDown: {
					bool stSet = false;
					if (rect.Contains(e.mousePosition)) {
						switch (Event.current.clickCount) {
							case 1:
								GUIUtility.hotControl = controlID;
								GUIUtility.keyboardControl = controlID;
								_selectedRowIndex = _drawIndexRow;
								e.Use();
								break;
							case 2:
								e.Use();
								st = SelectionType.Select;
								stSet = true;
								break;
						}
					}
					if (!stSet) {
						st = SelectionType.Focus;
					}
					break;
				}
				case EventType.MouseUp: {
					if (e.button == 0 && GUIUtility.hotControl == controlID) {
						GUIUtility.hotControl = 0;
						e.Use();
					}
					break;
				}
				case EventType.ContextClick:
					if (rect.Contains(e.mousePosition) && GUIUtility.keyboardControl == controlID) {
						e.Use();
						st = SelectionType.ContextSelect;
						break;
					}
					st = SelectionType.ContextOutside;
					break;
				case EventType.KeyDown: {
					switch (e.keyCode) {
						case KeyCode.UpArrow: {
							_selectedRowIndex--;
							e.Use();
							break;
						}
						case KeyCode.DownArrow: {
							_selectedRowIndex++;
							e.Use();
							break;
						}
						case KeyCode.KeypadEnter:
						case KeyCode.Return: {
							if (GUIUtility.keyboardControl == controlID) {
								e.Use();
								st = SelectionType.Select;
							}
							break;
						}
						case KeyCode.Delete: {
							if (GUIUtility.keyboardControl == controlID) {
								e.Use();
								st = SelectionType.Delete;
							}
							break;
						}
					}
					break;
				}
			}
			return st;
		}

		public static void Cell(string text) {
			var rect = GUILayoutUtility.GetRect(new GUIContent(text), StyleStore.TableCellStyle(), GUILayout.Width(_headers[_drawIndexCol].Width - 1));

			if (Event.current.type == EventType.repaint) {
				StyleStore.TableCellStyle().Draw(rect, new GUIContent(text), false, false, false, _drawIndexRow == _selectedRowIndex);
			}
			_drawIndexCol++;
			GUILayout.Box(GUIContent.none, StyleStore.BorderStyle(), GUILayout.Width(1));
		}

		public static void EndRow() {
			EditorGUILayout.EndHorizontal();
			GUILayout.Box(GUIContent.none, StyleStore.BorderStyle(), GUILayout.Height(1));
			_drawIndexRow++;
		}

		public static void EndBody() {
			if (_selectedRowIndex > _drawIndexRow -1) {
				_selectedRowIndex = _drawIndexRow -1;
			} else if (_selectedRowIndex < 0) {
				_selectedRowIndex = 0;
			}
		}

		public static void EndTable() {
			GUILayout.EndScrollView();
		}

		public static bool Header(TableHeader header) {
			// var controlID = GUIUtility.GetControlID(FocusType.Passive);
			var label = new GUIContent(header.Label);
			var headerPos = GUILayoutUtility.GetRect(label, StyleStore.ToolbarButtonStyle(), GUILayout.Width(header.Width));

			header.Width += Controls.ResizeControl(headerPos.height, headerPos.x + headerPos.width, true);

			return GUI.Button(headerPos, label, StyleStore.ToolbarButtonStyle());
		}
	}
}
