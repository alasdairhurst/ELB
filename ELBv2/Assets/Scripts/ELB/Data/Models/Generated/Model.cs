using SQLite4Unity3d;

namespace ELB.Data.Models.Generated {

	public class Model {
		[PrimaryKey, Unique]
		public string _Id { get; set; }
		[Unique]
		public string _EditorID { get; set; }
	}
}
