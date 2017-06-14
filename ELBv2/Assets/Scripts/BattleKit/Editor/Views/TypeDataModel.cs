using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BattleKit.Editor {

	interface iDataModel {
		IList<SerializedObject> GetRows();
		IList<DataModelColumn> GetColumns();
		MultiColumnHeaderState CreateMultiColumnHeaderState();
	}

	class DataModelColumn : MultiColumnHeaderState.Column {
		public string propertyName;
	}

	class TypeDataModel<T> : iDataModel where T : ScriptableObject {
		private Type type;
		private List<SerializedObject> rows;
		private List<DataModelColumn> columns;

		~TypeDataModel() {
			rows.ForEach(row => {
				row.Dispose();
			});
		}

		private List<SerializedObject> Rows {
			get {
				if (rows == null) {
					buildRows();
				}
				return rows;
			}
		}

		private List<DataModelColumn> Columns {
			get {
				if (columns == null) {
					buildColumns();
				}
				return columns;
			}
		}

		public TypeDataModel() {
		
		}

		public IList<SerializedObject> GetRows() {
			return Rows;
		}

		public IList<DataModelColumn> GetColumns() {
			return Columns;
		}

		private void buildRows() {
			// collect the instances of scriptableobject
			if (rows == null) {
				rows = new List<SerializedObject>();
			} else {
				rows.Clear();
			}
			var res = Resources.LoadAll<T>("").OrderBy(item => item.name).Select(item => new SerializedObject(item)).ToList();
			rows.AddRange(res);
		}

		private void buildColumns() {
			// figure out all the headers
			if (columns == null) {
				columns = new List<DataModelColumn>();
			} else {
				columns.Clear();
			}

			// add dummy column
			columns.Add(
				new DataModelColumn {
					propertyName = "",
					headerContent = new GUIContent(""),
					width = 10,
					maxWidth = 10,
					minWidth = 10,
					canSort = false,
					autoResize = false,
					allowToggleVisibility = false
				}
			);

			columns.Add(
				new DataModelColumn {
					propertyName = "name",
					headerContent = new GUIContent("Asset Name"),
					contextMenuText = "Asset Name",
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					sortingArrowAlignment = TextAlignment.Right,
					width = 90,
					maxWidth = 150,
					autoResize = true,
					allowToggleVisibility = false
				}
			);

			var objectForType = ScriptableObject.CreateInstance<T>();
			var selection = new SerializedObject(objectForType);
			UnityEngine.Object.DestroyImmediate(objectForType);
			var prop = selection.GetIterator();
			prop.NextVisible(true);
			while (prop.NextVisible(false)) {
				columns.Add(
					new DataModelColumn {
						headerContent = new GUIContent(prop.displayName, prop.tooltip),
						contextMenuText = prop.displayName,
						headerTextAlignment = TextAlignment.Left,
						sortedAscending = true,
						sortingArrowAlignment = TextAlignment.Right,
						width = 70,
						autoResize = true,
						allowToggleVisibility = true
					}
				);
			}

		}

		public MultiColumnHeaderState CreateMultiColumnHeaderState() {
			return new MultiColumnHeaderState(Columns.ToArray());
		}
	}
}
