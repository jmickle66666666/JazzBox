using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayer : MonoBehaviour {

	CharacterController characterController;
	public float moveSpeed;

	private Vector3 movement;
	public float gravity;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		characterController = GetComponent<CharacterController>();
		movement = new Vector3();
	}
	
	// Update is called once per frame
	void Update () {
		movement.Set(1,1,1);
		movement.Scale(
			Input.GetAxisRaw("Horizontal") * transform.right + Input.GetAxisRaw("Vertical") * transform.forward
		);
		
		characterController.Move(
			movement * moveSpeed * Time.deltaTime
		);

		characterController.Move(Vector3.down * gravity * Time.deltaTime);
	}
}
