using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OutpostSpawner : NetworkBehaviour
{

	public GameObject attackOutpost;
	public GameObject defenseOutpost;
	public int maxOutposts;
	public int attackCount;
	public int defenseCount;
	public float minWait;
	public float maxWait;
	public static OutpostSpawner instance;
	public float worldRadius;
	public Vector3 offset;

	bool keepGoing = true;

	void Awake ()
	{
		instance = this;
	}

	// Use this for initialization
	[Server]
	void Start ()
	{
		StartCoroutine (Spawn ());
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void CreateOutpost (GameObject outpost)
	{
		Vector3 position = Random.onUnitSphere * worldRadius;
		GameObject playerBase = Instantiate (outpost);		
		playerBase.transform.position = position;
		Quaternion rotation = Quaternion.LookRotation (position.normalized);
		playerBase.transform.rotation = rotation;
		playerBase.transform.Translate (offset);
		NetworkServer.Spawn(playerBase);
	}

	IEnumerator Spawn ()
	{
		while (keepGoing) {
			if (attackCount + defenseCount >= maxOutposts) {
				//dont spawn anything
				yield return new WaitForSeconds (Random.Range (minWait, maxWait));
				continue;
			}

			if (attackCount < defenseCount) {
				attackCount += 1;
				CreateOutpost (attackOutpost);
			} else if (defenseCount < attackCount) {
				defenseCount += 1;
				CreateOutpost (defenseOutpost);
			} else {
				float flip = Random.Range (0f, 100f);
				if (flip < 50) {
					defenseCount += 1;
					CreateOutpost (defenseOutpost);
				} else {
					attackCount += 1;
					CreateOutpost (attackOutpost);
				}
			}
			yield return new WaitForSeconds (Random.Range (minWait, maxWait));
		}
	}
}
