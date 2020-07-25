using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
	Rigidbody _rigidbody;
	Collider _collider;

	public float moveSpeed;
	public int team;
	[Range (1, 10)]
	public int boost;

	private Vector3 moveDirection;
	Transform root;

	void Start ()
	{
		_rigidbody = GetComponent<Rigidbody> ();
		_collider = GetComponent<Collider> ();
		root = transform.Find ("Root");

	}


	void Update ()
	{

		moveDirection = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized;
	}


	void FixedUpdate ()
	{

		if (Input.GetKey ("space")) {
			GetComponent<Rigidbody> ().MovePosition (GetComponent<Rigidbody> ().position + transform.TransformDirection (moveDirection) * (moveSpeed * boost) * Time.deltaTime);
		} else {
			root.localRotation = Quaternion.LookRotation (moveDirection, Vector3.up);
			GetComponent<Rigidbody> ().MovePosition (GetComponent<Rigidbody> ().position + transform.TransformDirection (moveDirection) * moveSpeed * Time.deltaTime);

		}
	}

}
