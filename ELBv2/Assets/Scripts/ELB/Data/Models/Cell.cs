using System;
using SQLite4Unity3d;

namespace ELB.Data.Models {
	public class Cell : Model {
		[NotNull]
		public int Size { get; set; }
		[NotNull]
		public float Height { get; set; }
		[NotNull]
		public int Terrain { get; set; }
		public int Function { get; set; }
		public int Owner { get; set; }
		public string Name { get; set; }
		public string[] Commanders { get; set; }
		public string[] Prisoners { get; set; }
		public string[] Defenders { get; set; }

		public override bool Fetch(int id) {
			return Fetch<Cell>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}

	}
}