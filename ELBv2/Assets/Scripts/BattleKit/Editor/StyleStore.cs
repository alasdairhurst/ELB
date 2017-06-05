using UnityEditor;
using UnityEngine;

namespace BattleKit.Editor {
	public class StyleStore {
		public static Texture2D ColourTexture(byte r = 255, byte g = 255, byte b = 255, byte a = 255) {
			var tex = new Texture2D(1, 1);
			tex.SetPixel(0, 0, new Color32(r, g, b, a));
			tex.Apply();
			return tex;
		}

		public static GUIStyle FoldoutStyle( ) {
			var myFoldoutStyle = new GUIStyle(EditorStyles.foldout) {
				focused = { textColor = Color.white },
				onFocused = { textColor = Color.white },
				active = { textColor = Color.white },
				onActive = { textColor = Color.white },
				padding = new RectOffset(14, 0, 2, 2)
			};
			myFoldoutStyle.focused.background = myFoldoutStyle.normal.background;
			myFoldoutStyle.onFocused.background = myFoldoutStyle.onNormal.background;
			myFoldoutStyle.active.background = myFoldoutStyle.normal.background;
			myFoldoutStyle.onActive.background = myFoldoutStyle.onNormal.background;
			return myFoldoutStyle;
		}

		public static GUIStyle BorderStyle( ) {
			return new GUIStyle {
				normal = new GUIStyleState {
					background = ColourTexture(143, 143, 143)
				},
				onNormal = new GUIStyleState {
					background = ColourTexture(143, 143, 143)
				},
				padding = new RectOffset(2, 2, 2, 2)
			};
		}

		public static GUIStyle LabelFocusStyle() {
			return LabelTextureStyle(ColourTexture(62, 125, 231));
		}

		public static GUIStyle LabelUnfocusedStyle( ) {
			return LabelTextureStyle(ColourTexture(143, 143, 143));
		}

		public static GUIStyle LabelTextureStyle(Texture2D background) {
			return new GUIStyle {
				active = new GUIStyleState {
					background = background,
					textColor = Color.white
				},
				onActive = new GUIStyleState {
					background = background,
					textColor = Color.white
				},
				focused = new GUIStyleState {
					background = background,
					textColor = Color.white
				},
				onFocused = new GUIStyleState {
					background = background,
					textColor = Color.white
				},
				padding = new RectOffset(14, 0, 2, 2)
			};
		}

		public static GUIStyle ToolbarButtonStyle( ) {
			return new GUIStyle("ToolbarButton") {
				alignment = TextAnchor.MiddleLeft
			};
		}

		public static GUIStyle TableCellStyle( ) {
			var style = new GUIStyle {
				active = new GUIStyleState {
					textColor = Color.white,
					background = new Texture2D(0, 0)
				},
				onActive = new GUIStyleState {
					textColor = Color.white,
					background = new Texture2D(0, 0)
				},
				focused = new GUIStyleState {
					textColor = Color.white,
					background = new Texture2D(0, 0)
				},
				onFocused = new GUIStyleState {
					textColor = Color.white,
					background = new Texture2D(0, 0)
				},
				padding = new RectOffset(2, 2, 2, 2),
				wordWrap = false,
				clipping = TextClipping.Clip,
				alignment = TextAnchor.MiddleLeft
			};
			return style;
		}
	}
}
