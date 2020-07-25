using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotator3 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    void Update()
    {
        transform.Rotate(new Vector3(-15, -30, -45), Time.deltaTime * 5);
    }
}
