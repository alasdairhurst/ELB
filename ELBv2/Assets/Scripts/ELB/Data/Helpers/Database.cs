using ELB.Data.Collections;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using ELB.Data.Models;
using UnityEngine;

namespace ELB.Data.Helpers {
	public class Database {
		private Cache<string, Models.Generated.Model> cache;
		private SQLiteConnection connection;

		public Database() {
			cache = new Cache<string, Models.Generated.Model>();
			connection = new SQLiteConnection(Conf.dbPath, SQLiteOpenFlags.ReadWrite);
		}

		public Model GetOne<Model>(string id, bool bypassCache = false) where Model : Models.Generated.Model, new() {
			Debug.Log(typeof(Model));
			Model model = null;
			if (!bypassCache) {
				model = cache.GetOne(id, model);
			}
			if (model == null) {
				model = connection.Find<Model>(id);
			}
			if (model != null) {
				cache.SetOne(id, model);
			}
			return model;
		}

		public List<Model> Get<Model>(IEnumerable<string> ids, bool bypassCache = false) where Model : Models.Generated.Model, new() {
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
					var m = connection.Table<Model>().Where(x => diff.Contains(x._Id));
					foreach (Model mo in m) {
						cache.SetOne(mo._Id, mo);
					}
					models.AddRange(m);
				}
			}
			return models;
		}

		public List<Model> GetAll<Model>(Model instance = default(Model), bool bypassCache = false) where Model : Models.Generated.Model, new() {
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

		public SQLiteConnection getConn() {
			return connection;
		}
	}
}
