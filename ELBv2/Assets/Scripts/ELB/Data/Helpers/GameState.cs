using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ELB.Data.Helpers {
	public static class GameState {

		public enum ModelFlag {
			None,
			LoadedFromDB,
			Modified
		};
		private static Dictionary<string, ModelFlag> modelFlags = new Dictionary<string, ModelFlag>();
		private static Database db = new Database();
		private static Cache<string, object> state = new Cache<string, object>();
		private static Dictionary<Type, Type> modelGeneratedTypeMap = TypeHelper.CreateTypesForSubclassesOf(typeof(Models.Model<>));
		

		public static Model FetchOne<Model>(string id, bool bypassState = false) where Model : Models.Model, new() {
			var genInstance = Activator.CreateInstance(modelGeneratedTypeMap[typeof(Model)]);
			var fetchedModel = genInstance;
			if (!bypassState) {
				fetchedModel = state.GetOne(id, genInstance);
			}
			if (fetchedModel == null) {
				fetchedModel = typeof(Database).GetMethod("GetOne")
					.MakeGenericMethod(genInstance.GetType())
					.Invoke(db, new object[] {
						id, false
					}
				);
				state.SetOne(id, fetchedModel);
				modelFlags[id] = ModelFlag.LoadedFromDB;
			}
			return convertToModel<Model>((Models.Generated.Model)fetchedModel);
		}

		public static Collections.Collection<Model> Fetch<Model>(IEnumerable<string> ids, bool bypassState = false) where Model : Models.Model, new() {
			var genInstance = (Models.Generated.Model)Activator.CreateInstance(modelGeneratedTypeMap[typeof(Model)]);
			var models = new Collections.Collection<Model>();
			int idCount = ids.Count();
			if (idCount == 0) {
				return models;
			}
			var genModels = new List<Models.Generated.Model>();
			if (!bypassState) {
				genModels.AddRange(state.Get(ids, genInstance));
			}
			// did we hit all of them?
			if (bypassState || idCount != genModels.Count) {
				var diff = ids.Except(genModels.Select(x => x._Id));
				if (diff.Count() > 0) {
					var m = (IList)typeof(Database).GetMethod("Get")
						.MakeGenericMethod(genInstance.GetType())
						.Invoke(db, new object[] {
							ids, false
						}
					);

					foreach (Models.Generated.Model mo in m) {
						state.SetOne(mo._Id, mo);
						modelFlags[mo._Id] = ModelFlag.LoadedFromDB;
						genModels.Add(mo);
					}
				}
			}
			foreach(Models.Generated.Model m in genModels) {
				models.Add(convertToModel<Model>(m));
			}
			return models;
		}

		public static Collections.Collection<Model> FetchAll<Model>(bool bypassState = false) where Model : Models.Model, new() {
			var genInstance = (Models.Generated.Model)Activator.CreateInstance(modelGeneratedTypeMap[typeof(Model)]);
			var models = new Collections.Collection<Model>();
			if (!bypassState) {
				var fetched = state.GetAll(genInstance);
				foreach (var m in fetched) {
					if (genInstance.GetType() == m.GetType()) {
						models.Add(convertToModel<Model>(m));
					}
					
				}		
			}
			if (bypassState || models.Count == 0) {
				var fetched = (IList)typeof(Database).GetMethod("GetAll")
						.MakeGenericMethod(genInstance.GetType())
						.Invoke(db, new object[] { genInstance, false });
				foreach (Models.Generated.Model mo in fetched) {
					if (!state.ContainsKey(mo._Id)) {
						state.SetOne(mo._Id, mo);
						modelFlags[mo._Id] = ModelFlag.LoadedFromDB;
						models.Add(convertToModel<Model>(mo));
					} else {
						models.Add(convertToModel<Model>((Models.Generated.Model)state[mo._Id]));
					}
				}
			}
			return models;
		}

		public static void Update<Model>(Model model) where Model : Models.Model, new() {
			var compressedModel = convertToGenModel(model);
			// set the model
			if (compressedModel != state[model._Id]) {
				state.SetOne(model._Id, compressedModel);
				modelFlags[model._Id] = ModelFlag.Modified;
			} else {
				Debug.Log("models actually matched");
			}
		}

		private static Model convertToModel<Model>(Models.Generated.Model genModel) where Model : Models.Model, new() {
			if (typeof(Model).Name != genModel.GetType().Name) {
				throw new Error(string.Format("Trying to convert {0} to {1}", genModel.GetType().FullName, typeof(Model).FullName));
			}
			
			var model = Activator.CreateInstance(typeof(Model));
			PropertyInfo[] stateModelProperties = genModel.GetType().GetProperties();
			PropertyInfo[] modelProperties = model.GetType().GetProperties();

			foreach (PropertyInfo pi in modelProperties) {
				if (!pi.CanWrite) {
					continue;
				}

				// get info for matching property
				var sp = stateModelProperties.Where(x => x.Name == pi.Name).FirstOrDefault();

				// get value to write
				object value = sp.GetValue(genModel, null);
				// check if prop is a collection or model
				if (TypeHelper.IsSubclassOfRawGeneric(typeof(Models.Model<>), pi.PropertyType) ||
					TypeHelper.IsSubclassOfRawGeneric(typeof(Collections.Collection<>), pi.PropertyType)) {
					var instance = Activator.CreateInstance(pi.PropertyType);
					if (value != null) {
						// call fetch on the collection/model - make sure the method invoked is the one that takes a string
						pi.PropertyType.GetMethod("Fetch", new[] { typeof(string) })
							.Invoke(instance, new[] {
						value
						});
					}
					value = instance;
				}

				// write value
				pi.SetValue(model, value, null);
			}
			return (Model)model;
		}

		private static Models.Generated.Model convertToGenModel(Models.Model model) {
			var stateModelType = modelGeneratedTypeMap[model.GetType()];
			var genModel = Activator.CreateInstance(stateModelType);
			PropertyInfo[] modelProperties = model.GetType().GetProperties();
			PropertyInfo[] stateModelProperties = stateModelType.GetProperties();

			foreach (PropertyInfo pi in modelProperties) {
				if (!pi.CanWrite) {
					continue;
				}
				// get value to write
				object value = pi.GetValue(model, null);

				// check if prop is a collection or model
				if (TypeHelper.IsSubclassOfRawGeneric(typeof(Models.Model<>), pi.PropertyType)) {
					value = ((Models.Model)value)._Id;
				} else if (TypeHelper.IsSubclassOfRawGeneric(typeof(Collections.Collection<>), pi.PropertyType)) {
					value = value.GetType().GetMethod("DBString").Invoke(pi.GetValue(model, null), null);
				}

				// get info for matching property
				var sp = stateModelProperties.Where(x => x.Name == pi.Name).FirstOrDefault();

				// write value
				sp.SetValue(genModel, value, null);
			}
			return (Models.Generated.Model)genModel;
		}

		public static void Delete(string id) {
			state.Remove(id);
		}

		public static void Save() {
			SaveManager.SaveData(getDataToSave());
		}

		public static void Save(SaveInfo save) {
			SaveManager.SaveData(getDataToSave(), save);
		}

		private static List<Models.Generated.Model> getDataToSave() {
			var toSave = new List<Models.Generated.Model>();
			foreach(Models.Generated.Model model in state.Values) {
				// check if the model is deleted, created or updated
				var flag = modelFlags[model._Id];
				if (flag == ModelFlag.Modified) {
					toSave.Add(model);
				}		
			}
			return toSave;
		}
	}
}
