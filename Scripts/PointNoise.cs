using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PointNoise 
{
    public static Vector2[] BlueNoise( int count, int candidateCount = 20 ) {
        if (count == 0) {
            return new Vector2[0];
        }

        Vector2[] points = new Vector2[count];

        // Starting point
        points[0] = new Vector2( Random.value, Random.value );

        if (count == 1) {
            return points;
        }

        Vector2[] candidates = new Vector2[candidateCount];
        float[] distances = new float[candidateCount];

        for( int i = 1; i < count; i++ ) {
            // Build a list of candidates to pick from, and calculate their minimum distance from existing points
            for( int c = 0; c < candidateCount; c++ ) {
                candidates[c] = new Vector2( Random.value, Random.value );
                float minDist = float.MaxValue;
                for( int p = 0; p < i; p++ ) {
                    float d = Mathf.Abs( points[p].x - candidates[c].x ) + Mathf.Abs( points[p].y - candidates[c].y );
                    if( d < minDist )
                        minDist = d;
                }
                distances[c] = minDist;
            }

            // Find the best candidate to add
            int bestCandidate = 0;
            for( int j = 1; j < candidateCount; j++ ) {
                if( distances[bestCandidate] < distances[j] ) {
                    bestCandidate = j;
                }
            }

            points[i] = candidates[bestCandidate];
        }

        return points;
    }

    public static Vector2[] PoissonDiscNoise(int count, float discRange = .5f)
    {
        List<Vector2> output = new List<Vector2>();

        // Calculate the rough distance to use to place new points
        Vector2 diff = new Vector2(1f / Mathf.Sqrt(count), 1f / Mathf.Sqrt(count)) * 1.7f;

        // Random starting point
        output.Add(new Vector2(Random.value, Random.value));
        for (int i = 0; i < count; i++)
        {
            var candidate = Vector2.zero;

            bool valid = false;
            int life = 1000; // Failsafe if no valid candidate is found!
            while (!valid && life > 0) {
                life--;

                // Place a candidate nearby to an existing point
                Vector2 candidatePosition = output[Random.Range(0, output.Count)];
                candidate.Set(candidatePosition.x, candidatePosition.y);
                candidate.x += (Random.value - .5f) * diff.x;
                candidate.y += (Random.value - .5f) * diff.y;

                // Make sure it's in the area
                if (candidate.x < 0f || candidate.x >= 1f || candidate.y < 0f || candidate.y > 1f) {
                    valid = false;
                    continue;
                }

                // Make sure it's far enough from all other points
                valid = true;
                foreach (var p in output)
                {
                    if (Mathf.Abs(p.x - candidate.x) + Mathf.Abs(p.y - candidate.y) < diff.magnitude * discRange) {
                        valid = false;
                        break;
                    }
                }
            }

            if (life > 1) {
                output.Add(candidate);
            }
        }

        return output.ToArray();
    }
}
