using System.Collections.Generic;
using System.Linq;
using Engine.String;

namespace Engine.Data {
	public class Collection<Model> : List<Model>, iFancyString where Model : Data.Model, new() {

		public Collection() {
		}

		public Collection(string dbString) {
			Fetch(dbString);
		}

		public string DBString() {
			return string.Join("\n", this.Select(x => { return x._Id; }).ToArray());
		}

		public bool Fetch(IEnumerable<string> ids) {
			List<Model> models = GameState.Fetch<Model>(ids);
			if (models.Count != ids.Count()) {
				return false;
			}
			Clear();
			AddRange(models);
			return true;
		}

		// Fetch all models with the ID currently loaded from the cache or database
		public bool Fetch() {
			return Fetch(this.Select(x => x._Id));
		}

		public bool Fetch(string dbString) {
			return Fetch(
				dbString.Split('\n')
			);
		}

		// Fetch all models of this type from the cache or database
		public bool FetchAll() {
			Clear();
			AddRange(GameState.FetchAll<Model>());
			return true;
		}

		public void SaveTemp() {
			foreach (Model m in this) {
				GameState.Update(m);
			}
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
