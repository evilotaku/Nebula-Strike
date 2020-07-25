using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BaseSpawner : NetworkBehaviour
{

	public GameObject basePrefab;
	public float worldRadius;
	public Vector3 offset;
	public List<GameObject> bases;
	public static BaseSpawner instance;
	
	// Use this for initialization
	[Server]
	void Start ()
	{
		instance = this;
		bases = new List<GameObject> ();
		Spawn ();
	}

	
	public void GetBase (int id, out Base oBase)
	{
		oBase = null;
		foreach (GameObject b in bases) {
			if (b.GetComponent<Base> ().team == id) {
				oBase = b.GetComponent<Base> ();
			}
		}		
	}

	[ContextMenu ("Spawn"), Server]
	void Spawn ()
	{
		int playerCount = NetworkManager.singleton.numPlayers;
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		//Set player
		foreach (GameObject player in players) {
			//Get a random polar coordinate
			PlayerController controller = player.GetComponent<PlayerController> ();
			Vector3 position = Random.onUnitSphere * worldRadius;
			GameObject playerBase = Instantiate (basePrefab);
			playerBase.transform.position = position;
			Quaternion rotation = Quaternion.LookRotation (position.normalized);
			playerBase.transform.rotation = rotation;
			playerBase.transform.Translate (offset);
			playerBase.GetComponent<Base> ().team = controller.teamID;
			NetworkServer.Spawn (playerBase);
			bases.Add (playerBase);
			playerBase.transform.GetChild (0).name = "player " + controller.teamID + " spawn";
			controller.ResetSpawn (playerBase.transform.GetChild (0));
			controller.ownedBase = playerBase;
			//Set the base to the controller color
			MeshRenderer[] renderers = playerBase.GetComponentsInChildren<MeshRenderer> ();
			foreach (MeshRenderer renderer in renderers) {
				renderer.material.color = controller.color;
			}
		}


	}

	// Update is called once per frame
	void Update ()
	{
		
	}
}
