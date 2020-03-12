using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

static class Extensions {
	public static void Normalize( this float[] fArr ) {
		float sum = fArr.Sum();
		for( int i = 0; i < fArr.Length; i++ )
			fArr[i] /= sum;
	}
	public static T GetWrapped<T>( this T[] arr, int i ) {
		return arr[(i % arr.Length + arr.Length) % arr.Length];
	}
}

struct PointWeight {
	public float[] weights;

	public PointWeight( Vector3 point, Vector3[] refPts ) {
		int count = refPts.Length;
		weights = new float[count];
		for( int i = 0; i < count; i++ ) {
			float w = CalcMeanValueWeight( point, refPts.GetWrapped(i-1), refPts[i], refPts.GetWrapped(i+1) );
			weights[i] = w;
		}
		weights.Normalize();
	}

	// The general solution to this is called generalized barycentric coordinates
	// There's a small section on it here: https://en.wikipedia.org/wiki/Barycentric_coordinate_system#Generalized_barycentric_coordinates
	// The coordinates we use here is called Mean Value coordinates
	// There are other ones that seems to not handle non-convex reference shapes well, so, this one is probably best
	// More info: https://www.slideshare.net/teseoch/bijective-composite-mean-value-mappings
	float CalcMeanValueWeight( Vector3 pt, Vector3 prev, Vector3 mid, Vector3 next ) {
		Vector3 pToPrev = prev - pt;
		Vector3 pToMid  = mid  - pt;
		Vector3 pToNext = next - pt;
		float distToMid  = pToMid.magnitude;
		float angRadPrev = Vector3.Angle( pToMid, pToPrev ) * Mathf.Deg2Rad;
		float angRadNext = Vector3.Angle( pToMid, pToNext ) * Mathf.Deg2Rad;
		return ( Mathf.Tan( angRadPrev * 0.5f ) + Mathf.Tan( angRadNext * 0.5f ) ) / distToMid;
	}

	public Vector3 CalcDistortedPoint( Vector3[] refFrameVerts ) {
		Vector3 output = new Vector3();
		for( int i = 0; i < refFrameVerts.Length; i++ )
			output += refFrameVerts[i] * weights[i];
		return output;
	}
}

[ExecuteInEditMode]
public class Distorter : MonoBehaviour {
	Vector3[] refFrameStart;
	Vector3[] refFrameCurrent {
		get {
			Vector3[] output = new Vector3[transform.childCount];
			for( int i = 0; i < transform.childCount; i++ ) {
				output[i] = transform.GetChild( i ).position;
			}
			return output;
		}
	}

	void OnEnable() => Reset();

	// this is a special unity function, you can right-click the component and press reset to call this
	void Reset() {
		refFrameStart = refFrameCurrent;
	}

    public Vector3 TransformPoint(Vector3 input) {
        float yPos = input.y;
        input.y = 0f;
        var output = new PointWeight( input, refFrameStart ).CalcDistortedPoint(refFrameCurrent);
        output.y = yPos;
        return output;
    }

	void OnDrawGizmos() {
		Gizmos.color = Color.cyan;
		for( int i = 0; i < transform.childCount; i++ ) {
			Gizmos.DrawLine( transform.GetChild( i ).position, transform.GetChild( ( i + 1 ) % transform.childCount ).position );
		}
	}

}