using BattleKit.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BattleKit.Editor {

	class ObjectTreeViewItem : TreeViewItem {
		public SerializedObject reference;
	}

	class ModelsListView : TreeView {

		public delegate void selectionDelegate(ScriptableObject[] selection);
		public selectionDelegate OnSelectionChanged;

		private TypeDataModel m_dataModel;
		private SuperModelDataStore m_dataStore;

		public ModelsListView(TreeViewState treeViewState, SuperModelDataStore dataStore, Type initialType)
			: base(treeViewState) {

			m_dataStore = dataStore;
			extraSpaceBeforeIconAndLabel = 350;
			showAlternatingRowBackgrounds = true;

			SetListType(initialType);
		} 

		public void SetListType(Type type) {
			multiColumnHeader = new MultiColumnHeader(m_dataStore.GetHeader(type));
			LoadData(m_dataStore.GetData(type));
		}

		public void LoadData(TypeDataModel dataModel) {
			m_dataModel = dataModel;
			Reload();
		}

		protected override void SelectionChanged(IList<int> selectedIds) {
			base.SelectionChanged(selectedIds);
			if (OnSelectionChanged != null) {
				OnSelectionChanged(selectedIds.Select(id => m_dataModel.GetRowByID(id).targetObject as ScriptableObject).ToArray());
			}
		}

		protected override TreeViewItem BuildRoot() {
			// BuildRoot is called every time Reload is called to ensure that TreeViewItems 
			// are created from data. 

			var list = new List<TreeViewItem>();
			var rows = m_dataModel.GetRows();
			foreach (SerializedObject row in rows) {
				var item = row.targetObject as ScriptableObject;
				list.Add(
					new ObjectTreeViewItem { id = item.GetInstanceID(), depth = 0, displayName = item.name, reference = row }
				);
			}

			var root = new ObjectTreeViewItem { id = 0, depth = -1, displayName = "Root" };

			// Utility method that initializes the TreeViewItem.children and .parent for all items.
			SetupParentsAndChildrenFromDepths(root, list);

			// Return root of the tree
			return root;
		}

		protected override void RowGUI(RowGUIArgs args) {
			var item = args.item as ObjectTreeViewItem;
			var prop = item.reference.GetIterator();
			prop.NextVisible(true);
			var colCount = 0;
			CellGUI(args.GetCellRect(colCount), item, null, args.GetColumn(colCount++), ref args);
			CellGUI(args.GetCellRect(colCount), item, item.displayName, args.GetColumn(colCount++), ref args);
			while (prop.NextVisible(false)) {
				var val = Utils.GetTargetObjectOfProperty(prop);
				CellGUI(args.GetCellRect(colCount), item, val, args.GetColumn(colCount++), ref args);
			}
		}


		void CellGUI(Rect cellRect, TreeViewItem item, object val, int column, ref RowGUIArgs args) {
			// Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
			CenterRectUsingSingleLineHeight(ref cellRect);
			var col = multiColumnHeader.GetColumn(column) as MultiColumnHeaderState.Column;
			var str = getStringRepresentationOf(val);
			DefaultGUI.Label(cellRect, str, args.selected, args.focused);
		}

		private	string getStringRepresentationOf(object val) {
			if (val == null) {
				return string.Empty;
			}

			if (val.GetType() == typeof(string)) {
				return val as string;
			}

			if (val.GetType().IsSubclassOf(typeof(Model))) {
				var name = (val as Model).name;
				return string.IsNullOrEmpty(name) ? val.GetType().ToString() : name;
			}

			if (val.GetType().IsArray || typeof(IList).IsAssignableFrom(val.GetType())) {
				var type = GetTypeName(val.GetType());
				// if it's an array we just need to get rid of these
				type = type.TrimEnd('[', ']');
				return string.Format("{0}[{1}]", type, (val as IList).Count);
			}

			return val.ToString();

		}

		public static string GetTypeName(Type t) {
			if (!t.IsGenericType)
				return t.Name;
			if (t.IsNested && t.DeclaringType.IsGenericType)
				throw new NotImplementedException();
			string txt = t.Name.Substring(0, t.Name.IndexOf('`')) + "<";
			int cnt = 0;
			foreach (Type arg in t.GetGenericArguments()) {
				if (cnt > 0)
					txt += ", ";
				txt += GetTypeName(arg);
				cnt++;
			}
			return txt + ">";
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
