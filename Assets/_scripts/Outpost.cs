using System.Collections;
using System.Collections.Generic;
using spacejam;
using UnityEngine;

public class Outpost : MonoBehaviour
{

	public bool isAttack;
	public int team = 0;
	public int spawns = 5;
	public float delay;
	public float direction;
	public GameObject spawn;
	float randomWanderAngle;
	float initialAngle;
	PlayerController owner;

	// Use this for initialization
	void Start ()
	{
		randomWanderAngle = Random.Range (-.1f, .1f);
		initialAngle = Random.Range (0f, 360f);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnTriggerEnter (Collider other)
	{
		if (team != 0) {
			//Already captured
			return;
		}

		if (other.gameObject.tag == "Player") {
			team = other.gameObject.GetComponent<PlayerController> ().teamID;

			//Change Color
			MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer> ();
			owner = other.gameObject.GetComponent<PlayerController> ();
			foreach (MeshRenderer renderer in renderers) {
				renderer.material.color = owner.color;
			}
			StartCoroutine (SpawnArmies ());
		}
	}

	IEnumerator SpawnArmies ()
	{
		int currentWave = 0;
		int direction = Random.Range (0, 4);
		while (currentWave < spawns) {
			//spawn
			currentWave += 1;
			GameObject army = Instantiate (spawn);
			Transform spointPoint = transform.Find ("Spawn Point");
			army.transform.position = spointPoint.position;
			Army armyC = army.GetComponent<Army> ();

			//Change Color
			MeshRenderer[] renderers = army.GetComponentsInChildren<MeshRenderer> ();
			foreach (MeshRenderer renderer in renderers) {
				renderer.material.color = owner.color;
			}

			armyC.team = team;
			if (isAttack) {
				armyC.randomWanderAngle = randomWanderAngle;
				armyC.initialAngle = initialAngle;
				armyC.isAttack = true;
			} else {
				//Go to base to defend it.
				armyC.isAttack = false;
				Base targetBase = owner.ownedBase.GetComponentInParent<Base> ();
				armyC.target = targetBase.transform.position;
				
			}
			yield return new WaitForSeconds (delay);
		}

		if (isAttack) {
			OutpostSpawner.instance.attackCount -= 1;
		} else {
			OutpostSpawner.instance.defenseCount -= 1;
		}
		GameObject.Destroy (gameObject);
	}
}
