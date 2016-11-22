using Engine.Data;

namespace ELB.Models {
	public class Cell : Model {
		public int Size { get; set; }
		public float Height { get; set; }
		public Landscape Landscape { get; set; }
		public string Name { get; set; }
		public Building Building { get; set; }
		public string Owner { get; set; }
		public string[] Commanders { get; set; }
		public Collection<Unit> Prisoners { get; set; }
		public Collection<Unit> Defenders { get; set; }
	}
}