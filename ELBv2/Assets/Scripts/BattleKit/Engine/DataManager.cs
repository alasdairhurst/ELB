using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using JsonDiffPatchDotNet;
using Newtonsoft.Json.Schema;

namespace BattleKit.Engine {

	public class ValidationException : Exception {
		public ValidationException(string message) : base(message) { }
	}


	public class DependencyException : Exception {
		public DependencyException(string message) : base(message) { }
	}

	public enum FileType {
		Plugin,
		Save
	}

	public class FileMetadata : iSerializable {
		public string name;
		public string lastModified;
		public string author;
		public string version;
		public string[] dependencies;
		public FileType type;
		public bool readOnly;

		public FileMetadata() {}

		public FileMetadata(JToken t) {
			name = t.Value<string>("name");
			lastModified = t.Value<string>("lastModified");
			author = t.Value<string>("author");
			version = t.Value<string>("version");
			dependencies = t["dependencies"].Values<string>().ToArray();
			//type = t.Value<FileType>("type");
			readOnly = t.Value<bool>("readOnly");
		}

		public JToken Serialize() {
			return new JObject(
				new JProperty("name", name),
				new JProperty("lastModified", lastModified),
				new JProperty("author", author),
				new JProperty("version", version),
				new JProperty("dependencies",
					new JArray(dependencies)
				),
				//new JProperty("type", type),
				new JProperty("readOnly", readOnly)
			);
		}
	}

	public class DataManager : ScriptableObject {

		private const string _databaseSchemaPath = "/StreamingAssets/Schema/DatabaseSchema.json";

		private Dictionary<string, FileMetadata> _sources;
		private JObject _databaseData;
		private string _nonActiveJSON;
		private JSchema _databaseSchema;
		private string _activeSource = null;

		// Models that are hardcoded in the game
		public List<Model> models;
		public bool loaded;

		public DataManager() {
			//_sources = new Dictionary<string, FileMetadata>();
			//_databaseData = new JObject();
			//_databaseData.Add("ids", new JObject());
			//_databaseData.Add("data", new JObject());
			// Load schema
			//string schema = LoadFileString(Application.dataPath + _databaseSchemaPath);
			//_databaseSchema = JSchema.Parse(schema);
			//LoadModels(models);
		}

		public void OnEnable() {
			Debug.Log("DataManager Enabled");
			if (!loaded) {
				_nonActiveJSON = null;
				_activeSource = null;
				_sources = new Dictionary<string, FileMetadata>();
				_databaseData = new JObject();
				_databaseData.Add("ids", new JObject());
				_databaseData.Add("data", new JObject());
				// Load schema
				string schema = LoadFileString(Application.dataPath + _databaseSchemaPath);
				_databaseSchema = JSchema.Parse(schema);
				LoadModels(models);
				loaded = true;
			}
		}

		private void LoadModels(List<Model> models) {
			if (_sources.Count > 0) {
				throw new Exception("Cannot load models into manager after loading from external sources");
			}
			if (models != null) {
				models.ForEach(addModel);
			}
			_nonActiveJSON = _databaseData.ToString(Newtonsoft.Json.Formatting.None);
		}

		private void addModel<T>(T m) where T : Model {
			_addModel(m, true);
		}
 
		private void _addModel<T>(T m, bool checkID) where T : Model {
			if (m == null || m.id == null) {
				throw new Exception("Cannot add model. Model is null or ID is missing");
			}
			var hasId = HasID(m.id);
			if (checkID && hasId) {
				throw new Exception("Model with id " + m.id + " already exists");
			}
			var type = EnsureType(typeof(T));
			type[m.id] = m.Serialize();
			if (!hasId) {
				((JObject)_databaseData["ids"]).Add(new JProperty(m.id, m.name));
			}
		}

		public T CreateModel<T>() where T : Model {
			var m = ScriptableObject.CreateInstance<T>();
			m.id = GenerateID();
			addModel(m);
			return m;
		}

		public string GenerateID(bool unique = true) {
			string id;
			do {
				id = Guid.NewGuid().ToString().ToUpper();
			}
			while (unique && HasID(id));
			return id;
		}

		public T GetModel<T>(string id) where T : Model {
			if (_databaseData["data"][typeof(T).Name] == null) {
				return null;
			}
			var data = _databaseData["data"][typeof(T).Name][id];
			if (data == null) {
				return null;
			}
			var m = ScriptableObject.CreateInstance<T>();
			m.Init(data);
			m.name = _databaseData["ids"].Value<string>(id);
			return m;
		}

		public void UpdateModel<T>(T m) where T : Model {
			if (!HasID(m.id)) {
				throw new Exception("Cannot update model which does not exist");
			}
			_addModel(m, false);
		}

		public void DeleteModel<T>(T m) where T : Model {
			if (m == null) {
				throw new Exception("Cannot delete model. Model is null");
			}
			DeleteModel<T>(m.id);
		}

		public void DeleteModel<T>(string id) where T : Model {
			if (id == null) {
				throw new Exception("Cannot delete model. Model id is missing");
			}
			var data = _databaseData["data"][typeof(T).Name][id];
			if (data == null) {
				return;
			}
			_databaseData["data"][typeof(T).Name][id].Remove();
			_databaseData["ids"][id].Remove();
		}


		public JObject EnsureType(Type t) {
			JObject type = _databaseData["data"][t.Name] as JObject;
			if (type == null) {
				((JObject)_databaseData["data"]).Add(
					new JProperty(t.Name, new JObject())
				);
				type = (JObject)_databaseData["data"][t.Name];
			}
			return type;
		}

