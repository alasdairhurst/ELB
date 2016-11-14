using SQLite4Unity3d;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ELB.Data.Models.Generated;
using System.Linq;
using System;

namespace ELB.Data.Helpers {
	static class SaveManager {

		private static SaveInfo currentSave;
		private static SQLiteConnection _conn;

		private static string generateName() {
			string date = System.DateTime.Now.ToString("yyMMddhhmmssfff");
			string hexDate = System.Convert.ToString(long.Parse(date), 16);
			return string.Format("{0}{1}{2}", "elb_", hexDate, Conf.saveExt);
		}

		private static SaveInfo getSaveInfo(FileInfo fi) {
			return new SaveInfo {
				Filename = fi.Name,
				Time = fi.CreationTimeUtc,
				IsCurrent = currentSave == null ? false : currentSave.Filename == fi.Name
			};
		}

		public static SaveInfo GetLatestSave() {
			var saves = GetSaves();
			return getLatestSave(saves);
		}

		private static SaveInfo getLatestSave(List<SaveInfo> saves) {
			SaveInfo latest = null;
			foreach (SaveInfo fi in saves) {
				if (latest == null || fi.Time > latest.Time) {
					latest = fi;
				}
			}
			return latest;
		}

		public static void SetCurrentSave(SaveInfo save) {
			currentSave = save;
		}

		public static List<SaveInfo> GetSaves() {
			var saves = new List<SaveInfo>();
			var files = Directory.GetFiles(Conf.savePath, "*" + Conf.saveExt);
			foreach (string file in files) {
				saves.Add(getSaveInfo(new FileInfo(file)));
			}
			return saves;
		}

		public static SaveInfo CreateSave() {
			try {
				// verify that the save path exists
				if (!Directory.Exists(Conf.savePath)) {
					Directory.CreateDirectory(Conf.savePath);
				}
				string newSave = Conf.savePath + generateName();
				File.Copy(Conf.defaultSavePath, newSave);
				var fi = new FileInfo(newSave);
				return getSaveInfo(fi);
			} catch (IOException e) {
				Debug.LogError(e);
				return null;
			}
		}

		public static void SaveData(List<Model> data, SaveInfo save) {
			SetCurrentSave(save);
			SaveData(data);
		}

		public static void SaveData(List<Model> data) {
			_conn = new SQLiteConnection(Conf.savePath + currentSave.Filename, SQLiteOpenFlags.ReadWrite);
			// remember types we already created tables for
			var types = new List<Type>();
			foreach(Model v in data) {
				var type = v.GetType();
				if (!types.Contains(type)) {
					_conn.CreateTable(v.GetType());
					types.Add(type);
				}
				_conn.InsertOrReplace(v);
			}
			_conn.Close();
			_conn = null;
		}

		public static List<Model> LoadData(SaveInfo save) {
			SetCurrentSave(save);
			return LoadData();
		}

		public static List<Model> LoadData() {
			_conn = new SQLiteConnection(Conf.savePath + currentSave.Filename, SQLiteOpenFlags.ReadWrite);
			var loadedData = new List<Model>();
			var subclasses =
			from assembly in AppDomain.CurrentDomain.GetAssemblies()
			from type in assembly.GetTypes()
			where type.IsSubclassOf(typeof(Model))
			select type;

			foreach(var c in subclasses) {
				var instance = Activator.CreateInstance(c);
				try {
					var data = _conn.Table(instance);
					foreach (Model item in data) {
						loadedData.Add(item);
					}
				} catch (Exception e) {
					// it's gonna complain about missing tables. ignore them
				}
			}
			_conn.Close();
			_conn = null;
			return loadedData;
		}
	}
}
