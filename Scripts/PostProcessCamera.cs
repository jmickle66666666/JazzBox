using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessCamera : MonoBehaviour {

	public Material postProcessMaterial;

	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		if (postProcessMaterial != null) {
			source.filterMode = FilterMode.Point;
			Graphics.Blit (source, destination, postProcessMaterial);
		} else {
			Graphics.Blit (source, destination);
		}
	}
}
