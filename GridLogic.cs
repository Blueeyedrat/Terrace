using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexDirection {
	NE, E, SE, SW, W, NW
}

public enum OrthoDirection {
	N, E, S, W
}

[System.Serializable]
public struct HexCoordinates {
	[SerializeField]
	private int x,y;

	public int X { get { return x; } }	// W-E coordinate
	public int Y { get { return y; } }	// NE-SW coordinate
	
	public HexCoordinates (int x, int y, int z) {
		this.x = x+z;
		this.y = y+z;
	}	// cubic
	public HexCoordinates (int x, int y) {
		this.x = x;
		this.y = y;
	}	// axial
	
	public override string ToString () {
		return "(" + X.ToString() + ", " + Y.ToString() + ")";
	}
	
	public static HexCoordinates operator+ (HexCoordinates a, HexCoordinates b) {
		return new HexCoordinates(a.X+b.X, a.Y+b.Y);
	}
	public static HexCoordinates operator- (HexCoordinates a, HexCoordinates b) {
		return new HexCoordinates(a.X-b.X, a.Y-b.Y);
	}
}

[System.Serializable]
public struct HexParameters {
	[SerializeField]
	private int a,b,c;

	public int A { get { return a; } }	// W-E axis
	public int B { get { return b; } }	// NE-SW axis
	public int C { get { return c; } }	// NW-SE axis

	public HexParameters (int a, int b, int c) {
		this.a = a;
		this.b = b;
		this.c = c;
	}
	
	public override string ToString () {
		return "(" + A.ToString() + ", " + B.ToString() + ", " + C.ToString() + ")";
	}
	
	public static HexParameters operator+ (HexParameters P, HexParameters Q) {
		return new HexParameters(P.A+Q.A-1, P.B+Q.B-1, P.C+Q.C-1);
	}
	public static HexParameters operator- (HexParameters P, HexParameters Q) {
		return new HexParameters(P.A-Q.A+1, P.B-Q.B+1, P.C-Q.C+1);
	}
}

public static class HexTools {
	
	public static HexDirection Opposite (this HexDirection d) {
		return (int)d < 3 ? (d + 3) : (d - 3);
	}
	
	/* Shifted coordinate */
	public static HexCoordinates Shift (this HexCoordinates C, int x, int y, int z) {
		return C + new HexCoordinates(x+z,y+z);
	}	// cubic
	public static HexCoordinates Shift (this HexCoordinates C, int x, int y) {
		return C + new HexCoordinates(x,y);
	}	// axial
	
	/* Rotated coordinate (positive = clockwise) */
	public static HexCoordinates Rotate (this HexCoordinates C, int r, HexCoordinates pivot) {
		return (C - pivot).Rotate(r) + pivot;
	}
	public static HexCoordinates Rotate (this HexCoordinates C, int r) {
		HexCoordinates c = C;
		if (r > 0) {
			for (int i = 0; i < (r % 6); i++) {
				c = new HexCoordinates(c.X-c.Y,c.X);
			}
		} else if (r < 0) {
			for (int i = 0; i < (-r % 6); i++) {
				c = new HexCoordinates(c.Y,c.Y-c.X);
			}
		}
		return c;
	}
	/* Rotated parameters */
	public static HexParameters Rotate (this HexParameters P, int r) {
		HexParameters p = P;
		if (r > 0) {
			for (int i = 0; i < (r % 3); i++) {
				p = new HexParameters(p.B, p.C, p.A);
			}
		} else if (r < 0) {
			for (int i = 0; i < (-r % 3); i++) {
				p = new HexParameters(p.C, p.A, p.B);
			}
		}
		return p;
	}
	
	/* Coordinate of hexagonal neighbor */
	public static HexCoordinates Neighbor (this HexCoordinates C, HexDirection d) {
		switch(d) {
			case HexDirection.NE:
				return C.Shift(0,-1);
			case HexDirection.E:
				return C.Shift(1,0);
			case HexDirection.SE:
				return C.Shift(1,1);
			case HexDirection.SW:
				return C.Shift(0,1);
			case HexDirection.W:
				return C.Shift(-1,0);
			case HexDirection.NW:
				return C.Shift(-1,-1);
			default:
				return C;
		}
	}
	
	/* Coordinate of orthogonal neighbor in hexagonal grid */
	static public bool orthoXZ = false;	// false = XY, true = XZ
	public static HexCoordinates MoveInBounds (this HexParameters P, HexCoordinates C, OrthoDirection d) {
		HexCoordinates[] neighbors;
		switch(d) {
			case OrthoDirection.N:
				if (orthoXZ) {
					neighbors = new HexCoordinates[] {
						C.Neighbor(HexDirection.NW),
						C.Neighbor(HexDirection.NE)
					};
				} else {
					neighbors = new HexCoordinates[] {
						C.Neighbor(HexDirection.NE),
						C.Neighbor(HexDirection.NW)
					};
				}
				break;
			case OrthoDirection.E:
				neighbors = new HexCoordinates[] {
					C.Neighbor(HexDirection.E),
					C.Neighbor(HexDirection.SE),
					C.Neighbor(HexDirection.NE)
				};
				break;
			case OrthoDirection.S:
				if (orthoXZ) {
					neighbors = new HexCoordinates[] {
						C.Neighbor(HexDirection.SE),
						C.Neighbor(HexDirection.SW)
					};
				} else {
					neighbors = new HexCoordinates[] {
						C.Neighbor(HexDirection.SW),
						C.Neighbor(HexDirection.SE)
					};
				}
				break;
			case OrthoDirection.W:
				neighbors = new HexCoordinates[] {
					C.Neighbor(HexDirection.W),
					C.Neighbor(HexDirection.NW),
					C.Neighbor(HexDirection.SW)
				};
				break;
			default:
				neighbors = new HexCoordinates[0];
				break;
		}
		foreach (HexCoordinates N in neighbors) {
			if (!P.OutOfBounds(N)) return N;
		}
		return C;
	}
	
