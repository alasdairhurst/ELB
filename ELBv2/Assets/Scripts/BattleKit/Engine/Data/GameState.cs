using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
namespace BattleKit.Engine {
	public static class GameState {

		public enum ModelFlag {
			None,
			LoadedFromDB,
			Modified
		};
		private static Dictionary<string, ModelFlag> modelFlags;
		private static Database db;
		private static Cache<string, object> state;
		private static Dictionary<Type, Type> modelGeneratedTypeMap;

		static GameState() {
			db = new Database();
			modelFlags = new Dictionary<string, ModelFlag>();
			state = new Cache<string, object>();

			// generate type map
			var t = typeof(Model);
			var codeNamespace = new CodeNamespace("Engine.DB");
			codeNamespace.Imports.Add(new CodeNamespaceImport("System"));

			var subclasses = t.Assembly.GetTypes().Where(type => type.IsSubclassOf(t));
			foreach (var c in subclasses) {
				var newType = new CodeTypeDeclaration(c.Name) {
					TypeAttributes = TypeAttributes.Public
				};
				newType.BaseTypes.Add("BattleKit.Engine.ModelDB");

				PropertyInfo[] properties = c.GetProperties();
				foreach (PropertyInfo pi in properties) {
					// ignore the property if it exists in the model base
					if (typeof(ModelBase).GetProperties().Count(x => x.Name == pi.Name) != 0) {
						continue;
					}
					var snippet = new CodeSnippetTypeMember {
						Text = string.Format("public {0} {1} {{ get; set; }}",
							pi.PropertyType.IsPrimitive ? pi.PropertyType.Name : "string",
							pi.Name
						)
					};
					newType.Members.Add(snippet);
				}

				codeNamespace.Types.Add(newType);
			}

			var codeCompileUnit = new CodeCompileUnit();
			codeCompileUnit.Namespaces.Add(codeNamespace);

			var compilerParameters = new CompilerParameters {
				GenerateInMemory = true,
				IncludeDebugInformation = false,
				TreatWarningsAsErrors = true,
				WarningLevel = 4
			};
			compilerParameters.ReferencedAssemblies.Add("System.dll");
			compilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().CodeBase);

			var provider = new CSharpCodeProvider();
			var compilerResults = provider.CompileAssemblyFromDom(compilerParameters, codeCompileUnit);

			if (compilerResults == null) {
				throw new InvalidOperationException("ClassCompiler did not return results.");
			}
			if (compilerResults.Errors.HasErrors) {
				var errors = string.Empty;
				foreach (CompilerError compilerError in compilerResults.Errors) {
					errors += compilerError.ErrorText + "\n";
				}
				throw new InvalidOperationException("Errors while compiling the dynamic classes:\n" + errors);
			}

			var dynamicAssembly = compilerResults.CompiledAssembly;
			var types = dynamicAssembly.GetExportedTypes();
			var dic = new Dictionary<Type, Type>();
			for (int i = 0; i < types.Length; i++) {
				dic.Add(subclasses.ElementAt(i), types[i]);
			}
			modelGeneratedTypeMap = dic;

		}

		public static List<Type> GetGeneratedModelTypes() {
			return modelGeneratedTypeMap.Values.ToList();
		}

		public static List<Type> GetModelTypes() {
			return modelGeneratedTypeMap.Keys.ToList();
		}

		public static M FetchOne<M>(string id, bool bypassState = false) where M : Model, new() {
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
			return convertToModel<M>((ModelDB)fetchedModel);
		}

