using BattleKit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BattleKit.Editor {

	class TypeTreeViewItem : TreeViewItem {
		public Type type;
	}

	class TypeTreeView<T> : TreeView {

		public delegate void selectionDelegate(Type selection);
		public selectionDelegate OnSelectionChanged;

		private int i_FirstItem;

		public TypeTreeView(TreeViewState treeViewState)
			: base(treeViewState) {
			Reload();
			if (!HasSelection()) {
				SetSelection(new List<int>{ i_FirstItem });
			}
		}

		private void BuildListRecursive(Type t, ref ILookup<Type, Type> lookup, ref List<TreeViewItem> list, int depth, ref int id) {
			foreach (var child in lookup[t]) {
				list.Add(new TypeTreeViewItem { id = child.AssemblyQualifiedName.GetHashCode(), depth = depth, displayName = child.Name, type = child });
				if (lookup[child].Any()) {
					BuildListRecursive(child, ref lookup, ref list, depth + 1, ref id);
				}
			}
		}

		public void SetSelection(Type type) {
			var id = type.AssemblyQualifiedName.GetHashCode();
			SetSelection(new List<int> { type.AssemblyQualifiedName.GetHashCode() });
			EnsureExpanded(id);
		}

		private void EnsureExpanded(int id) {
			if (IsExpanded(id)) {
				return;
			}
			var parent = GetRows().First(r => r.id == id).parent;
			EnsureExpanded(parent.id);
			SetExpanded(new List<int> { id });
		}

		private Type GetSelectedType(IList<int> selectedIds) {
			return (GetRows().First(x => x.id == selectedIds[0]) as TypeTreeViewItem).type;
		}

		public Type GetSelectedType() {
			return GetSelectedType(GetSelection());
		}

		protected override bool CanMultiSelect(TreeViewItem item) {
			return false;
		}

		protected override void SelectionChanged(IList<int> selectedIds) {
			base.SelectionChanged(selectedIds);
			if (OnSelectionChanged != null) {
				var type = GetSelectedType(selectedIds);
				OnSelectionChanged(type);
			}
		}

		protected override TreeViewItem BuildRoot() {
			// BuildRoot is called every time Reload is called to ensure that TreeViewItems 
			// are created from data. 

			var list = new List<TreeViewItem>();
			var id = 1;

			var types = typeof(T).Assembly.GetTypes()
					.Where(type => type.IsSubclassOf(typeof(T)))
					.OrderBy(type => type.Name)
					.ToLookup(model => model.BaseType, model => model);

			var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
			BuildListRecursive(typeof(T), ref types, ref list, 0, ref id);
			if (list.Count > 0) {
				i_FirstItem = list[0].id;
			}
			// Utility method that initializes the TreeViewItem.children and .parent for all items.
			SetupParentsAndChildrenFromDepths(root, list);

			// Return root of the tree
			return root;
		}
	}
}
