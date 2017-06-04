using System;

namespace BattleKit {
	public static class Utils {
		public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
			while(toCheck != null && toCheck != typeof(object)) {
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if(generic == cur && cur != toCheck) {
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}
		public static object call(this object o, string methodName, params object[] args) {
			var mi = o.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			if(mi != null) {
				return mi.Invoke(o, args);
			}
			return null;
		}
		public static object call(Type t, string methodName, params object[] args) {
			var mi = t.GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			if(mi != null) {
				return mi.Invoke(null, args);
			}
			return null;
		}
		public static object get(this object o, string propertyName) {
			var pi = o.GetType().GetProperty(propertyName);
			if(pi != null) {
				return pi.GetValue(o, null);
			}
			return null;
		}

		public static object get(string type, string propertyName) {
			Type t = Type.GetType(type);
			var pi = t.GetProperty(propertyName);
			if(pi != null) {
				return pi.GetValue(null, null);
			}
			return null;
		}
	}
}
