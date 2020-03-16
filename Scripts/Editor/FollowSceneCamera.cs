using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class FollowSceneCamera : MonoBehaviour {

	private Vector3 lastPosition;
	private Quaternion lastRotation;

	void OnEnable() {
		lastPosition = transform.position;
		lastRotation = transform.rotation;
	}

	void OnDisable() {
		transform.position = lastPosition;
		transform.rotation = lastRotation;
	}
	
	void OnRenderObject () {
		if (SceneView.currentDrawingSceneView != null) {
			transform.position = SceneView.currentDrawingSceneView.camera.transform.position;
			transform.rotation = SceneView.currentDrawingSceneView.camera.transform.rotation;
		}
	}
}