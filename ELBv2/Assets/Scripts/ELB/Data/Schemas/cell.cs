using SQLite4Unity3d;

namespace ELB.Data.Schemas {
	public class cell : Schema {
		public cell() { }
		[NotNull]
		public int size { get; set; }
		[NotNull]
		public float height { get; set; }
		[NotNull]
		public int terrain { get; set; }
		public int function { get; set; }
		public int owner { get; set; }
		public string name { get; set; }
		public string commanders { get; set; }
		public string prisoners { get; set; }
		public string defenders { get; set; }
	}
}