		public bool HasID(string id) {
			return _databaseData["ids"][id] != null;
		}

		public string LoadFileString(string filePath) {
			if (File.Exists(filePath)) {
				return File.ReadAllText(filePath);
			} else {
				throw new Exception("Cannot load " + filePath + ". File does not exist");
			}
		}

		public JObject GetData() {
			return _databaseData;
		}

		public void LoadSave(string fileName) {
			var filePath = Path.Combine(Application.persistentDataPath, fileName);
			LoadData(filePath, FileType.Save, false);
		}

		public void LoadPlugin(string fileName) {
			var filePath = Application.dataPath + "/StreamingAssets/" + fileName;
			LoadData(filePath, FileType.Plugin, true);
		}

		public void LoadData(string source, FileType type, bool readOnly = true) {
			if (hasActiveSource()) {
				Debug.Log(_activeSource);
				throw new Exception("Cannot load data when active source is loaded");
			}
			Debug.Log("Loading data from " + source);
			var jsonString = LoadFileString(source);
			// deserialise
			var json = JObject.Parse(jsonString);
			// validate
			IList<string> validationMessages;
			if (json.IsValid(_databaseSchema, out validationMessages)) {
				var metadata = new FileMetadata(json["metadata"]);
				if (readOnly) {
					metadata.readOnly = true;
				}
				metadata.type = type;

				var loadedDependencies = getActiveSourceDependencies();
				var nonLoadedDependencies = metadata.dependencies.Except(loadedDependencies);
				if (nonLoadedDependencies.Count() > 0) {
					var deps = string.Join(", ", nonLoadedDependencies.ToArray());
					throw new DependencyException(source + " requires dependencies which are not loaded: " + deps);
				}

				var jdp = new JsonDiffPatch();
				_databaseData = (JObject)jdp.Patch(_databaseData, json["data"]);
				_sources.Add(source, metadata);
				if (!readOnly) {
					_activeSource = source;
				} else {
					_nonActiveJSON = _databaseData.ToString(Newtonsoft.Json.Formatting.None);
					_activeSource = null;
				}
			} else {
				var validationMessage = string.Join("\n", validationMessages.ToArray());
				throw new ValidationException("Validation failed when loading data from source: " + source + validationMessage);
			}
		}

		private string[] getActiveSourceDependencies() {
			return (from s in _sources
					where s.Key != _activeSource
					select Path.GetFileNameWithoutExtension(s.Key)).ToArray();
		}

		public void CreateSave(string author, string name, string version) {
			var src = new FileMetadata();
			src.author = author;
			src.version = name;
			src.name = version;
			src.type = FileType.Save;
			src.lastModified = DateTime.Now.ToFileTimeUtc().ToString();
			var filePath = Application.persistentDataPath + "/Save_" + src.lastModified + ".sav";
			CreateSource(src, filePath);
		}

		public void CreatePlugin(string author, string name, string version, string fileName) {
			var src = new FileMetadata();
			src.author = author;
			src.version = name;
			src.name = version;
			src.type = FileType.Plugin;
			src.lastModified = DateTime.Now.ToFileTimeUtc().ToString();

			var filePath = Application.dataPath + "/StreamingAssets/" + fileName + ".dat";
			CreateSource(src, filePath);
		}

		public void CreateSource(FileMetadata sourceMetadata, string filePath) {
			if (hasActiveSource()) {
				throw new Exception("Cannot create source when one is currently active");
			}

			sourceMetadata.dependencies = getActiveSourceDependencies();
			_sources[filePath] = sourceMetadata;
			_activeSource = filePath;
			SaveData();
		}

		public string getActiveSource() {
			return _activeSource;
		}

		public bool hasActiveSource() {
			return _activeSource != null && _activeSource != string.Empty;
		}

		public void SaveData() {
			if (!hasActiveSource()) {
				throw new Exception("No active source. Data could not be saved.");
			}
			var jdp = new JsonDiffPatch();
			var loadedJSON = JObject.Parse(_nonActiveJSON);
			// get what's changed between the last loaded non-active source and the current database
			var diff = jdp.Diff(loadedJSON, _databaseData);

			var metadata = _sources[_activeSource];
			metadata.lastModified = DateTime.Now.ToFileTimeUtc().ToString();
			metadata.dependencies = getActiveSourceDependencies();

			var saveData = new JObject(
				new JProperty("metadata", metadata.Serialize()),
				new JProperty("data", diff)
			);
			var saveString = saveData.ToString(Newtonsoft.Json.Formatting.Indented);
			Debug.Log("Saving to " + _activeSource);
			File.WriteAllText(_activeSource, saveString);
		}
	}
}


/*
 * 
 * TODO:
 *	remove full path from dependencies (use relative path to data folder or something)
 *	figure out good way of storing embedded model classes when saving/loading model
 *  maybe it's fine for ID dictionary to be generated at load and not saved into the file
 * 
 * /


/*
	Load as save or plugin
	Save as save or plugin
	Create as save or plugin


/*

/*
 * load all Models into memory and serialise them as json strings in database format
 * 
 * load all diffs in order and patch the database JSON
 * 
 * store each database jsons
 * 
 * deserialise JSON as JObject
 * 
 * deserialise portions of JObject as needed at runtime into game state
 * 
 * on save serialise loaded models into JObject database DataStore
 * 
 * serialise database into json
 * 
 * diff new json against json saved in step 3
 * 
 * overwrite/save diff to disk in place of last loaded file.
 * 
 */
