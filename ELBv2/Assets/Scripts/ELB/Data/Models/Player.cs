using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	public class Player : Model {
		public int TeamId { get; set; }
		public string UnitIds { get; set; }
		public Collection<Unit> Units { get; set; }
		public int Money { get; set; }
		public int Level { get; set; }
	}
}
