using SQLite4Unity3d;

namespace ELB.Data.Schemas {
	public class board : Schema {
		public board() { }
		public int cell_size { get; set; }
		public int board_size { get; set; }
		public string name { get; set; }
		public string cells { get; set; }
		public float scale { get; set; }
	}
}
