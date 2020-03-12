using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayer : MonoBehaviour {

	CharacterController characterController;
	public float moveSpeed;

	private Vector3 movement;

	// Use this for initialization
	void Start () {
		characterController = GetComponent<CharacterController>();
		movement = new Vector3();
	}
	
	// Update is called once per frame
	void Update () {
		movement.Set(1,1,1);
		movement.Scale(
			Input.GetAxisRaw("Horizontal") * Camera.main.transform.right + Input.GetAxisRaw("Vertical") * Camera.main.transform.forward
		);
		
		characterController.Move(
			movement * moveSpeed * Time.deltaTime
		);
	}
}
