using System;
using Engine.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using BattleKit.Editor.Utils;

namespace BattleKit.Editor {
	class ModelPicker : EditorWindow {

		private Model _selection;
		private Model[] _models;
		private Vector2 _scrollPos;	 
		private SearchableItemList _list = new SearchableItemList();
		private EditorWindow m_DelegateView;

		public static ModelPicker instance;

		public static void ShowWizard<T>(T selection, EditorWindow caller) where T : Model, new() {
			if(instance) {
				instance.Show();
				return;
			}
			Type collectionType = typeof(Collection<>).MakeGenericType(selection.GetType());
			var models = Activator.CreateInstance(collectionType);
			collectionType.GetMethod("FetchAll").Invoke(models, null);
			var items = collectionType.GetMethod("ToArray").Invoke(models, null) as Model[];
			instance = GetWindow<ModelPicker>(true, selection.GetType().Name + " Selector");
			instance.Load(selection, items, caller);
			instance.Show();
		}

		public static void ShowWizard(Type t, EditorWindow caller) {
			if(instance) {
				instance.Show();
				return;
			}
			Type collectionType = typeof(Collection<>).MakeGenericType(t);
			var models = Activator.CreateInstance(collectionType);
			collectionType.GetMethod("FetchAll").Invoke(models, null);
			var items = collectionType.GetMethod("ToArray").Invoke(models, null) as Model[];
			instance = GetWindow<ModelPicker>(true, t.Name + " Selector");
			instance.Load(null, items, caller);
			instance.Show();
		}

		void Load(Model selection, Model[] items, EditorWindow caller) {
			_selection = selection;
			_models = items;	 
			m_DelegateView = caller;
			if (_selection != null) {
				_list.Preselect(_selection._EditorId);
			}
		}

		private void SendEvent(string message) {
			Event e = EditorGUIUtility.CommandEvent(message);
			try {
				m_DelegateView.SendEvent(e);
			} finally {}
		} 

		private void setSelection(Model m) {
			if (_selection != m) {
				_selection = m;
				SendEvent("ModelPickerUpdated");
			}
		}

		public Model GetSelection( ) {
			return _selection;
		}

		void OnGUI( ) {

			_list.Start();
			{
				if(_list.ListItem("None", false)) {
					setSelection(null);
				}
				for (int i = 0; i < _models.Length; ++i) {
					if(_list.ListItem(_models[i]._EditorId)) {
						setSelection(_models[i]);
					}
				}
			}
			_list.End();
		}

		private void OnDestroy( ) {
			SendEvent("ModelPickerClosed");
			instance = null;
		}

		void OnLostFocus( ) {
			Close();
		}
	}
}
