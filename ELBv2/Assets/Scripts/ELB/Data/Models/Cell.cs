using System;
using ELB.Data.Collections;

namespace ELB.Data.Models {
	public class Cell : Model {
		public int Size { get; set; }
		public float Height { get; set; }
		public string LandscapeId { get; set; }
		public Landscape Landscape { get; set; }
		public string Name { get; set; }
		public string BuildingId { get; set; }
		public Building Building { get; set; }
		public string OwnerId { get; set; }
		public string[] Commanders { get; set; }
		public string PrisonerIds { get; set; }
		public Collection<Unit> Prisoners { get; set; }
		public string DefenderIds { get; set; }
		public Collection<Unit> Defenders { get; set; }

		public override bool Fetch(string id) {
			return Fetch<Cell>(id);
		}
		public override bool LoadTemp(string id) {
			return LoadTemp<Cell>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}

	}
}