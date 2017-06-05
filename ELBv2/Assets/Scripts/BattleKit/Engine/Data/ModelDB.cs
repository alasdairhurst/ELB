using SQLite4Unity3d;
using System.Linq;

namespace BattleKit.Engine {
	public class ModelDB {
		[PrimaryKey, Unique]
		public string _Id { get; set; }
		[Unique]
		public string _EditorId { get; set; }
	}
}
