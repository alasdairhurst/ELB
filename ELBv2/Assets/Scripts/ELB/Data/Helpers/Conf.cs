namespace ELB.Data.Helpers {
	static class Conf {
		// SHOULD BE IN CONFIG FILE
		private static string dbName = "db.s3db";
		private static string defaultSaveName = "default.sav";

		public static string saveExt = ".sav";

#if UNITY_EDITOR
		public static string assetsPath = @"Assets/StreamingAssets/";
#else
		public static string assetsPath = Application.dataPath + "/StreamingAssets/";
#endif
		public static string dbPath = assetsPath + dbName;

		public static string saveRelPath = "/My Games/ELB/Saves/";

		public static string savePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + saveRelPath;

		public static string defaultSavePath = assetsPath + defaultSaveName;

		
	}
}
