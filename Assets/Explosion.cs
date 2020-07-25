using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

	public Transform explosionPoint;
	public float force;
	public float radius;
	// Use this for initialization
	void Start ()
	{
		GameObject world = GameObject.FindGameObjectWithTag ("Planet");
		foreach (Transform t in transform) {
			t.GetComponent<GravityBody> ().attractor = world.GetComponent<GravityAttractor> ();
			t.GetComponent<Rigidbody> ().AddExplosionForce (force, explosionPoint.position, radius);
			t.parent = null;
		}
		Destroy (gameObject, .5f);
	}
	

}
