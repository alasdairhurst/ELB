using SQLite4Unity3d;
using UnityEngine;

namespace ELB.Data.Helpers {
	static class SaveManager {

		private static string currentSave;
		private static SQLiteConnection _conn;

		private static string generateName() {
			string date = System.DateTime.Now.ToString("yyMMddhhmmssfff");
			string hexDate = System.Convert.ToString(long.Parse(date), 16);
			return string.Format("{0}{1}{2}", "elb_", hexDate, Conf.saveExt);
		}

		private static Save getLatestSave(string[] saves) {
			System.IO.FileInfo latest = null;
			foreach (string s in saves) {
				try {
					var fi = new System.IO.FileInfo(s);
					if (latest == null || fi.CreationTime > latest.CreationTime) {
						latest = fi;
					}
				} catch (System.IO.FileNotFoundException e) { }
			}
			return new Save {
				Filename = latest.Name,
				Time = latest.CreationTime,
				current = currentSave == latest.Name
			};
		}

		public static bool LoadSave(Save save) {
			if (currentSave == save.Filename) {
				return false;
			}
			_conn = new SQLiteConnection(Conf.savePath + save.Filename, SQLiteOpenFlags.ReadWrite);
			currentSave = save.Filename;
			return true;
		}

		public static string[] GetSaves() {
			return System.IO.Directory.GetFiles(Conf.savePath, "*" + Conf.saveExt);
		}

		public static bool CreateSave() {
			// verify that the save path exists
			if (!System.IO.Directory.Exists(Conf.savePath)) {
				System.IO.Directory.CreateDirectory(Conf.savePath);
			}
			string newSave = Conf.savePath + generateName();
			System.IO.File.Copy(Conf.defaultSavePath, newSave);
			return true;
		}
	}
}
