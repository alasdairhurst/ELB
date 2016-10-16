using SQLite4Unity3d;

namespace ELB.Data.Schemas {
	public class Schema {
		public Schema() { }
		[PrimaryKey, AutoIncrement]
		public int _id { get; set; }
	}
}
