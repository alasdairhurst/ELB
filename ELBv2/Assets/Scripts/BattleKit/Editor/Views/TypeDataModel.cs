using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BattleKit.Editor {

	interface iDataModel {
		IList GetRows();
		IList<MultiColumnHeaderState.Column> GetColumns();
		MultiColumnHeaderState CreateMultiColumnHeaderState();
	}

	class TypeDataModel<T> : iDataModel where T : ScriptableObject {
		private Type type;
		private List<T> rows;
		private List<MultiColumnHeaderState.Column> columns;

		private List<T> Rows {
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

		public TypeDataModel() {
		
		}

		public IList GetRows() {
			return Rows;
		}

		public IList<MultiColumnHeaderState.Column> GetColumns() {
			return Columns;
		}

		private void buildRows() {
			// collect the instances of scriptableobject
			if (rows == null) {
				rows = new List<T>();
			} else {
				rows.Clear();
			}
			var res = Resources.LoadAll<T>("").OrderBy(item => item.name).Select(item => item as T).ToList();
			rows.AddRange(res);
		}

		private void buildColumns() {
			// figure out all the headers
			if (columns == null) {
				columns = new List<MultiColumnHeaderState.Column>();
			} else {
				columns.Clear();
			}

			var objectForType = ScriptableObject.CreateInstance<T>();
			var selection = new SerializedObject(objectForType);
			UnityEngine.Object.DestroyImmediate(objectForType);
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
						maxWidth = 150,
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
