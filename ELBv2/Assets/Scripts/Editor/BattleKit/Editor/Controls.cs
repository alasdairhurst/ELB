using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BattleKit.Editor {
	static class Controls {
		public static float ResizeControl(float height, float xPos) {
			GUILayout.Box(GUIContent.none, StyleStore.Border, GUILayout.Width(1), GUILayout.Height(height));

			var resizeHandle = new Rect {
				x = xPos,
				y = 0,
				width = 6,
				height = height
			};
			var controlID = GUIUtility.GetControlID(FocusType.Passive, resizeHandle);

			EditorGUIUtility.AddCursorRect(resizeHandle, MouseCursor.ResizeHorizontal);

			switch(Event.current.type) {
				case EventType.MouseDown:
					if(Event.current.button == 0
						&& resizeHandle.Contains(Event.current.mousePosition)
					) {
						GUIUtility.hotControl = controlID;
						Event.current.Use();
					}
					break;
				case EventType.MouseDrag:
					if(!Event.current.delta.x.Equals(0) &&
						GUIUtility.hotControl == controlID) {
						xPos = xPos + Event.current.delta.x;
						Event.current.Use();
					}
					break;
				case EventType.MouseUp:
					if(GUIUtility.hotControl == controlID) {
						GUIUtility.hotControl = 0;
						Event.current.Use();
					}
					break;
			}
			return xPos;
		}
	}
}
