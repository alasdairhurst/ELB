using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BattleKit.Editor {

	[Serializable]
	class MultiColumnTypeHeaderState : MultiColumnHeaderState {
		[SerializeField]
		string t_Type;
		public MultiColumnTypeHeaderState(Column[] columns, Type t) : base(columns) {
			t_Type = t.AssemblyQualifiedName;
		}

		public Type GetHeaderType() {
			return Type.GetType(t_Type);
		}
	}

	class TypeDataModel {
		private ScriptableObject instance;
		private List<SerializedObject> rows;
		private List<MultiColumnHeaderState.Column> columns;

		private List<SerializedObject> Rows {
			get {
				if (rows == null) {
					buildRows();
				}
				return rows;
			}
		}

		private List<MultiColumnHeaderState.Column> Columns {
			get {
				if (columns == null) {
					buildColumns();
				}
				return columns;
			}
		}

		public TypeDataModel(Type t) {
			instance = ScriptableObject.CreateInstance(t);
		}

		public TypeDataModel(string t) {
			instance = ScriptableObject.CreateInstance(t);
		}

		~TypeDataModel() {
			rows.ForEach(row => {
				row.Dispose();
			});
			// UnityEngine.Object.DestroyImmediate(instance);
		}

		public Type GetDataType() {
			return instance.GetType();
		}

		public IList<SerializedObject> GetRows() {
			return Rows;
		}

		public IList<MultiColumnHeaderState.Column> GetColumns() {
			return Columns;
		}

		public SerializedObject GetRowByID(int id) {
			return GetRows().First(row => row.targetObject.GetInstanceID() == id);
		}

		private void buildRows() {
			// collect the instances of scriptableobject
			if (rows == null) {
				rows = new List<SerializedObject>();
			} else {
				rows.Clear();
			}
			var res = Resources.LoadAll("", instance.GetType())
				.Where(item => item.GetType() == instance.GetType()) // don't get subclassese
				.OrderBy(item => item.name)
				.Select(item => new SerializedObject(item))
				.ToList();
			rows.AddRange(res);
		}

		private void buildColumns() {
			// figure out all the headers
			if (columns == null) {
				columns = new List<MultiColumnHeaderState.Column>();
			} else {
				columns.Clear();
			}

			// add dummy column
			columns.Add(
				new MultiColumnHeaderState.Column {
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
				new MultiColumnHeaderState.Column {
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

			var selection = new SerializedObject(instance);
			var prop = selection.GetIterator();
			prop.NextVisible(true);
			while (prop.NextVisible(false)) {
				columns.Add(
					new MultiColumnHeaderState.Column {
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

		public MultiColumnTypeHeaderState CreateMultiColumnHeaderState() {
			return new MultiColumnTypeHeaderState(Columns.ToArray(), instance.GetType());
		}
	}
}
