using System;
using UnityEngine;

namespace ELB.Data.Helpers {
	public class DataHelper {
		// Converts a comma seperated string stored in SQL into an array of strings
		public static char ARRAY_SEPARATOR = ',';
		public static string[] ToStringArray(string array) {
			string[] items = array.Split(',');
			return items;
		}

		// Converts a CSV string stored in SQL into an array of parsed integers
		public static int[] ToIntArray(string array) {
			Debug.LogWarning("TODO: Needs to handle error cases where parsing goes wrong - return []");
			string[] items = array.Split(ARRAY_SEPARATOR);
			int[] intItems = new int[items.Length];
			for (int i = 0; i < items.Length; ++i) {
				intItems[i] = Convert.ToInt16(items[i]);
			}
			return intItems;
		}

		// Converts an int array into CSV string for database use
		public static string ToDBString(int[] array) {
			string output = "";
			for (int i = 0; i < array.Length; ++i) {
				if (i != 0) {
					output += ARRAY_SEPARATOR;
				}
				output += array[i].ToString();
			}
			return output;
		}

		// Converts a string array into a CSV string for database use
		public static string ToDBString(string[] array) {
			string output = "";
			for (int i = 0; i < array.Length; ++i) {
				if (i != 0) {
					output += ARRAY_SEPARATOR;
				}
				output += array[i];
			}
			return output;
		}

		// Converts a model array into a CSV string for database use
		public static string ToDBString(Models.Model[] array) {
			string output = "";
			for (int i = 0; i < array.Length; ++i) {
				if (i != 0) {
					output += ARRAY_SEPARATOR;
				}
				output += array[i]._Id.ToString();
			}
			return output;
		}
	}
}
