using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

namespace ELB.Data.Helpers {
	public class Database {
		private Cache<string, Models.Model> cache;
		private SQLiteConnection connection;

		public Database() {
			cache = new Cache<string, Models.Model>();
			connection = new SQLiteConnection(Conf.dbPath, SQLiteOpenFlags.ReadOnly);
		}

		public Model GetOne<Model>(string id, bool bypassCache = false) where Model : Models.Model, new() {
			Model model = null;
			if (!bypassCache) {
				model = cache.GetOne<Model>(id);
			}
			if (model == null) {
				model = connection.Find<Model>(id);
			}
			if (model != null) {
				cache.SetOne(id, model);
			}
			return model;
		}

		public Collections.Collection<Model> Get<Model>(IEnumerable<string> ids, bool bypassCache = false) where Model : Models.Model, new() {
			int idCount = ids.Count();
			if (idCount == 0) {
				return new Collections.Collection<Model>();
			}
			var models = new Collections.Collection<Model>();
			if (!bypassCache) {
				models.AddRange(cache.Get<Model>(ids));
			}
			// did we hit all of them?
			if (bypassCache || idCount != models.Count) {
				var diff = ids.Except(models.Select(x => x._Id));
				if (diff.Count() > 0) {
					var m = connection.Table<Model>().Where(x => diff.Contains(x._Id));
					foreach (Model mo in m) {
						cache.SetOne(mo._Id, mo);
					}
					models.AddRange(m);
				}
			}
			return models;
		}

		public Collections.Collection<Model> GetAll<Model>(bool bypassCache = false) where Model : Models.Model, new() {
			// until unity gets c# 6 support we will be using sqlite4unity3d.
			// afterwards we can switch to a better library such as sqlite-net

			// var cache = _cache.get();
			// var cacheId = cache.Select(x => x._Id);
			// var m = _conn.Table<Model>().Where(x => !cacheId.Contains(x._Id));
			// foreach (Model mo in m) {
			// 	_cache.set(mo._Id, mo);
			// }
			var models = new Collections.Collection<Model>();
			if (!bypassCache) {
				models.AddRange(cache.GetAll<Model>());
			}
			if (bypassCache || models.Count == 0) {
				var fetched = connection.Table<Model>();
				if (fetched.Count() != 0) {
					cache.Clear();
					foreach (Model mo in fetched) {
						cache.SetOne(mo._Id, mo);
					}
					models.AddRange(fetched);
				}
			}
			return models;
		}
	}
}
