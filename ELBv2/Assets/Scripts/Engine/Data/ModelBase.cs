using SQLite4Unity3d;
using System.Linq;

namespace BattleKit.Engine {
	public abstract class ModelBase : iFancyString {
		[PrimaryKey, Unique]
		public string _Id { get; set; }
		[Unique]
		public string _EditorId { get; set; }

		public override string ToString() {
			return ToString(StringOpts.TwoLine);
		}

		delegate string FormatDelegate(StringOpts opts);

		public string ToString(StringOpts opts, int tabIndex = 0) {

			// if the stringifying is just IDs then don't bother with the rest of the stuff
			if (opts == StringOpts.Identifier) {
				return string.Format("{0} ({1})", _EditorId, _Id);
			}

			if (tabIndex > 10) {
				return "";
			}
			//string pretty = string.Format("{0}\n", data);
			string currentTabLevel = new string('\t', tabIndex);
			string nextTabLevel = new string('\t', tabIndex + 1);
			var pList = GetType().GetProperties().Where(x => {
				// ignore ids
				return !x.Name.EndsWith("_ids");
			}).OrderBy(x => x.Name);

			FormatDelegate f = delegate (StringOpts o) {
				string start = "";
				string join = "";
				string end = "";
				if (o == StringOpts.Pretty) {
					start = string.Format("{{\n{0}", nextTabLevel);
					join = string.Format(",\n{0}", nextTabLevel);
					end = string.Format("\n{0}}}", currentTabLevel);
				} else {
					start = "{ ";
					join = ", ";
					end = " }";
				}
				string format = "{0}{1}{2}";

				string s = string.Format(format, start, string.Join(join, pList.Select(x => {
					var value = x.GetValue(this, null);
					if (x.PropertyType.Name == "String") {
						value = string.Format("\"{0}\"", x.GetValue(this, null));
					} else if (value == null) {
						value = "null";
					} else if (typeof(iFancyString).IsAssignableFrom(value.GetType())) {
						StringOpts opt = o == StringOpts.Pretty ? StringOpts.Pretty : StringOpts.OneLine;
						value = ((iFancyString)value).ToString(opt, tabIndex + 1);
					} else {
						value = value.ToString();
					}
					return string.Format("{0}: {1}", x.Name, value);
				}).ToArray()), end);

				if (s.Length > 100 && o == StringOpts.Short) {
					s = s.Substring(0, 100) + "...";
				}
				return s;
			};

			switch (opts) {
				case StringOpts.OneLine:
					return f(StringOpts.OneLine);
				case StringOpts.Pretty:
					return f(StringOpts.Pretty);
				case StringOpts.TwoLine:
					return string.Format(
					"{0}\n{1}",
					GetType(), f(StringOpts.Short)
				);
				case StringOpts.Full:
					return string.Format(
					"{0}\n{1}\n{2}",
					GetType(), f(StringOpts.Short), f(StringOpts.Pretty)
				);
				case StringOpts.Short:
					return f(StringOpts.Short);
				default:
					return "";
			}
		}
	}
}