		public static Collection<M> Fetch<M>(IEnumerable<string> ids, bool bypassState = false) where M : Model, new() {
			var genInstance = (ModelDB)Activator.CreateInstance(modelGeneratedTypeMap[typeof(M)]);
			var models = new Collection<M>();
			int idCount = ids.Count();
			if (idCount == 0) {
				return models;
			}
			var genModels = new List<ModelDB>();
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

					foreach (ModelDB mo in m) {
						state.SetOne(mo._Id, mo);
						modelFlags[mo._Id] = ModelFlag.LoadedFromDB;
						genModels.Add(mo);
					}
				}
			}
			foreach(ModelDB m in genModels) {
				models.Add(convertToModel<M>(m));
			}
			return models;
		}

		public static Collection<M> FetchAll<M>(bool bypassState = false) where M : Model, new() {
			Type instanceType;
			try {
				instanceType = modelGeneratedTypeMap[typeof(M)];
			} catch (Exception) {
				throw (new Exception("Type " + typeof(M).ToString() + " not initialised.."));
			}
			var genInstance = (ModelDB)Activator.CreateInstance(instanceType);
			var models = new Collection<M>();
			if (!bypassState) {
				var fetched = state.GetAll(genInstance);
				foreach (var m in fetched) {
					if (genInstance.GetType() == m.GetType()) {
						models.Add(convertToModel<M>(m));
					}
				}
			}
			if (bypassState || models.Count == 0) {
				var fetched = (IList)typeof(Database).GetMethod("GetAll")
						.MakeGenericMethod(genInstance.GetType())
						.Invoke(db, new object[] { genInstance, false });
				foreach (ModelDB mo in fetched) {
					if (!state.ContainsKey(mo._Id)) {
						state.SetOne(mo._Id, mo);
						modelFlags[mo._Id] = ModelFlag.LoadedFromDB;
						models.Add(convertToModel<M>(mo));
					} else {
						models.Add(convertToModel<M>((ModelDB)state[mo._Id]));
					}
				}
			}
			return models;
		}

		public static void Update<M>(M model) where M : Model, new() {
			var compressedModel = convertToGenModel(model);
			// set the model
			state.SetOne(model._Id, compressedModel);
			modelFlags[model._Id] = ModelFlag.Modified;
		}



		private static M convertToModel<M>(ModelDB genModel) where M : Model, new() {
			if (typeof(M).Name != genModel.GetType().Name) {
				throw new Exception(string.Format("Trying to convert {0} to {1}", genModel.GetType().FullName, typeof(M).FullName));
			}
			
			var model = Activator.CreateInstance(typeof(M));
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
				
				if ((pi.PropertyType.IsSubclassOf(typeof(Model)) ||
					Utils.isSubclassOfRawGeneric(typeof(Collection<>), pi.PropertyType))
					&& value != null) {
						var instance = Activator.CreateInstance(pi.PropertyType);
						// call fetch on the collection/model - make sure the method invoked is the one that takes a string
						pi.PropertyType.GetMethod("Fetch", new[] { typeof(string) })
							.Invoke(instance, new[] {
						value
						});
						value = instance;
				}

				// write value
				pi.SetValue(model, value, null);
			}
			return (M)model;
		}

		private static ModelDB convertToGenModel(Model model) {
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
				if (pi.PropertyType.IsSubclassOf(typeof(Model))) {
					value = ((Model)value)._Id;
				} else if (Utils.isSubclassOfRawGeneric(typeof(Collection<>), pi.PropertyType)) {
					value = value.GetType().GetMethod("DBString").Invoke(pi.GetValue(model, null), null);
				}

				// get info for matching property
				var sp = stateModelProperties.Where(x => x.Name == pi.Name).FirstOrDefault();

				// write value
				sp.SetValue(genModel, value, null);
			}
			return (ModelDB)genModel;
		}

		public static void Delete(string id) {
			state.Remove(id);
		}

		public static void Save(SaveInfo save = null) {
			var toSave = new List<ModelDB>();
			foreach (ModelDB model in state.Values) {
				// check if the model is deleted, created or updated
				var flag = modelFlags[model._Id];
				if (flag == ModelFlag.Modified) {
					toSave.Add(model);
				}
			}
			SaveManager.SaveData(toSave, save);
		}

		public static void Load(SaveInfo save = null) {
			// first unload the current state
			Unload();
			var data = SaveManager.LoadData(save);
			foreach (ModelDB m in data) {
				state[m._Id] = m;
				modelFlags[m._Id] = ModelFlag.Modified;
			}
		}

		public static void Unload() {
			foreach(ModelDB model in state.Values) {
				if (modelFlags[model._Id] == ModelFlag.Modified) {
					state.Remove(model._Id);
				}
			}
		}
	}
}
