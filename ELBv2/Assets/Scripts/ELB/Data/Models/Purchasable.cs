using System;

namespace ELB.Data.Models {
	public class Purchasable : Model {
		public string Name { get; set; }
		public int Cost { get; set; }
		public int Level { get; set; }
		public string Icon { get; set; }
		public string Type { get; set; }
		public string TypeId { get; set; }
	}
}
