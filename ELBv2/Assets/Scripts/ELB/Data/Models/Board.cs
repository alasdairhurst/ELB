using ELB.Data.Collections;
using ELB.Data.Helpers;
using System;
using UnityEngine;

namespace ELB.Data.Models {
	public class Board : Model<Board> {
		public int CellSize { get; set; }
		public int BoardSize { get; set; }
		public string Name { get; set; }
		public string Cells_ids { get; set; }
		public Collection<Cell> Cells { get; set; }
		public float Scale { get; set; }
	}
}