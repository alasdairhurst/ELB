using System.Collections.Generic;
using ELB.Data.Helpers;
using UnityEngine;
using ELB.Utils;
using System.Linq;
using SQLite4Unity3d;

namespace ELB.Data.Collections {
	public class Collection<Model> : List<Model>, iFancyString where Model : Models.Model, new() {

		// Static Variables

		// SHOULD BE IN CONFIG
		private static string dbRelPath = "/StreamingAssets/db.s3db";
#if UNITY_EDITOR
		private static string dbPath = @"Assets/" + dbRelPath;
#else
		private static string dbPath = Application.dataPath + dbRelPath;
#endif
		protected static SQLiteConnection _conn = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly);

		public Collection(string dbString) {
			Debug.LogWarning("TODO: Cache collection statically and load on fetch or new.");
			initialise(dbString);
		}

		private bool initialise(string dbString) {
			return initialise(DataHelper.ToIntArray(dbString));
		}

		private bool initialise(int[] ids) {
			List<int> _ids = ids.ToList();
			var m = _conn.Table<Model>().Where(x => _ids.Contains(x._Id));
			if (m == null || m.Count() == 0) {
				Clear();
				return false;
			}
			AddRange(m.ToList());
			return true;
		}

		public bool Fetch() {
			return false;
		}

		public bool Fetch(string dbString) {
			return initialise(dbString);
		}

		public bool Fetch(int[] ids) {

			// fetch then

			return initialise(ids);
		}

		public bool FetchAll() {
			return false;
		}

		public bool Save() {
			return false;
		}

		delegate string FormatDelegate(StringOpts opts);

		public override string ToString() {
			return ToString(StringOpts.TwoLine);
		}

		public string ToString(StringOpts opts, int tabIndex = 0) {
			if (tabIndex > 10) {
				return "";
			}
			int count = Count;
			string className = string.Format("{0}.Collection<{1}> [{2}]", GetType().Namespace, typeof(Model), count);

			FormatDelegate f = delegate (StringOpts o) {
				string s = string.Format("[{0}]", string.Join(", ", this.Select(x => x.ToString(o, tabIndex)).ToArray()));
				if (s.Length > 100 && o == StringOpts.Short) {
					s = s.Substring(0, 100).Replace("\n", "").Replace("\t", " ") + "...";
				}
				return s;
			};

			switch(opts) {
				case StringOpts.OneLine:
					return f(StringOpts.OneLine);
				case StringOpts.Pretty:
					return f(StringOpts.Pretty);
				case StringOpts.TwoLine:
					return string.Format(
					"{0}\n{1}",
					className, f(StringOpts.Short)
				);
				case StringOpts.Full:
					return string.Format(
					"{0}\n{1}\n{2}",
					className, f(StringOpts.Short), f(StringOpts.Pretty)
				);
				case StringOpts.Short:
					return f(StringOpts.Short);
				default:
					return "";
			}
		}
	}
}
