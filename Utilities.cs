using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
	/* Given an array of N weights, return a random value from 0 to N-1 */
    public static int WeightedRandom (float[] weights) {
		if (weights == null || weights.Length == 0) return -1;
		
		float sum = 0;
		foreach (float w in weights) {
			if (w >= 0) sum += w;
		}
		float r = Random.value * sum;
		for (int i = 0; i < weights.Length; i++) {
			if (weights[i] <= 0) continue;
			
			if (r <= weights[i]) return i;
			else r -= weights[i];
		}
		
		return -1;
	}
}
