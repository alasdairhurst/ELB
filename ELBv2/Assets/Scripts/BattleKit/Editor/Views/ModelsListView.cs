using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BattleKit.Editor {

	class ObjectTreeViewItem : TreeViewItem {
		public ScriptableObject reference;
	}

	class ModelsListView : TreeView {

		private iDataModel m_dataModel;

		public ModelsListView(TreeViewState treeViewState, MultiColumnHeaderState headerState, iDataModel dataModel)
			: base(treeViewState, new MultiColumnHeader(headerState)) {

			showAlternatingRowBackgrounds = true;

			LoadData(dataModel);

			Reload();
		} 

		public void LoadData(iDataModel dataModel) {
			m_dataModel = dataModel;
		}


		protected override TreeViewItem BuildRoot() {
			// BuildRoot is called every time Reload is called to ensure that TreeViewItems 
			// are created from data. 

			var list = new List<TreeViewItem>();
			var rows = m_dataModel.GetRows();
			foreach (ScriptableObject row in rows) {
				list.Add(
					new ObjectTreeViewItem { id = row.GetInstanceID(), depth = 0, displayName = row.name, reference = row }
				);
			}

			var root = new ObjectTreeViewItem { id = 0, depth = -1, displayName = "Root" };

			// Utility method that initializes the TreeViewItem.children and .parent for all items.
			SetupParentsAndChildrenFromDepths(root, list);

			// Return root of the tree
			return root;
		}

		protected override void RowGUI(RowGUIArgs args) {
			var item = args.item;

			for (int i = 0; i < args.GetNumVisibleColumns(); ++i) {
				CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
			}
		}

		void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args) {
			// Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
			CenterRectUsingSingleLineHeight(ref cellRect);

			switch (column) {
				case 0: {
						// Default icon and label
						args.rowRect = cellRect;
						base.RowGUI(args);
					}
					break;
				default:
					base.RowGUI(args);
					DefaultGUI.Label(cellRect, item.displayName, args.selected, args.focused);
					break;
			}
		}

		public static MultiColumnHeaderState CreateMultiColumnHeaderState() {
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Asset Name", "Name of the asset"),
					contextMenuText = "Asset",
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Right,
					width = 50,
					maxWidth = 100,
					autoResize = false,
					allowToggleVisibility = false
				},
			};

			var state = new MultiColumnHeaderState(columns);
			return state;
		}

	}
}
