using System;
using Engine.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BattleKit.Editor {
	public delegate void ModelPickerFn(Model model);
	class ModelPicker : EditorWindow {

		private Model _selection;
		private Model[] _models;
		private Vector2 _scrollPos;
		private ModelPickerFn _cb;
		private Action _close;

		public static void ShowWizard<T>(T selection, ModelPickerFn cb, Action close) where T : Model, new() {
			Type collectionType = typeof(Collection<>).MakeGenericType(selection.GetType());
			var models = Activator.CreateInstance(collectionType);
			collectionType.GetMethod("FetchAll").Invoke(models, null);
			var items = collectionType.GetMethod("ToArray").Invoke(models, null) as Model[];
			var window = GetWindow<ModelPicker>();
			window.Load(selection, items, cb, close);
			window.Show();
		}

		void Load(Model selection, Model[] items, ModelPickerFn cb, Action close) {
			_selection = selection;
			_cb = cb;
			_close = close;
			_models = items;
		}

		public static void ShowWizard(Type t, ModelPickerFn cb, Action close) {
			ShowWizard(Activator.CreateInstance(t) as Model, cb, close);
		}

		void OnGUI( ) {
		}

		private void OnDestroy( ) {
			_close();
		}

		void OnLostFocus( ) {
			Close();
		}
	}
}
