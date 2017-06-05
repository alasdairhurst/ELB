using System.Reflection;
using System;
using UnityEngine;
using System.Linq;

namespace BattleKit.Engine {

	[Serializable]
	public class Model : ScriptableObject {

		public override string ToString( ) {
			return JsonUtility.ToJson(this);
		}

		public string ToString(bool pretty) {
			return JsonUtility.ToJson(this, pretty);
		}
	}
}
