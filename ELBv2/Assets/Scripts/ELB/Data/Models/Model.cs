using SQLite4Unity3d;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ELB.Utils;
using System.Linq;

namespace ELB.Data.Models {

	public abstract class ModelBase : iFancyString {

		// Static Variables

		// SHOULD BE IN CONFIG
		private static string dbRelPath = "/StreamingAssets/db.s3db";
#if UNITY_EDITOR
		private static string dbPath = @"Assets/" + dbRelPath;
#else
		private static string dbPath = Application.dataPath + dbRelPath;
#endif
		protected static SQLiteConnection _conn = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly);

		// Constructors

		public ModelBase() {
			Debug.LogWarning("TODO: Load static DB strings from config instead");
			Initialise();
		}

		// Initialisers

		protected virtual void Initialise() {
			_Id = -1;
		}

		// Props
		public int _Id { get; set; }

		// Methods

		public bool Fetch() {
			return Fetch(_Id);
		}

		public abstract bool Fetch(int id);

		public abstract bool Save();

		public abstract string ToString(StringOpts opts, int tabIndex = 0);
	}


	public abstract class Model<Schema> : ModelBase where Schema : Schemas.Schema, new() {

		// Constructors

		public Model() : base() {}

		public Model(Schema schema) {
			Initialise(schema);
		}
		public Model(Model<Schema> model) {
			Initialise(model);
		}

		// Initialisers

		protected virtual void Initialise(Schema schema) {
			_Id = schema._id;
		}

		protected void Initialise(Model<Schema> model) {
			PropertyInfo[] properties = model.GetType().GetProperties();
			
			foreach (PropertyInfo pi in properties) {
				if (pi.CanWrite) {
					pi.SetValue(this, pi.GetValue(model, null), null);
				}
			}
		}

		// Methods

		public override string ToString() {
			return ToString(StringOpts.TwoLine);
		}

		delegate string FormatDelegate(StringOpts opts);

		public override string ToString(StringOpts opts, int tabIndex = 0) {
			if (tabIndex > 10) {
				return "";
			}
			//string pretty = string.Format("{0}\n", data);
			string currentTabLevel = new string('\t', tabIndex);
			string nextTabLevel = new string('\t', tabIndex + 1);

			PropertyInfo[] properties = GetType().GetProperties();
			List<PropertyInfo> pList = new List<PropertyInfo>(properties);
			pList.Sort((x, y) => {
				return string.Compare(x.Name, y.Name);
			});

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
					}
					else if(value == null) {
						value = "null";
					}
					else if (typeof(iFancyString).IsAssignableFrom(value.GetType())) {
						StringOpts opt = o == StringOpts.Pretty ? StringOpts.Pretty : StringOpts.OneLine;
						value = ((iFancyString)value).ToString(opt, tabIndex + 1);
					}
					else {
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

		public override bool Fetch(int id) {
			if (id == -1) {
				return false;
			}
			var sQuery = _conn.Table<Schema>().Where(x => x._id == id);
			var s = sQuery.FirstOrDefault();
			if (s == null) {
				return false;
			}
			Initialise(s);
			return true;
		}

		public override bool Save() {
			return false;
		}
	}
}
