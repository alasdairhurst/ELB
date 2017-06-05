using BattleKit.Engine;
using System.Collections.Generic;

namespace ELB.Models {
	public enum something {
		test,
		no
	}
	public class Player : Model {
		public int TeamId;
		public List<Unit> Units;
		public int Money;
		public int Level;
		public bool Text;
		public something Something;
	}
}
