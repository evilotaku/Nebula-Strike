using UnityEngine;
using UnityEngine.Networking;

public class CameraController : MonoBehaviour {

    public GameObject cameraRig;

    [Range(1f, 10f)]
    public float time; 

        // Use this for initialization
	void Start () 
        {
                //cameraRig = GameObject.FindGameObjectWithTag("Camera Rig"); 
	}
	
	// Update is called once per frame
	void Update () 
        {
                cameraRig = GameObject.FindGameObjectWithTag("Camera Rig"); 
                transform.position = Vector3.Lerp(transform.position, cameraRig.transform.position, time);
                //transform.forward = -GameObject.FindGameObjectWithTag("Player").transform.position; 
                transform.LookAt(transform.parent);
	}
}
