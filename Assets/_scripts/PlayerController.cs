using UnityEngine;
using Mirror;
using spacejam;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
	Rigidbody _rigidbody;
	Collider _collider;
	PlayerStats _player;
	Light _light;
	Transform root;
	public int teamID;

	[SyncVar]
	public Color color;
	[SyncVar]
	public string playerName;

	public float moveSpeed;


	public float boost;
	bool isBoosting;
	public float boostTime;
	public float boostCooldown;

	private Vector3 moveDirection;

	float outRadius, outPolar, outElevation;
	public GameObject ownedBase;

	//NetworkPlayer netPlayer;

	void Start ()
	{
		_rigidbody = GetComponent<Rigidbody> ();
		_collider = GetComponent<Collider> ();
		_player = GetComponent<PlayerStats> ();
		// _light = GetComponentInChildren<Light>();            

		Renderer rend = GetComponentInChildren<Renderer> ();
		root = transform.Find ("Root");
                        
		rend.material.SetColor ("_EmissionColor", color);
		rend.material.SetColor ("_Color", color);

        
		//teamID = (int)netId.Value;

		Text text = GameObject.FindGameObjectWithTag ("player " + teamID + " HP GUI").GetComponent<Text> ();
		text.color = color;
        

		//_light.color = _player.color;

        
		if (isLocalPlayer) {
			Camera.main.transform.SetParent (transform.Find ("Main Camera Root"), false);
		}

		//We don't want to handle collision on client, so disable collider there
		//_collider.enabled = isServer;
	}

	public void ResetSpawn (Transform baseTransform)
	{
		transform.position = baseTransform.position;
	}

	[ClientCallback]
	void Update ()
	{
//		if (!isLocalPlayer) {
//			var camera = GetComponentInChildren<Camera> ();
//			var rig = GetComponentInChildren<cameraRig> ();
//			if (camera != null && rig != null) {
//				camera.gameObject.SetActive (false);
//				rig.gameObject.SetActive (false);
//				return;
//			}
//		}

		if (isLocalPlayer) {
			moveDirection = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized;
		}
	}

	[ClientCallback]
	void FixedUpdate ()
	{
		if (!hasAuthority)
			return;


		if (Input.GetKey ("space")) {               
			if (!isBoosting) {
				StartCoroutine (Boost ());
			}                  
		} 


		if (moveDirection.sqrMagnitude > 0) {
			root.localRotation = Quaternion.LookRotation (moveDirection, Vector3.up);
		}
		//_rigidbody.MovePosition (_rigidbody.position + transform.TransformDirection (moveDirection) * moveSpeed * Time.deltaTime);
		_rigidbody.AddForce (transform.TransformDirection (moveDirection) * moveSpeed);
	}

	IEnumerator Boost ()
	{
		isBoosting = true;
		float timeCooldown = Time.time + boostTime;
		float originalSpeed = moveSpeed;
		while (Time.time < timeCooldown) {
			if (Input.GetKeyUp ("space")) {
				break;
			}
			moveSpeed = boost;
			yield return 0;
		}
		moveSpeed = originalSpeed;

		yield return new WaitForSeconds (boostCooldown);
		isBoosting = false;
	}
}


