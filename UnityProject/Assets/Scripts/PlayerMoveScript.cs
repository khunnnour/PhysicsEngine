using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveScript : MonoBehaviour
{
	public Vector3 minBounds, maxBounds;
	public float speed = 5.0f;

	private float xMove, yMove;

	// Start is called before the first frame update
	void Start()
	{
		xMove = 0f;
		yMove = 0f;
	}

	// Update is called once per frame
	void Update()
	{
		GetInput();

		Move();

		CheckBounds();
	}

	private void GetInput()
	{
		xMove = Input.GetAxis("Horizontal");
		yMove = Input.GetAxis("Vertical");
	}

	private void Move()
	{
		Vector3 moveDir = transform.forward * yMove;
		moveDir += transform.right * xMove;
		moveDir.y = 0.0f;

		moveDir.Normalize();

		transform.position = Vector3.Lerp(transform.position, transform.position + moveDir * 2f, Time.deltaTime * speed);
	}

	private void CheckBounds()
	{
		Vector3 newPos = transform.position;

		if (transform.position.x < minBounds.x)
			newPos.x = minBounds.x;
		else if (transform.position.x > maxBounds.x)
			newPos.x = maxBounds.x;

		if (transform.position.y < minBounds.y)
			newPos.y = minBounds.y;
		else if (transform.position.y > maxBounds.y)
			newPos.y = maxBounds.y;

		if (transform.position.z < minBounds.z)
			newPos.z = minBounds.z;
		else if (transform.position.z > maxBounds.z)
			newPos.z = maxBounds.z;

		transform.position = newPos;
	}
}
