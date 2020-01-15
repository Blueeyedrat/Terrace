using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element {
	NULL, AIR, FIRE, ICE, PLANT, STONE, WATER
}
public enum TileAttribute {
	Null = 0,
	ElevationSea = 10,
	ElevationValley = 11,
	ElevationPlain = 12,
	ElevationHill = 13,
	ElevationMountain = 14,
	TerrainStone = 20,
	TerrainGrass = 21,
	TerrainSnow = 22,
	TerrainVolcano = 23,
	WaterDry = 30,
	WaterFeature = 31,
	DetailEmpty = 40,
	DetailTrees = 41,
	DetailVillage = 42,
	DetailFort = 43
}

[System.Serializable]
public class TileState
{
	private int elevation;	// 0=sea, 1=valley, 2=plain, 3=hill, 4=mountain
	private int terrain;	// 0=stone, 1=grass or volcano, 2=snow
	private int water;		// 0=dry, 1=water
	private int detail;		// 0=empty, 1=trees, 2=village, 3=fort
	
	public int Elevation { get { return elevation; } }
	public int Terrain { get { return terrain; } }
	public int Water { get { return water; } }
	public int Detail { get { return detail; } }
	
	public void SetElevation (int e) { elevation = e; }
	public void SetTerrain (int t) { terrain = t; }
	public void SetWater (int w) { water = w; }
	public void SetDetail (int d) { detail = d; }
	
	public void SetState (int e, int t, int w, int d) {
		SetElevation(e);
		SetTerrain(t);
		SetWater(w);
		SetDetail(d);
	}
	public void SetState (TileState state) {
		SetElevation(state.Elevation);
		SetTerrain(state.Terrain);
		SetWater(state.Water);
		SetDetail(state.Detail);
	}
	
	public TileState () {
		this.SetState(0,0,0,0);
	}
	public TileState (int e, int t, int w, int d) {
		this.SetState(e,t,w,d);
	}
	
	/* Returns true if tile state is valid within logic */
	public bool ValidState () {
		return TileState.ValidState(elevation, terrain, water, detail);
	}
	public static bool ValidState (int e, int t, int w, int d) {
		if (e == 0 || e == 4) {
			if (t == 0 || t == 1 || t == 2) {
				if (w == 0) {
					if (d == 0) return true;
				}
			}
		} else if (e == 1 || e == 2 || e == 3) {
			if (t == 0) {
				if (w == 0) {
					if (d == 0 || d == 2 || d == 3) return true;
				} else if (w == 1) {
					if (d == 0 || d == 2) return true;
				}
			} else if (t == 1 || t == 2) {
				if (w == 0) {
					if (d == 0 || d == 1 || d == 2 || d == 3) return true;
				} else if (w == 1) {
					if (d == 0 || d == 1 || d == 2) return true;
				}
			}
		}
		return false;
	}
	
	/* Returns true if tile has specific attribute */
	bool ElevationCap () {
		return (elevation == 0 || elevation == 4);
	}
	public bool HasAttribute (TileAttribute att) {
		switch (att) {
			case TileAttribute.ElevationSea:
				return (elevation == 0);
			case TileAttribute.ElevationValley:
				return (elevation == 1);
			case TileAttribute.ElevationPlain:
				return (elevation == 2);
			case TileAttribute.ElevationHill:
				return (elevation == 3);
			case TileAttribute.ElevationMountain:
				return (elevation == 4);
				
			case TileAttribute.TerrainStone:
				return (terrain == 0 && elevation > 0);
			case TileAttribute.TerrainGrass:
				return (terrain == 1 && !ElevationCap());
			case TileAttribute.TerrainSnow:
				return (terrain == 2);
			case TileAttribute.TerrainVolcano:
				return (terrain == 1 && ElevationCap());
				
			case TileAttribute.WaterDry:
				return (water == 0 && !ElevationCap());
			case TileAttribute.WaterFeature:
				return (water == 1 && !ElevationCap());
				
			case TileAttribute.DetailEmpty:
				return (detail == 0 && !ElevationCap());
			case TileAttribute.DetailTrees:
				return (detail == 1 && !ElevationCap());
			case TileAttribute.DetailVillage:
				return (detail == 2 && !ElevationCap());
			case TileAttribute.DetailFort:
				return (detail == 3 && !ElevationCap());
		}
		return false;
	}
	/* Check for multiple attributes (AND, OR) */
	public bool HasAttributeAND (TileAttribute[] atts) {
		if (atts.Length < 1) return false;
		foreach (TileAttribute att in atts) {
			if (!HasAttribute(att)) return false;
		}
		return true;
	}
	public bool HasAttributeOR (TileAttribute[] atts) {
		if (atts.Length < 1) return false;
		foreach (TileAttribute att in atts) {
			if (HasAttribute(att)) return true;
		}
		return false;
	}
	
	/* Set a random tile state */
	public void RandomState (float[][] randomWeights) {
		do {
			SetElevation(Utilities.WeightedRandom(randomWeights[0]) % 5);
			SetTerrain(Utilities.WeightedRandom(randomWeights[1]) % 4);
			SetWater(Utilities.WeightedRandom(randomWeights[2]) % 2);
			SetDetail(Utilities.WeightedRandom(randomWeights[3]) % 4);
			if (ElevationCap()) RemoveFeatures();
			if (terrain == 3) terrain = 1;	// volcano check!
		} while (!ValidState());
	}
	
