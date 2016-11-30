using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Data {
	public class Database {
		private Cache<string, ModelDB> cache;
		private SQLiteConnection connection;

		public Database() {
			cache = new Cache<string, ModelDB>();
			connection = new SQLiteConnection(Config.Conf.dbPath, SQLiteOpenFlags.ReadOnly);
		}

		public Model GetOne<Model>(string id, bool bypassCache = false) where Model : ModelDB, new() {
			Model model = null;
			if (!bypassCache) {
				model = cache.GetOne(id, model);
			}
			if (model == null) {
				try {
					model = connection.Find<Model>(id);
				} catch (SQLiteException e) {
					if (!e.Message.StartsWith("no such table")) {
						throw;
					}
				}
				
			}
			if (model != null) {
				cache.SetOne(id, model);
			}
			return model;
		}

		public List<Model> Get<Model>(IEnumerable<string> ids, bool bypassCache = false) where Model : ModelDB, new() {
			int idCount = ids.Count();
			if (idCount == 0) {
				return new List<Model>();
			}
			var models = new List<Model>();
			if (!bypassCache) {
				models.AddRange(cache.Get<Model>(ids));
			}
			// did we hit all of them?
			if (bypassCache || idCount != models.Count) {
				var diff = ids.Except(models.Select(x => x._Id));
				if (diff.Count() > 0) {
					try {
						var m = connection.Table<Model>().Where(x => diff.Contains(x._Id));
						foreach (Model mo in m) {
							cache.SetOne(mo._Id, mo);
						}
						models.AddRange(m);
					} catch (SQLiteException e) {
						if (!e.Message.StartsWith("no such table")) {
							throw;
						}
					}
				}
			}
			return models;
		}

		public List<Model> GetAll<Model>(Model instance = default(Model), bool bypassCache = false) where Model : ModelDB, new() {
			// until unity gets c# 6 support we will be using sqlite4unity3d.
			// afterwards we can switch to a better library such as sqlite-net

			// var cache = _cache.get();
			// var cacheId = cache.Select(x => x._Id);
			// var m = _conn.Table<Model>().Where(x => !cacheId.Contains(x._Id));
			// foreach (Model mo in m) {
			// 	_cache.set(mo._Id, mo);
			// }
			var models = new List<Model>();
			if (!bypassCache) {
				models.AddRange(cache.GetAll<Model>());
			}
			if (bypassCache || models.Count == 0) {
				try {
					var fetched = connection.Table<Model>();
					if (fetched.Count() != 0) {
						cache.Clear();
						foreach (Model mo in fetched) {
							cache.SetOne(mo._Id, mo);
						}
						models.AddRange(fetched);
					}
				} catch (SQLiteException e) {
					if (!e.Message.StartsWith("no such table")) {
						throw;
					}
				}
				
			}
			return models;
		}
	}
}
