﻿using UnityEngine;
using System.Collections;

public class TerrainTypeData : TileTypeDataBase {
    public TerrainType Type;
    public TerrainTypeData() {
        Type = TerrainType.Grass;
        Tile = TileType.Terrain;
    }
}