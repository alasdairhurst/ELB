using System.Reflection;
using System;
using UnityEngine;
using System.Linq;

namespace BattleKit.Engine {

	[Serializable]
	public class Model : ScriptableObject {

		public static T Find<T>(string name) where T : Model {
			var objects = Resources.FindObjectsOfTypeAll(typeof(T));
			return objects.FirstOrDefault(x => x.name == name) as T;
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
	}
}
