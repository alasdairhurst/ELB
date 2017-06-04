using BattleKit.Engine;
namespace ELB.Models {
	public enum something {
		test,
		no
	}
	public class Player : Model {
		public int TeamId { get; set; }
		public Collection<Unit> Units { get; set; }
		public int Money { get; set; }
		public int Level { get; set; }
		public bool Text { get; set; }
		public something Something { get; set; }
	}
}
