using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketDetectionScript : MonoBehaviour
{
	//public float detectionDistance = 0.2f;
	public float xWidth, yWidth, zWidth;
	public float scoreCooldown = 0.25f;

	private GameObject ball;
	Vector3 minCorner, maxCorner;
	//private float distSqr;
	private float timer;
	private bool waiting;

	// Start is called before the first frame update
	void Start()
	{
		ball = GameObject.FindGameObjectWithTag("Ball");
		//distSqr = detectionDistance * detectionDistance;
		minCorner = new Vector3(transform.position.x - xWidth * 0.5f, transform.position.y - yWidth * 0.5f, transform.position.z - zWidth * 0.5f);
		maxCorner = new Vector3(transform.position.x + xWidth * 0.5f, transform.position.y + yWidth * 0.5f, transform.position.z + zWidth * 0.5f);
	}

	// Update is called once per frame
	void Update()
	{
		if (MadeBasket() && !waiting)
		{
			timer = scoreCooldown;
			waiting = true;
			GameManager.instance.ScorePoint();
		}

		if (waiting)
		{
			timer -= Time.deltaTime;
			if (timer <= 0f)
				waiting = false;
		}
		minCorner = new Vector3(transform.position.x - xWidth * 0.5f, transform.position.y - yWidth * 0.5f, transform.position.z - zWidth * 0.5f);
		maxCorner = new Vector3(transform.position.x + xWidth * 0.5f, transform.position.y + yWidth * 0.5f, transform.position.z + zWidth * 0.5f);
		DebugDrawBounds();
	}

	bool MadeBasket()
	{
		Vector3 ballPos = ball.transform.position;

		if ((ballPos.x <= maxCorner.x && ballPos.y <= maxCorner.y && ballPos.z <= maxCorner.z) &&
			(ballPos.x >= minCorner.x && ballPos.y >= minCorner.y && ballPos.z >= minCorner.z))
			return true;
		else
			return false;

		/*Vector3 diff = transform.position - ball.transform.position;
		
		if (diff.sqrMagnitude < distSqr)
			return true;
		else
			return false;
		*/
	}

	void DebugDrawBounds()
	{
		//Vector3 pos = transform.position;
		Vector3[] corners = {
					minCorner,
					new Vector3(minCorner.x, minCorner.y, maxCorner.z),
					new Vector3(minCorner.x, maxCorner.y, maxCorner.z),
					new Vector3(minCorner.x, maxCorner.y, minCorner.z),
					maxCorner,
					new Vector3(maxCorner.x, minCorner.y, maxCorner.z),
					new Vector3(maxCorner.x, minCorner.y, minCorner.z),
					new Vector3(maxCorner.x, maxCorner.y, minCorner.z)
				};

		Color col = Color.blue;
		Debug.DrawLine(corners[0], corners[1], col);
		Debug.DrawLine(corners[1], corners[2], col);
		Debug.DrawLine(corners[2], corners[3], col);
		Debug.DrawLine(corners[3], corners[0], col);
		Debug.DrawLine(corners[0], corners[6], col);
		Debug.DrawLine(corners[4], corners[5], col);
		Debug.DrawLine(corners[5], corners[6], col);
		Debug.DrawLine(corners[6], corners[7], col);
		Debug.DrawLine(corners[7], corners[4], col);
	}
}