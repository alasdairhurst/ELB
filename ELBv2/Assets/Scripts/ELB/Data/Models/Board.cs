using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	public class Board : Model {

		public Board() : base() { }

		protected override void Initialise<T>(T model) {
			Board b = (Board)(Model)model;
			if (Cells != null) {
				Cells.Fetch(b.CellIds);
			}
			else {
				Cells = new Collection<Cell>(b.CellIds);
			}
		}

		public override bool Fetch(int id) {
			return Fetch<Board>(id);
		}

		public override bool Save() {
			throw new NotImplementedException();
		}

		public int CellSize { get; set; }
		public int BoardSize { get; set; }
		public string Name { get; set; }
		public string CellIds { get; set; }
		public Collection<Cell> Cells { get; set; }
		public float Scale { get; set; }
	}
}