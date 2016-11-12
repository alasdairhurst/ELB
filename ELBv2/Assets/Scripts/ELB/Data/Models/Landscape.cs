using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	public class Landscape : Model<Landscape> {
		public string Name { get; set; }
		public string Prefab { get; set; }
	}
}