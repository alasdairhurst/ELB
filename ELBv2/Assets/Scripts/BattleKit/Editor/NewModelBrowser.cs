using BattleKit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BattleKit.Editor {

	class TypeTreeView<T> : TreeView {

		public TypeTreeView(TreeViewState treeViewState)
			: base(treeViewState) {
			Reload();
		}

		private void BuildListRecursive(Type t, ref ILookup<Type,Type> lookup, ref List<TreeViewItem> list, int depth, ref int id) {
			foreach (var child in lookup[t]) {
				list.Add(new TreeViewItem { id = id++, depth = depth, displayName = child.Name });
				if (lookup[child].Any()) {
					BuildListRecursive(child, ref lookup, ref list, depth + 1, ref id);
				}
			}
		}

		protected override TreeViewItem BuildRoot() {
			// BuildRoot is called every time Reload is called to ensure that TreeViewItems 
			// are created from data. 

			var list = new List<TreeViewItem>();
			var id = 1;

			var types = typeof(T).Assembly.GetTypes()
					.Where(type => type.IsSubclassOf(typeof(Model)))
					.ToLookup(model => model.BaseType, model => model);

			var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
			BuildListRecursive(typeof(T), ref types, ref list, 0, ref id);

			// Utility method that initializes the TreeViewItem.children and .parent for all items.
			SetupParentsAndChildrenFromDepths(root, list);

			// Return root of the tree
			return root;
		}
	}


	public sealed class NewModelBrowser : EditorWindow {
		// SerializeField is used to ensure the view state is written to the window 
		// layout file. This means that the state survives restarting Unity as long as the window
		// is not closed. If the attribute is omitted then the state is still serialized/deserialized.
		[SerializeField]
		TreeViewState m_TreeViewState;

		//The TreeView is not serializable, so it should be reconstructed from the tree data.
		TypeTreeView<Model> m_SimpleTreeView;

		void OnEnable() {
			// Check whether there is already a serialized view state (state 
			// that survived assembly reloading)
			if (m_TreeViewState == null)
				m_TreeViewState = new TreeViewState();

			m_SimpleTreeView = new TypeTreeView<Model>(m_TreeViewState);
		}

		void OnGUI() {
			m_SimpleTreeView.OnGUI(new Rect(0, 0, position.width, position.height));
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
