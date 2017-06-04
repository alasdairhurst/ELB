using BattleKit.Engine;

namespace ELB.Models {
	public class Board : Model {
		public int CellSize { get; set; }
		public int BoardSize { get; set; }
		public string Name { get; set; }
		public Collection<Cell> Cells { get; set; }
		public float Scale { get; set; }
	}
}
