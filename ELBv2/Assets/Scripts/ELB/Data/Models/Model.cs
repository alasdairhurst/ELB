using SQLite4Unity3d;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ELB.Utils;
using System.Linq;
using ELB.Data.Helpers;
using System;

namespace ELB.Data.Models {

	public abstract class Model : iFancyString {

		// Static Variables

		public static SQLiteConnection _conn = new SQLiteConnection(Conf.dbPath, SQLiteOpenFlags.ReadOnly);

		// cache of the data pulled from the main database
		public static Cache<string, Model> _dbCache = new Cache<string, Model>();
		// cache of the data stored by the current game
		public static Cache<string, Model> _gameCache = new Cache<string, Model>();

		// Constructors

		public Model() {
			_Id = Guid.NewGuid().ToString("B").ToUpper();
		}

		public Model(Model model) : base() {
			Debug.LogWarning("TODO: Load static DB strings from config instead");
			Init(model);
		}

		// Initialisers

		private void Init<T>(T model) where T : Model {
			PropertyInfo[] properties = model.GetType().GetProperties();

			foreach (PropertyInfo pi in properties) {
				if (pi.CanWrite) {
					pi.SetValue(this, pi.GetValue(model, null), null);
				}
			}
			Initialise(model);
		}

		protected virtual void Initialise<T>(T model) where T : Model { }

		// Props
		[PrimaryKey]
		public string _Id { get; set; }

		// Methods

		public override string ToString() {
			return ToString(StringOpts.TwoLine);
		}

		protected virtual string[] PropsToIgnore() {
			return null;
		}

		delegate string FormatDelegate(StringOpts opts);

		public string ToString(StringOpts opts, int tabIndex = 0) {
			if (tabIndex > 10) {
				return "";
			}
			//string pretty = string.Format("{0}\n", data);
			string currentTabLevel = new string('\t', tabIndex);
			string nextTabLevel = new string('\t', tabIndex + 1);
			string[] propsToIgnore = PropsToIgnore();
			var pList = GetType().GetProperties().Where(x => {
				if (propsToIgnore == null) {
					return true;
				}
				return !propsToIgnore.Contains(x.Name);
			}).OrderBy(x => x.Name);
			//.Sort((x, y) => {
			//	return string.Compare(x.Name, y.Name);
			//});

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

		// Fetch model contents from main database
		public bool Fetch<T>(string id) where T : Model, new() {
			var model = _dbCache.get<T>(id);
			if (model == null) {
				model = _conn.Find<T>(id);
			}
			if (model == null) {
				return false;
			}
			_dbCache.set(id, model);
			Init(model);
			return true;
		}

		// Fetch model contents from main database
		public abstract bool Fetch(string id);

		// Fetch model contents from main database
		public bool Fetch() {
			return Fetch(_Id);
		}

		// Save contents of model into temporary memory
		public void SaveTemp() {
			_gameCache.set(_Id, this);
		}

		public abstract bool LoadTemp(string id);

		// Load contents of model from temporary memory
		public bool LoadTemp<T>(string id) where T : Model, new() {
			var model = _gameCache.get<T>(id);
			if (model == null) {
				return false;
			}
			Init(model);
			return true;
		}

		public bool LoadTemp<T>() where T : Model, new() {
			return LoadTemp<T>();
		}

		// Override to save select model contents to a save file
		public abstract bool Save();
	}
}
