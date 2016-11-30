using System.Reflection;
using System;

namespace Engine.Data {

	public class Model : ModelBase {
		
		public Model() {
			// generate a guid
			_Id = Guid.NewGuid().ToString("B").ToUpper();
		}

		public Model(Model model) : base() {
			Initialise(model);
		}

		private void Initialise(Model model) {
			PropertyInfo[] properties = model.GetType().GetProperties();
			foreach (PropertyInfo pi in properties) {
				if (!pi.CanWrite) {
					continue;
				}
				// get the value of the current property in the passed model
				var value = pi.GetValue(model, null);
				// set the value in the current model
				pi.SetValue(this, value, null);
			}
		}

		// Fetch model contents from state
		public void Fetch(string id) {
			var model = typeof(GameState).GetMethod("FetchOne")
				.MakeGenericMethod(GetType())
				.Invoke(null, new object[] {
					id, false
				});
			if (model != null) {
				Initialise((Model)model);
			}
		}

		// Fetch model contents from state
		public void Fetch() {
			Fetch(_Id);
		}

		// Delete this model from the state
		public void Delete() {
			// reset the model? change the id?
			GameState.Delete(_Id);
		}

		// Save contents of model into state
		public void Save() {
			typeof(GameState).GetMethod("Update")
				.MakeGenericMethod(GetType())
				.Invoke(null, new[] {
					this
				});
		}
	}
}
