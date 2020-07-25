using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army : MonoBehaviour
{
	public float moveSpeed;
	public float initialAngle;
	public int team = 0;
	public bool isAttack;
	public Vector3 target;
	public Transform targetTransform;
	public float arrivalDistance;
	public float explosionRadius;
	public LayerMask explosionMask;
	public int explosionDamage;
	public int healAmount;

	Transform rightFeeler;
	Transform leftFeeler;
	Transform frontFeeler;
	public GameObject ps;
	public GameObject deathEffect;
	public GameObject salvagePrefab;
	public float randomWanderAngle;
	bool isHealing = false;
	// Use this for initialization
	void Start ()
	{

		rightFeeler = transform.GetChild (0).Find ("Right Feeler");
		leftFeeler = transform.GetChild (0).Find ("Left Feeler");
		frontFeeler = transform.GetChild (0).Find ("Center Feeler");

		GameObject g = GameObject.FindGameObjectWithTag ("Planet");
		GravityBody[] bodies = transform.GetComponentsInChildren<GravityBody> ();
		foreach (GravityBody body in bodies) {
			body.attractor = g.GetComponentInChildren<GravityAttractor> ();
		}

		if (isAttack) {
			transform.GetChild (0).Rotate (0, initialAngle, 0);
		}
	}


	public void Die ()
	{
		GameObject deathGO = GameObject.Instantiate (deathEffect);
		deathGO.transform.position = transform.position;
		if (salvagePrefab != null) {
			GameObject salvage = GameObject.Instantiate (salvagePrefab);
			salvage.transform.position = transform.position;
		}

		GameObject.Destroy (deathGO, 2);
		GameObject.Destroy (gameObject);
	}

	void FixedUpdate ()
	{


		if (isAttack) {
			if (targetTransform != null) {
				target = targetTransform.position;
				float distance = Vector3.Distance (transform.position, target);
				if (distance < arrivalDistance) {
					GameObject psO = Instantiate (ps);
					GameObject.Destroy (psO, 2);
					psO.transform.position = transform.position;


					//Area of effect destruction
					Collider[] cols = Physics.OverlapSphere (transform.position, explosionRadius, explosionMask.value);
					foreach (Collider c in cols) {
						print (c.gameObject.name);
						if (c.gameObject.name == "Root" && c.gameObject.transform != gameObject.transform) {
							c.gameObject.GetComponentInParent<Army> ().Die ();
						} else if (c.gameObject.tag == "Base") {
							c.gameObject.GetComponent<Base> ().TakeDamage (explosionDamage);
						}
					}
					GameObject.Destroy (gameObject);
					return;
				}
				//Rotate to target
				target = targetTransform.position;
				RotateToTarget ();
				GetComponent<Rigidbody> ().MovePosition (GetComponent<Rigidbody> ().position + (transform.GetChild (0).TransformDirection (Vector3.forward) * moveSpeed * Time.deltaTime));
			} else {
				//Add a little wander
				transform.GetChild (0).Rotate (0, randomWanderAngle, 0);
				GetComponent<Rigidbody> ().MovePosition (GetComponent<Rigidbody> ().position + (transform.GetChild (0).TransformDirection (Vector3.forward) * moveSpeed * Time.deltaTime));

			}
		} else {
			//Find shortest direction to base
			if (Vector3.Distance (transform.position, target) > arrivalDistance) {
				RotateToTarget ();
				GetComponent<Rigidbody> ().MovePosition (GetComponent<Rigidbody> ().position + (transform.GetChild (0).TransformDirection (Vector3.forward) * moveSpeed * Time.deltaTime));
			} else {
				RotateToTargetAbsolute ();
				if (!isHealing) {
					isHealing = true;
					GameObject psO = Instantiate (ps);
					psO.transform.SetParent (transform.GetChild (0), false);
					psO.transform.localPosition = new Vector3 (0, 0, 0);
					StartCoroutine (Heal (psO));
				}

			}
		}

	}

	IEnumerator Heal (GameObject psO)
	{

		ParticleSystem healBeam = psO.GetComponent<ParticleSystem> ();
		while (gameObject.transform != null) {
			healBeam.Emit (100);
			yield return new WaitForSeconds (4f);

			Base targetBase;
			BaseSpawner.instance.GetBase (team, out targetBase);
			targetBase.Heal (healAmount);

		}

	}

	void RotateToTargetAbsolute ()
	{
		transform.GetChild (0).rotation = Quaternion.LookRotation (target - transform.GetChild (0).position, Vector3.up);
	}

	void RotateToTarget ()
	{
		float minDistance = 100000;
		Transform feeler;
		minDistance = Vector3.Distance (rightFeeler.position, target);
		feeler = rightFeeler;
		float distance = Vector3.Distance (leftFeeler.position, target);
		if (distance < minDistance) {
			minDistance = distance;
			feeler = leftFeeler;
		}
		distance = Vector3.Distance (frontFeeler.position, target);
		if (distance < minDistance) {
			minDistance = distance;
			feeler = frontFeeler;
		}
		if (feeler.name == "Left Feeler") {
			transform.GetChild (0).Rotate (0, -1, 0);
		} else if (feeler.name == "Right Feeler") {
			transform.GetChild (0).Rotate (0, 1, 0);
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (!isAttack) {
			return;
		}
		Army unit = other.gameObject.GetComponentInParent<Army> ();

		if (unit != null) {
			if (unit.team != team && !unit.isAttack) {
				targetTransform = unit.transform;
				moveSpeed = moveSpeed * 1.20f;
				print ("Attacking defensive unit!");
			}
		} else {
			Base baseC = other.gameObject.GetComponent<Base> ();
			if (baseC != null && baseC.team != team) {
				targetTransform = baseC.transform;
				print ("Attacking enemy base!");
			}

		}



	}
}
