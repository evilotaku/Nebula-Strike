using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotator2 : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(45, 15, 15), Time.deltaTime * 25); 
	}
}
