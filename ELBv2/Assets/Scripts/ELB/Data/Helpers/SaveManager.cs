using SQLite4Unity3d;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ELB.Data.Models;
using System.Linq;

namespace ELB.Data.Helpers {
	static class SaveManager {

		private static string currentSave;
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
				IsCurrent = currentSave == fi.Name
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

		public static bool SetCurrentSave(SaveInfo save) {
			if (currentSave == save.Filename) {
				return false;
			}
			if (_conn != null) {
				_conn.Close();
			}	
			_conn = new SQLiteConnection(Conf.savePath + save.Filename, SQLiteOpenFlags.ReadWrite);
			currentSave = save.Filename;
			return true;
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

		public static void SaveData(Cache<string, object> data, SaveInfo save) {
			SetCurrentSave(save);
			SaveData(data);
		}

		public static void SaveData(Cache<string, object> data) {
			foreach(Models.Generated.Model v in data.Values) {
				//_conn.CreateTable(v.GetType());
				_conn.InsertOrReplace(v);
			}
		}

		public static Cache<string, object> LoadData(SaveInfo save) {
			SetCurrentSave(save);
			return LoadData();
		}

		public static Cache<string, object> LoadData() {
			var loadedData = new Cache<string, object>();
			// BBEEEUUUUUUAAAAAHHHHH... 
					   /*%%%%%
					   %%%% = =
					   %%C    >
						_)' _( .' ,
					 __/ |_/\   " *. o
					/` \_\ \/     %`= '_  .
				   /  )   \/|      .^',*. ,
				  /' /-   o/       - " % '_
				 /\_/     <       = , ^ ~ .
				 )_o|----'|          .`  '
			 ___// (_  - (\
			///-(    \'  */
			var subclasses =
			from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
			from type in assembly.GetTypes()
			where type.IsSubclassOf(typeof(Model<>))
			select type;

			foreach(var c in subclasses) {
				var instance = System.Activator.CreateInstance(c);
				Debug.Log(instance);
				var data = _conn.Table(instance);
				foreach (Model item in data) {
					loadedData.Add(item._Id, item);
				}
			}
			return loadedData;
		}
	}
}
