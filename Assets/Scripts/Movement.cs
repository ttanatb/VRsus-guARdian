using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Movement : NetworkBehaviour
{
	Player player;

    Rigidbody rigidBody;
    float jumpValue = 0;
    float lerpTarget = 0;

    const float JUMP_FACTOR = 2;
    const float JUMP_DAMPING = 0.2f;

    const float MOVEMENT_SPEED = 0.025f;
    const float TURNING_SPEED = 5f;

    // Use this for initialization
    void Awake()
    {
		player = GetComponent<Player> ();
    }

	public override void OnStartLocalPlayer()
	{
		if (player.PlayerType == PlayerType.VR) {
			rigidBody = gameObject.AddComponent<Rigidbody> ();
			rigidBody.mass = 1;
			rigidBody.drag = 10;
			rigidBody.useGravity = false;
			rigidBody.isKinematic = false;
			rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		}
		base.OnStartLocalPlayer();
	}

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

		if (rigidBody){
			transform.Translate (transform.forward * Input.GetAxis ("Vertical") * MOVEMENT_SPEED, Space.World);
			transform.Rotate (transform.up, Input.GetAxis ("Horizontal") * TURNING_SPEED, Space.World);

			if (Input.GetKeyDown (KeyCode.Space))
				lerpTarget = JUMP_FACTOR;

			jumpValue = Mathf.Lerp (jumpValue, lerpTarget, Time.deltaTime * 20);

			if (lerpTarget > 0)
				lerpTarget -= JUMP_DAMPING;
			else
				lerpTarget = 0;
		} else if (player.PlayerType == PlayerType.AR) {
			//transform.position = Camera.main.transform.position;
			//transform.rotation = Camera.main.transform.rotation;
		}
    }

    private void FixedUpdate()
    {
		if (rigidBody) {
			rigidBody.velocity += Vector3.up * jumpValue;
		}
	}
}
