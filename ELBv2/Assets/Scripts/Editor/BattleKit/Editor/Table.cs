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
		private const float DOUBLE_CLICK_TIME = 10;
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
			GUILayout.Box(GUIContent.none, StyleStore.Border, GUILayout.Width(1));
			_drawIndexCol = 0;

			var controlID = GUIUtility.GetControlID(FocusType.Keyboard);
			if (_selectedRowIndex == _drawIndexRow && _hasFocus) {
				GUIUtility.keyboardControl = controlID;
			}
			var e = Event.current;
			switch (e.type) {
				case EventType.Repaint:
					var style = _hasFocus ? StyleStore.LabelFocus : StyleStore.LabelNoFocus;
					style.Draw(rect, GUIContent.none, false, false, false, _drawIndexRow == _selectedRowIndex);
					break;
				case EventType.MouseDown: {
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
								return SelectionType.Select;
						}
					}
					return SelectionType.Focus;
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
						return SelectionType.ContextSelect;
					}
					return SelectionType.ContextOutside;
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
						case KeyCode.Return: {
							if (GUIUtility.keyboardControl == controlID) {
								e.Use();
								return SelectionType.Select;
							}
							break;
						}
						case KeyCode.Delete: {
							if (GUIUtility.keyboardControl == controlID) {
								e.Use();
								return SelectionType.Delete;
							}
							break;
						}
					}
					break;
				}
			}
			return SelectionType.None;
		}

		public static void Cell(string text) {
			var rect = GUILayoutUtility.GetRect(new GUIContent(text), StyleStore.TableCell, GUILayout.Width(_headers[_drawIndexCol].Width - 1));

			if (Event.current.type == EventType.repaint) {
				StyleStore.TableCell.Draw(rect, new GUIContent(text), false, false, false, _drawIndexRow == _selectedRowIndex);
			}
			_drawIndexCol++;
			GUILayout.Box(GUIContent.none, StyleStore.Border, GUILayout.Width(1));
		}

		public static void EndRow() {
			EditorGUILayout.EndHorizontal();
			GUILayout.Box(GUIContent.none, StyleStore.Border, GUILayout.Height(1));
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
			var controlID = GUIUtility.GetControlID(FocusType.Passive);

			var label = new GUIContent(header.Label);
			var headerPos = GUILayoutUtility.GetRect(label, StyleStore.ToolbarButton, GUILayout.Width(header.Width));
			var resizeHandle = headerPos;
			resizeHandle.x = resizeHandle.x + headerPos.width;
			resizeHandle.width = RESIZE_HANDLE_SIZE;

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
						Event.current.Use();
						header.Width = header.Width + Event.current.delta.x;
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID) {
						GUIUtility.hotControl = 0;
						Event.current.Use();
					}
					break;
			}

			return GUI.Button(headerPos, label, StyleStore.ToolbarButton);
		}
	}
}
