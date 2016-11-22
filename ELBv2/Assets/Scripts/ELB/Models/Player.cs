using Engine.Data;

namespace ELB.Models {
	public class Player : Model {
		public int TeamId { get; set; }
		public string UnitIds { get; set; }
		public Collection<Unit> Units { get; set; }
		public int Money { get; set; }
		public int Level { get; set; }
	}
}
