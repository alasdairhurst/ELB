using BattleKit.Engine;
using ELB.Models;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BattleKit.Editor {

	public sealed class NewModelBrowser : EditorWindow {
		// SerializeField is used to ensure the view state is written to the window 
		// layout file. This means that the state survives restarting Unity as long as the window
		// is not closed. If the attribute is omitted then the state is still serialized/deserialized.
		//[SerializeField]
		TreeViewState m_TreeViewState;
		//[SerializeField]
		TreeViewState m_ListViewState;
		//[SerializeField]
		MultiColumnHeaderState m_ListViewHeaderState;
		[SerializeField]
		float f_ModelTreeViewWidth = 100;
		float ModelTreeWidth {
			get { return f_ModelTreeViewWidth; }
			set {
				if (value >= f_MinPanelWidth && value <= position.width - f_MinPanelWidth) {
					f_ModelTreeViewWidth = value;
				}
			}
		}

		//The TreeView is not serializable, so it should be reconstructed from the tree data.
		TypeTreeView<Model> m_ModelTreeView;
		iDataModel m_TypeDataModel;
		ModelsListView m_ModelsListView;
		SearchField m_ModelSearchField;
		SearchField m_ListViewSearchField;
		Vector2 v_ModelScrollPos;
		Vector2 v_ListViewScrollPos;
		float f_MinPanelWidth = 100;

		static NewModelBrowser m_Instance;

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void OnScriptsReloaded() {
			// rebuild all the different states
			// try to get a hash of object definition and see if it changed and if it's worth regenerating headers
			if (m_Instance == null) {
				return;
			}
			m_Instance.init();
		}


		void init() {
			m_TypeDataModel = new TypeDataModel<Cell>();
			m_ListViewHeaderState = m_TypeDataModel.CreateMultiColumnHeaderState();
			m_ModelTreeView = new TypeTreeView<Model>(m_TreeViewState);
			m_ModelsListView = new ModelsListView(m_ListViewState, m_ListViewHeaderState, m_TypeDataModel);
		}

		void OnEnable() {

			m_Instance = this;

			m_ModelSearchField = new SearchField();
			m_ListViewSearchField = new SearchField();

			// Check whether there is already a serialized view state (state 
			// that survived assembly reloading
			//if (m_TreeViewState == null) {
			m_TreeViewState = new TreeViewState();
			//}
			//if (m_ListViewState == null) {
				m_ListViewState = new TreeViewState();
			//	} 

			//if (m_ListViewHeaderState == null) {
			//m_ListViewHeaderState = m_TypeDataModel.CreateMultiColumnHeaderState();
			//}

			init();
		}

		void OnGUI() {
			GUILayout.BeginHorizontal();
			{
				RenderModelTreeView();
				ModelTreeWidth += Controls.ResizeControl(position.height, ModelTreeWidth);
				RenderModelsListView();
			}
			GUILayout.EndHorizontal();
		}

		void RenderModelTreeView() {
			v_ModelScrollPos = EditorGUILayout.BeginScrollView(v_ModelScrollPos, GUILayout.Width(f_ModelTreeViewWidth), GUILayout.MinWidth(f_MinPanelWidth));
			RenderModelSearch();
			RenderModelTree();
			EditorGUILayout.EndScrollView();
		}

		void RenderModelSearch() {
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			GUILayout.Space(10);
			GUILayout.FlexibleSpace();
			m_ModelTreeView.searchString = m_ModelSearchField.OnToolbarGUI(m_ModelTreeView.searchString);
			GUILayout.EndHorizontal();
		}


		void RenderModelsListSearch() {
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			GUILayout.Space(10);
			GUILayout.FlexibleSpace();
			m_ModelsListView.searchString = m_ListViewSearchField.OnToolbarGUI(m_ModelsListView.searchString);
			GUILayout.EndHorizontal();
		}

		void RenderModelTree() {
			Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
			m_ModelTreeView.OnGUI(rect);
		}

		void RenderModelsListView() {
			v_ListViewScrollPos = EditorGUILayout.BeginScrollView(v_ListViewScrollPos, GUILayout.Width(position.width - f_ModelTreeViewWidth), GUILayout.MinWidth(f_MinPanelWidth));
			RenderModelsListSearch();
			RenderModelsList();
			EditorGUILayout.EndScrollView();

		}

		void RenderModelsList() {
			Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
			m_ModelsListView.OnGUI(rect);
		}


		// Add menu named "My Window" to the Window menu
		[MenuItem("BattleKit/New Model Browser")]
		static void ShowWindow() {
			// Get existing open window or if none, make a new one:
			var window = GetWindow<NewModelBrowser>();
			window.titleContent = new GUIContent("Models (new)");
			window.Show();
		}

	}
}
