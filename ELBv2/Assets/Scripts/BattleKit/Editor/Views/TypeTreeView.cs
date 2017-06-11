using BattleKit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.IMGUI.Controls;

namespace BattleKit.Editor {

	class TypeTreeView<T> : TreeView {

		public TypeTreeView(TreeViewState treeViewState)
			: base(treeViewState) {
			Reload();
		}

		private void BuildListRecursive(Type t, ref ILookup<Type, Type> lookup, ref List<TreeViewItem> list, int depth, ref int id) {
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
					.Where(type => type.IsSubclassOf(typeof(T)))
					.ToLookup(model => model.BaseType, model => model);

			var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
			BuildListRecursive(typeof(T), ref types, ref list, 0, ref id);

			// Utility method that initializes the TreeViewItem.children and .parent for all items.
			SetupParentsAndChildrenFromDepths(root, list);

			// Return root of the tree
			return root;
		}
	}
}
