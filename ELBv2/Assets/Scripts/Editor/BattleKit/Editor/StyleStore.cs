using UnityEditor;
using UnityEngine;

public static class StyleStore {
	private static readonly Texture2D focusedBackground = ColourTexture(62, 125, 231);
	private static readonly Texture2D unFocusedBackround = ColourTexture(143, 143, 143);

	public static readonly GUIStyle Foldout = FoldoutStyle();
	public static readonly GUIStyle LabelFocus = LabelTextureStyle(focusedBackground);
	public static readonly GUIStyle LabelNoFocus = LabelTextureStyle(unFocusedBackround);

	private static Texture2D ColourTexture(byte r = 255, byte g = 255, byte b = 255, byte a = 255) {
		var tex = new Texture2D(1, 1);
		tex.SetPixel(0, 0, new Color32(r, g, b, a));
		tex.Apply();
		return tex;
	}

	private static GUIStyle FoldoutStyle() {
		var myFoldoutStyle = new GUIStyle(EditorStyles.foldout) {
			focused = {textColor = Color.white},
			onFocused = {textColor = Color.white},
			active = {textColor = Color.white},
			onActive = {textColor = Color.white},
			padding = new RectOffset(14, 0, 2, 2)
		};
		myFoldoutStyle.focused.background = myFoldoutStyle.normal.background;
		myFoldoutStyle.onFocused.background = myFoldoutStyle.onNormal.background;
		myFoldoutStyle.active.background = myFoldoutStyle.normal.background;
		myFoldoutStyle.onActive.background = myFoldoutStyle.onNormal.background;
		return myFoldoutStyle;
	}

	private static GUIStyle LabelTextureStyle(Texture2D background) {
		var style = new GUIStyle {
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
		return style;
	}
}
