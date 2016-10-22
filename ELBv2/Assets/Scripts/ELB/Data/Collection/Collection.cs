using System.Collections.Generic;
using ELB.Data.Helpers;
using UnityEngine;
using ELB.Utils;
using System.Linq;
using SQLite4Unity3d;
using System;

namespace ELB.Data.Collections {
	public class Collection<Model> : List<Model>, iFancyString where Model : Models.Model, new() {

		// Static Variables

		protected static SQLiteConnection _conn = new SQLiteConnection(Conf.dbPath, SQLiteOpenFlags.ReadOnly);

		protected static Cache<string, Model> _cache = new Cache<string, Model>();

		public Collection() {
		}

		public Collection(string dbString) {
			initialise(dbString);
		}

		private bool initialise(string dbString) {
			return initialise(
				dbString.Split('\n')
			);
		}

		private bool initialise(string[] ids) {
			if (ids.Length == 0) {
				return true;
			}
			List<string> _ids = ids.ToList();
			List<Model> models = _cache.get(_ids);
			// did we hit all of them?
			if (_ids.Count != models.Count) {
				var diff = _ids.Except(models.Select(x => x._Id));
				if(diff.Count() > 0) {
					var m = _conn.Table<Model>().Where(x => diff.Contains(x._Id));
					foreach(Model mo in m) {
						_cache.set(mo._Id, mo);
					}
					models.AddRange(m);
				}
			}
			if (models.Count != _ids.Count) {
				return false;
			}
			Clear();
			AddRange(models.ToList());
			return true;
		}

		public bool Fetch() {
			return initialise(this.Select(x => x._Id).ToArray());
		}


		public bool Fetch(string dbString) {
			return initialise(dbString);
		}

		public bool Fetch(string[] ids) {
			return initialise(ids);
		}

		public bool FetchAll() {
			// until unity gets c# 6 support we will be using sqlite4unity3d.
			// afterwards we can switch to a better library such as sqlite-net

			//var cache = _cache.get();
			//var cacheId = cache.Select(x => x._Id);
			//var m = _conn.Table<Model>().Where(x => !cacheId.Contains(x._Id));
			//foreach (Model mo in m) {
			//	_cache.set(mo._Id, mo);
			//}
			Clear();
			_cache.clear();
			var m = _conn.Table<Model>();
			foreach (Model mo in m) {
				_cache.set(mo._Id, mo);
			}
			AddRange(m);
			return true;
		}

		public bool Save() {
			return false;
		}

		public override string ToString() {
			return ToString(StringOpts.TwoLine);
		}

		delegate string FormatDelegate(StringOpts opts);

		public string ToString(StringOpts opts, int tabIndex = 0) {
			if (tabIndex > 10) {
				return "";
			}
			int count = Count;
			string className = string.Format("{0}.Collection<{1}> [{2}]", GetType().Namespace, typeof(Model), count);

			var specialChars = new Dictionary<char, char>() {
				{ '{', '}' },
				{ '[', ']' }
			};

			FormatDelegate f = delegate (StringOpts o) {
				if (count == 0) {
					return "[]";
				}
				bool isContainer = false;
				string s = string.Format("[{0}{1}]", string.Join(", ", this.Select(x => {
					string innerS = "";
					if(x == null) {
						innerS = "null";
					} else if(typeof(iFancyString).IsAssignableFrom(x.GetType())) {
						innerS = ((iFancyString)x).ToString(o, tabIndex);
					} else {
						innerS = x.ToString();
					}

					char sFirst = innerS.First();
					char last;
					if (specialChars.ContainsKey(sFirst)
						&& specialChars.TryGetValue(sFirst, out last) 
						&& last == innerS.Last()
					) {
						isContainer = true;
					} else if (o == StringOpts.Pretty) {
						innerS = "\n" + new string('\t', tabIndex + 1) + innerS;
					}
					return innerS;
				}).ToArray()), !isContainer && o == StringOpts.Pretty ? "\n" + new string('\t', tabIndex) : "");
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
