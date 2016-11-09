using ELB.Data.Collections;
using System;
using UnityEngine;

namespace ELB.Data.Models {
	public class Board : Model {
		public Board() : base() { }

		public override bool Fetch(string id) {
			return Fetch<Board>(id);
		}
		public override bool LoadTemp(string id) {
			return LoadTemp<Board>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}

		public int CellSize { get; set; }
		public int BoardSize { get; set; }
		public string Name { get; set; }
		public string Cells_ids { get; set; }
		public Collection<Cell> Cells { get; set; }
		public float Scale { get; set; }
	}
}