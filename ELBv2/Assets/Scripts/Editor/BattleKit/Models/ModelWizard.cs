using Engine.Data;
using System;
using UnityEditor;
using UnityEngine;

public delegate void OnSubmit(Model outputModel); 

public class ModelWizard : EditorWindow {

	private static Model selection;

	private static void ShowWizard<T>(T instance, bool editMode) where T : Model {
		var title = editMode ? "Edit " : "New ";
		title += typeof(T).Name;

		selection = instance;

		GetWindow<ModelWizard>(true, title);
	}

	public static void ShowWizard<T>(T instance) where T : Model {
		ShowWizard(instance, true);
	}

	public static void ShowWizard(Type t) {
		ShowWizard(Activator.CreateInstance(t) as Model, false);
	}



	void OnWizardUpdate() {
		//helpString = "Please set the color of the light!";
	}

	// When the user pressed the "Apply" button OnWizardOtherButton is called.
	void OnWizardOtherButton() {
		if (Selection.activeTransform != null) {
			Light lt = Selection.activeTransform.GetComponent<Light>();

			if (lt != null) {
				lt.color = Color.red;
			}
		}
	}
}