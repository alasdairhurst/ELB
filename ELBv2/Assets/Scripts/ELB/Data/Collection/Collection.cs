using System.Collections.Generic;
using ELB.Data.Helpers;
using UnityEngine;
using ELB.Utils;
using System.Linq;

namespace ELB.Data.Collections {
	public class Collection<Model> : List<Model>, iFancyString where Model : Models.ModelBase, new() {

		private int[] ids;

		public Collection(string dbString) {
			Debug.LogWarning("TODO: Cache collection statically and load on fetch or new.");
			initialise(dbString);
		}

		private bool initialise(string dbString) {
			ids = DataHelper.ToIntArray(dbString);
			bool success = true;
			for(int i = 0; i < ids.Length; ++i) {
				Model m = new Model();
				Debug.LogWarning("TODO: Fetch all IDs at once instead");
				if(!m.Fetch(ids[i])) {
					success = false;
				}
				Add(m);
			}
			return success;
		}

		public bool Fetch() {
			return false;
		}

		public bool Fetch(string dbString) {
			return initialise(dbString);
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