	/* Remove grass, trees, water from a tile */
	void RemoveFeatures () {
		water = 0;
		detail = 0;
		if (terrain == 1) {
			terrain = 0;
		}
	}
	/* Reduce one layer if possible */
	void Erode () {
		if (elevation > 0) {
			if (terrain != 1 && detail != 1) {	// Grass, trees prevent erosion
				elevation--;
				if (elevation == 0) {	// Sea layer
					RemoveFeatures();
				}
			}
		}
	}
	/* Elevate one layer if possible */
	void Elevate () {
		if (elevation < 4) {
			elevation++;
			if (elevation == 4) {	// Mountain layer
				RemoveFeatures();
			}
		}
	}
	
	/* Update tile state based on element and drop type */
	public void ChangeState (Element elem, bool direct) {
		switch (elem) {
			case Element.AIR:
				ChangeAir(direct);
				break;
			case Element.FIRE:
				ChangeFire(direct);
				break;
			case Element.ICE:
				ChangeIce(direct);
				break;
			case Element.PLANT:
				ChangePlant(direct);
				break;
			case Element.STONE:
				ChangeStone(direct);
				break;
			case Element.WATER:
				ChangeWater(direct);
				break;
		}
	}
	/* AIR:
	- erode tile */
	void ChangeAir (bool direct) {
		if (detail == 3) {
			if (!direct) detail = 0;
		} else if (detail == 2) {
			detail = 0;
		} else {
			if (elevation == 4) RemoveFeatures();
			Erode();
		}
	}
	/* FIRE:
	- valley, plain, hill:
	-- remove all grass, trees, snow
	- sea, mountain:
	-- turn non-volcano to volcano
	-- if volcano, erupt (raise sea, erode mountain) */
	void ChangeFire (bool direct) {
		if (elevation == 0) {
			if (terrain == 1) {
				RemoveFeatures();
				Elevate();
			} else {
				terrain = 1;
			}
		} else if (elevation == 4) {
			if (terrain == 1) {
				RemoveFeatures();
				Erode();
			} else {
				terrain = 1;
			}
		} else {
			terrain = 0;
			if (detail != 3 || !direct) detail = 0;
		}
	}
	/* ICE:
	- valley, plain, hill:
	-- add snow
	-- if snow + trees, remove trees
	-- if snow + no trees, erode tile (disabled)
	- sea, mountain:
	-- turn non-ice to ice */
	void ChangeIce (bool direct) {
		if (elevation == 0) {
			terrain = 2;
		} else if (elevation == 4) {
			terrain = 2;
		} else {
			if (terrain == 2) {
				// if (detail == 0) Erode();
				if (detail != 3 || !direct) detail = 0;
			} else {
				terrain = 2;
				if (detail == 3) {
					if (!direct) detail = 0;
				} else if (detail == 2) {
					detail = 0;
				}
			}
		}
	}
	/* PLANT:
	- valley, plain, hill:
	-- add grass
	-- if grass, add trees
	- sea:
	-- raise tile, add grass */
	void ChangePlant (bool direct) {
		if (elevation == 0) {
			terrain = 1;
			Elevate();
		} else if (elevation < 4) {
			if (terrain == 1) {
				if (detail != 3 || !direct) detail = 1;
			} else {
				terrain = 1;
				if (detail == 2) detail = 0;
				if (detail == 3 && !direct) detail = 0;
			}
		}
	}
	/* STONE:
	- sea:
	-- raise tile
	- valley, plain, hill:
	-- if water, remove water
	-- if no water, raise tile
	- mountain:
	-- remove ice, volcano */
	void ChangeStone (bool direct) {
		if (elevation == 4) {
			terrain = 0;
		} else if (elevation == 0) {
			RemoveFeatures();
			Elevate();
		} else if (detail == 3) {
			if (!direct) detail = 0;
		} else if (detail == 2) {
			detail = 0;
		} else if (water != 0) {
			water = 0;
		} else {
			Elevate();
		}
	}
	/* WATER:
	- sea:
	-- remove ice, volcano
	- valley, plain, hill:
	-- if no water, add water
	-- if water, erode tile
	- mountain:
	-- erode tile, add water */
	void ChangeWater (bool direct) {
		if (elevation == 0) {
			terrain = 0;
		} else if (elevation == 4) {
			RemoveFeatures();
			Erode();
			water = 1;
		} else {
			if (detail == 3) {
				if (!direct) {
					detail = 0;
					water = 1;
				}
			} else if (detail == 2) {
				detail = 0;
				water = 1;
			} else if (water != 1) {
				water = 1;
			} else {
				Erode();
			}
		}
	}
	
	/* Returns true if element causes a cascade */
	public bool Cascade (Element elem) {
		switch (elem) {
			case Element.AIR:	// Air + Sea = Whirlwind
				if (elevation == 0 && terrain == 0) return true;
				break;
				
			case Element.FIRE:	// Fire + Volcano = Eruption
				if (ElevationCap() && terrain == 1) return true;
				break;
				
			case Element.ICE:	// Ice + Ice cap = Blizzard
				if (ElevationCap() && terrain == 2) return true;
				break;
				
			case Element.PLANT:	// Plant + Trees = Overgrowth
				if (terrain == 1 && detail == 1) return true;
				break;
				
			case Element.STONE:	// Stone + Mountain = Landslide
				if (elevation == 4 && terrain == 0) return true;
				break;
				
			case Element.WATER:	// Water + Lake = Flood
				if (elevation == 3 && water == 1) return true;
				break;
		}
		return false;
	}
}