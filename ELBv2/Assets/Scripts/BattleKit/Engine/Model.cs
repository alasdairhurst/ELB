using System;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

namespace BattleKit.Engine {

	public static class ModelHelpers {
		public static T Clone<T>(this T model) where T : Model {
			var clone = ScriptableObject.Instantiate(model);
			if (!string.IsNullOrEmpty(model.name)) {
				clone.name = model.name + "(Clone)";
			}
			return clone;
		}
	}

	[Serializable]
	public abstract class Model : ScriptableObject {

		public static T Find<T>(string name) where T : Model {
			return (from resource in Resources.FindObjectsOfTypeAll<T>()
			where resource.name == name
			select resource).FirstOrDefault();
		}

		public override string ToString( ) {
			return ToString(false);
		}

		public string ToString(bool pretty) {
			string s = string.Format("({0}) ", GetType().FullName);
			if(name != "") {
				s += string.Format("\"{0}\": ", name);
			}
			return s += JsonUtility.ToJson(this, pretty);
		}

		[HideInInspector]
		public UnityEvent InspectorOnChange;

		public void OnValidate() {
			if (InspectorOnChange != null) {
				InspectorOnChange.Invoke();
			}
		}

	}
}
