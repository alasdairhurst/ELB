using System;
using BattleKit.Engine;
using UnityEditor;
using UnityEngine;

namespace BattleKit.Editor {
	class ModelPicker : EditorWindow {

		private Model _selection;
		private Model[] _models;
		private Vector2 _scrollPos;	 
		private SearchableItemList _list = new SearchableItemList();
		private EditorWindow m_DelegateView;
		private bool closing = false;

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
				ListItem(null);
				for (int i = 0; i < _models.Length; ++i) {
					ListItem(_models[i]);
				}
			}
			_list.End();
		}

		private void ListItem(Model m, bool filter = true) {
			var title = m == null ? "None" : m._EditorId;
			var st = _list.ListItem(title, filter, true);
			switch(st) {
				case ItemList.SelectionType.Focus:
					setSelection(m);
					break;
				case ItemList.SelectionType.Select: {
					closeWindow();
					break;
				}
			}
		}

		private void OnDestroy( ) {
			// this is called by the window when you press esc as well as onLostFocus so we want to make sure close isn't called again 
			closing = true;
			SendEvent("ModelPickerClosed");
			instance = null;
		}

		void closeWindow() {
			if(!closing) {
				closing = true;
				Close();
				GUIUtility.ExitGUI();
			}
		}

		void OnLostFocus( ) {
			closeWindow();
		}
	}
}
