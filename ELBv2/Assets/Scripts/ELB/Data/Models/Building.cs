﻿using ELB.Data.Collections;
using System;

namespace ELB.Data.Models {
	public class Building : Model<Building> {
		public string Name { get; set; }
		public string Prefab { get; set; }
	}
}
