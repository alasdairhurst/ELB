using System;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

namespace BattleKit.Engine {

	public static class ModelHelpers {

	}

	[Serializable]
	public abstract class Model : ScriptableObject, iSerializable {

		public string id;

		public override string ToString( ) {
			return ToString(false);
		}

		public string ToString(bool pretty) {
			string s = string.Format("({0}) ", GetType().FullName);
			if(name != "") {
				s += string.Format("\"{0}\": ", name);
			}
			return s += Serialize().ToString();
		}

		public Model() {
			id = Guid.NewGuid().ToString().ToUpper();
		}

		public virtual void Init(JToken data) {
			id = data.Value<string>("id");
		}

		public virtual JToken Serialize() {
			return new JObject(
				new JProperty("id", id)
			);
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