	/* Number of rows in a grid */
	public static int Height (this HexParameters P) {
		return P.B + P.C - 1;
	}
	/* Number of cells in a grid */
	public static int GridSize (this HexParameters P) {
		return P.A*(P.B+P.C-1) + (P.B-1)*(P.C-1);
	}
	
	/* Build array of HexCoordinates corresponding to a convex grid
	First cell is (0,0); builds left-to-right, top-to-bottom */
	public static HexCoordinates[] BuildHexGrid (HexParameters P) {
		int gridSize = P.GridSize();
		HexCoordinates[] coordinates = new HexCoordinates[gridSize];
		
		int i = 0;
		int w = P.A;
		int h = P.Height();
		int offset = 0;
		int y = 0;
		while (y < h) {
			for (int x = 0; x < w; x++) {
				coordinates[i++] = new HexCoordinates( x + offset, y );
			}
			y++;
			if (y >= P.B) { w--; offset++; }
			if (y < P.C) { w++; }
		}
		return coordinates;
	}
	
	/* Returns true if all parameters are positive */
	public static bool ValidParameters (HexParameters P) {
		return (P.A > 0 && P.B > 0 && P.C > 0);
	}
	
	/* Fit a subgrid of size Q into a grid of size P */
	public static HexParameters FitRange (this HexParameters P, HexParameters Q) {
		HexParameters S = P - Q;
		int a = S.A, b = S.B, c = S.C;
		while (a > 0 && b > 0 && c <= 0) {
			a--; b--; c++;
		}
		while (a > 0 && b <= 0 && c > 0) {
			a--; b++; c--;
		}
		while (a <= 0 && b > 0 && c > 0) {
			a++; b--; c--;
		}
		return new HexParameters(a,b,c);
	}
	public static HexCoordinates FitOffset (this HexParameters P, HexParameters Q) {
		HexParameters S = P - Q;
		HexCoordinates fit = new HexCoordinates(0,0);
		int a = S.A, b = S.B, c = S.C;
		while (a > 0 && b > 0 && c <= 0) {
			a--; b--; c++;
		}
		while (a > 0 && b <= 0 && c > 0) {
			a--; b++; c--;
			fit = fit.Shift(1,0);
		}
		while (a <= 0 && b > 0 && c > 0) {
			a++; b--; c--;
			fit = fit.Shift(0,1);
		}
		return fit;
	}
	
	/* Returns true if coordinates C are invalid for grid size P */
	public static bool OutOfBounds (this HexParameters P, HexCoordinates C) {
		int w = P.A;
		int h = P.Height();
		if (C.Y < 0 || C.Y >= h) return true;
		int offset = 0;
		int y = 0;
		while (y < C.Y) {
			y++;
			if (y >= P.B) { w--; offset++; }
			if (y < P.C) { w++; }
		}
		if (C.X < offset || C.X >= w + offset) return true;
		return false;
	}
	
	/* Returns true if any coordinates in listC are invalid for grid size P */
	public static bool OutOfBounds (this HexParameters P, HexCoordinates[] listC) {
		return P.OutOfBounds(listC, new HexCoordinates(0,0));
	}
	public static bool OutOfBounds (this HexParameters P, HexCoordinates[] listC, HexCoordinates offset) {
		foreach (HexCoordinates C in listC) {
			if (P.OutOfBounds(C + offset)) return true;
		}
		return false;
	}
	
	/* Returns true if subgrid of size Q does not fit within grid size P */
	public static bool OutOfBounds (this HexParameters P, HexParameters Q) {
		return P.OutOfBounds(Q, new HexCoordinates(0,0));
	}
	public static bool OutOfBounds (this HexParameters P, HexParameters Q, HexCoordinates offset) {
		HexParameters S = P - Q;
		HexCoordinates fit = new HexCoordinates(0,0);
		
		int a = S.A, b = S.B, c = S.C;
		while (a > 0 && b > 0 && c <= 0) {
			a--; b--; c++;
		}
		while (a > 0 && b <= 0 && c > 0) {
			a--; b++; c--;
			fit = fit.Shift(1,0);
		}
		while (a <= 0 && b > 0 && c > 0) {
			a++; b--; c--;
			fit = fit.Shift(0,1);
		}
		S = new HexParameters(a,b,c);
		if (!ValidParameters(S)) return true;
		return S.OutOfBounds(offset + fit);
	}
	
	/* Finds valid coordinate from input coordinate */
	public static HexCoordinates FindInBounds (this HexParameters P, HexCoordinates C) {
		if (!ValidParameters(P)) return C;
		HexCoordinates c = C;
		int h = P.Height();
		while (P.OutOfBounds(c)) {
			if (c.Y < 0) c = c.Shift(0,1);
			else if (c.Y >= h) c = c.Shift(0,-1);
			else if (c.X < 0 || c.X - c.Y + P.B <= 0) c = c.Shift(1,0);
			else c = c.Shift(-1,0);
		}
		return c;
	}
	
	/* Returns index corresponding to input coordinates, or -1 if invalid */
	public static int IndexFromCoordinates (this HexParameters P, HexCoordinates C) {
		if (P.OutOfBounds(C)) return -1;
		
		int i = 0;
		int w = P.A;
		int offset = 0;
		int y = 0;
		while (y < C.Y) {
			y++;
			i += w;
			if (y >= P.B) { w--; offset++; }
			if (y < P.C) { w++; }
		}
		i += C.X - offset;
		return i;
	}
}