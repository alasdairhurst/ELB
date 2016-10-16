using ELB.Data.Schemas;

namespace ELB.Data.Models {
	public class Cell : Model<cell> {

		protected override void Initialise(cell schema) {
			base.Initialise(schema);
			Size = schema.size;
			Height = schema.height;
			Terrain = schema.terrain;
			Function = schema.function;
			Owner = schema.owner;
			Name = schema.name;
		}

		public int Size { get; set; }
		public float Height { get; set; }
		public int Terrain { get; set; }
		public int Function { get; set; }
		public int Owner { get; set; }
		public string Name { get; set; }
		public string[] Commanders { get; set; }
		public string[] Prisoners { get; set; }
		public string[] Defenders { get; set; }
	}
}