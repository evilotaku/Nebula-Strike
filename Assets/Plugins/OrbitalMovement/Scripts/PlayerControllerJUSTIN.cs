using UnityEngine;
using Mirror;
using spacejam;

public class PlayerControllerJUSTIN : NetworkBehaviour
    {
        Rigidbody _rigidbody;
        Collider _collider;
        PlayerStats _player;
        Light _light;

        public float moveSpeed;

        [Range(1, 10)]
        public int boost;

        private Vector3 moveDirection;

        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _player = GetComponent<PlayerStats>();
            _light = GetComponentInChildren<Light>();            

            Renderer rend = GetComponent<Renderer>();

            if(!_player)
                print("PlayerStats not found!");
            
            rend.material.SetColor("_EmissionColor", _player.color);
            rend.material.SetColor("_Color", _player.color);
            _light.color = _player.color;


            //We don't want to handle collision on client, so disable collider there
            _collider.enabled = isServer;
        }

        [ClientCallback]
        void Update()
        {
            if(!isLocalPlayer)
                return;

            moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }

        [ClientCallback]
        void FixedUpdate()
        {
            if (!hasAuthority)
                return;

            if (Input.GetKey("space"))
            {
                GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + transform.TransformDirection(moveDirection) * (moveSpeed * boost) * Time.deltaTime);
            }
            else
            {
                GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);

            }
        }

        
    }


