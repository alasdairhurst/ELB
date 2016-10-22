namespace ELB.Data.Helpers {
	static class Conf {
		// SHOULD BE IN CONFIG
		private static string dbRelPath = "/StreamingAssets/db.s3db";
#if UNITY_EDITOR
		public static string dbPath = @"Assets/" + dbRelPath;
#else
		public static string dbPath = Application.dataPath + dbRelPath;
#endif
	}
}
