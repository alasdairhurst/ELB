using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ELB.Data.Helpers {
	public static class GameState {

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
						genModels.Add(mo);
					}
				}
			}
			foreach(Models.Generated.Model m in genModels) {
				models.Add(convertToModel<Model>(m));
			}
			return models;
		}

		static List<T> Cast<T>(T instance, List<T> GenericList) where T : Models.Generated.Model {
			return GenericList;
		}

		public static Collections.Collection<Model> FetchAll<Model>(bool bypassState = false) where Model : Models.Model, new() {
			var genInstance = (Models.Generated.Model)Activator.CreateInstance(modelGeneratedTypeMap[typeof(Model)]);
			var models = new Collections.Collection<Model>();
			if (!bypassState) {
				models.AddRange(state.GetAll<Model>());
			}
			if (bypassState || models.Count == 0) {
				var fetched = (IList)typeof(Database).GetMethod("GetAll")
						.MakeGenericMethod(genInstance.GetType())
						.Invoke(db, null);
				foreach (Models.Generated.Model mo in fetched) {
					state.SetOne(mo._Id, mo);
					models.Add(convertToModel<Model>(mo));
				}
			}
			return models;
		}

		public static void Update<Model>(Model model) where Model : Models.Model, new() {
			// should it be up to the model or consumer of the model to update the state?
			//Model compressedModel = new Model();
			var compressedModel = Activator.CreateInstance(modelGeneratedTypeMap[typeof(Model)]);

			PropertyInfo[] properties = model.GetType().GetProperties();

			foreach (PropertyInfo pi in properties) {
				if (!pi.CanWrite) {
					continue;
				}

				// check if prop has an associated _ids string
				var idsProp = properties.Where(x => x.Name == pi.Name + "_ids").FirstOrDefault();
				if (idsProp != null) {
					// has _ids string associated. we don't want to write it
					continue;
				}
				object value;
				// if property is an id string
				if (pi.Name.EndsWith("_ids")) {
					// extract the ids string out of the associated collection and write it
					var collectionPropName = pi.Name.Substring(0, pi.Name.LastIndexOf("_ids"));
					// try to find the collection property that is mapped to the current id prop
					var collectionProp = properties.Where(x => x.Name == collectionPropName).FirstOrDefault();
					if (collectionProp == null) {
						continue;
					}
					// call the method on the collection to turn it to a string
					var collection = collectionProp.GetValue(model, null);
					value = collection.GetType().GetMethod("DBString")
						.Invoke(collection, null);

				} else {
					// get the value of the current property
					value = pi.GetValue(model, null);
				}
				// set the value in the model
				pi.SetValue(compressedModel, value, null);
			}
			// set the model
			state.SetOne(model._Id, compressedModel);
		}

		public static void Update2<Model>(Model model) where Model : Models.Model, new() {
			var compressedModel = convertToGenModel(model);
			// set the model
			state.SetOne(model._Id, compressedModel);
			foreach(object m in state.Values) {
				Debug.Log(m);
			}
		}

		private static Model convertToModel<Model>(Models.Generated.Model genModel) where Model : Models.Model, new() {
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
				Debug.Log(sp.Name);
				Debug.Log(value);

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

		public static void Save() {
			SaveManager.SaveData(state);
		}

		public static void Save(SaveInfo save) {
			SaveManager.SaveData(state, save);
		}

		public static SQLite4Unity3d.SQLiteConnection getConnection() {
			return db.getConn();
		}
	}
}
