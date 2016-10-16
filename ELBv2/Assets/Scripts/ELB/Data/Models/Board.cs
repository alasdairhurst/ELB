using ELB.Data.Schemas;
using ELB.Data.Collections;

namespace ELB.Data.Models {
	public class Board : Model<board> {

		public Board() : base() { }
		public Board(Board b) : base(b) { }
		public Board(Model<board> b) : base(b) { }

		protected override void Initialise(board schema) {
			base.Initialise(schema);
			CellSize = schema.cell_size;
			Name = schema.name;
			BoardSize = schema.board_size;
			if (Cells != null) {
				Cells.Fetch(schema.cells);
			} else {
				Cells = new Collection<Cell>(schema.cells);
			}
			Scale = schema.scale;
		}

		public int CellSize { get; set; }
		public int BoardSize { get; set; }
		public string Name { get; set; }
		public Collection<Cell> Cells { get; set; }
		public float Scale { get; set; }
	}
}