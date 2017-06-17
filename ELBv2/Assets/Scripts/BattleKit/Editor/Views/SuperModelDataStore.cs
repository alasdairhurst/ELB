using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BattleKit.Editor {
	[Serializable]
	class SuperModelDataStore {
		[SerializeField]
		List<MultiColumnTypeHeaderState> m_HeaderStates;
		TypeDataModel m_DataModel;

		public SuperModelDataStore() {
			if (m_HeaderStates == null) {
				m_HeaderStates = new List<MultiColumnTypeHeaderState>();
			}
		}

		public MultiColumnHeaderState GetHeader(Type t) {
			var index = m_HeaderStates.FindIndex(h => h.GetHeaderType() == t);
			var headerState = GetData(t).CreateMultiColumnHeaderState();
			if (index == -1) {
				m_HeaderStates.Add(headerState);
				return headerState;
			}

			if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_HeaderStates[index], headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(m_HeaderStates[index], headerState);
			m_HeaderStates[index] = headerState;
			return m_HeaderStates[index];
		}

		public TypeDataModel GetData(Type t) {
			if (m_DataModel == null || m_DataModel.GetDataType() != t) {
				ScriptableObject.CreateInstance(t);
				m_DataModel = new TypeDataModel(t);
			}
			return m_DataModel;
		}
	}
}
