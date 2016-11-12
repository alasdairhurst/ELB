using System.Collections.Generic;
using System.Linq;

namespace ELB.Data.Helpers {
	public static class GameState {

		private static Database db = new Database();
		private static Cache<string, Models.Model> state = new Cache<string, Models.Model>();

		public static Model FetchOne<Model>(string ids, bool bypassState = false) where Model : Models.Model, new() {
			Model model = null;
			if (!bypassState) {
				model = state.GetOne<Model>(ids);
			}
			if (model == null) {
				model = db.GetOne<Model>(ids);
				state.SetOne(ids, model);
			}
			return model;
		}

		public static Collections.Collection<Model> Fetch<Model>(IEnumerable<string> ids, bool bypassState = false) where Model : Models.Model, new() {
			int idCount = ids.Count();
			if (idCount == 0) {
				return new Collections.Collection<Model>();
			}
			var models = new Collections.Collection<Model>();
			if (!bypassState) {
				models.AddRange(state.Get<Model>(ids));
			}
			// did we hit all of them?
			if (bypassState || idCount != models.Count) {
				var diff = ids.Except(models.Select(x => x._Id));
				if (diff.Count() > 0) {
					var m = db.Get<Model>(ids);
					foreach (Model mo in m) {
						state.SetOne(mo._Id, mo);
					}
					models.AddRange(m);
				}
			}
			return models;
		}

		public static Collections.Collection<Model> FetchAll<Model>(bool bypassState = false) where Model : Models.Model, new() {
			var models = new Collections.Collection<Model>();
			if (!bypassState) {
				models.AddRange(state.GetAll<Model>());
			}
			if (bypassState || models.Count == 0) {
				var fetched = db.GetAll<Model>();
				foreach (Model mo in fetched) {
					state.SetOne(mo._Id, mo);
				}
				models.AddRange(fetched);
			}
			return models;
		}
	}
}
