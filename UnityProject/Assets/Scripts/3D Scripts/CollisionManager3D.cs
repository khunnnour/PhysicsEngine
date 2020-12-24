using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager3D : MonoBehaviour
{
	public float checkRadius = 5f;

	[System.Serializable]
	public struct CollisionPair
	{
		public GameObject one;
		public GameObject two;

		public CollisionPair(GameObject a, GameObject b) { one = a; two = b; }
	}

	public List<CollisionPair> collisionPairs = new List<CollisionPair>();
	//public CollisionPair testPair;

	CollisionHull3D.Collision col;

	GameObject[] environment;
	GameObject ball;
	//GameObject paddle;

	// Start is called before the first frame update
	void Start()
	{
		col = new CollisionHull3D.Collision();
		environment = GameObject.FindGameObjectsWithTag("Environment");
		ball = GameObject.FindGameObjectWithTag("Ball");
		//paddle = GameObject.FindGameObjectWithTag("Paddle");

	}

	// Update is called once per frame
	void Update()
	{
		// Only update if not finished
		if (GameManager.instance.GetGameRunning())
		{
			// Not really implemented right
			BroadCollisionSearch();
			NarrowCollisionCheck();
		}
	}

	void RespondToCollision(ref CollisionHull3D.Collision col)
	{
		if (col.status)
		{
			col.contacts[0].restitution = 0.95f;
			col.a.GetComponent<Renderer>().material.color = Color.green;
			col.contacts[0].ResolveVelocity(0.1f, col.b.transform.up, col.a.GetParticle3D(), col.b.GetParticle3D());
			col.contacts[0].ResolveInterpenetration(0.1f, col.a.GetParticle3D(), col.b.GetParticle3D());
		}
		else
			col.a.GetComponent<Renderer>().material.color = Color.white;
	}

	private void NarrowCollisionCheck()
	{
		/* *
		 * 1. Move through the heirarchy
		 * 2. Check all potential collisions
		 * */
		
		//collisionPairs.Add(testPair);
		for (int i = 0; i < collisionPairs.Count; i++)
		{
			CollisionHull3D.TestCollision(collisionPairs[i].one.GetComponent<CollisionHull3D>(), collisionPairs[i].two.GetComponent<CollisionHull3D>(), ref col);

			RespondToCollision(ref col);
		}
		
		// Clear after check
		collisionPairs.Clear();
	}
	private void BroadCollisionSearch()
	{
		/* *
		 * 1. Create a bonding sphere for one particle
		 * 2. Create a new BS for another particle
		 * 3. Create a big BS for the two partciles
		 * 4. Contine through all particles parenting like this
		 * */

		collisionPairs.Clear();
		// Add all ground within the search bounds
		for (int i = 0; i < environment.Length; i++)
		{
			Vector2 diff = environment[i].transform.position - ball.transform.position;
			if (diff.sqrMagnitude < checkRadius * checkRadius)
			{
				//environment[i].GetComponent<Renderer>().material.color = new Color(211f, 84f, 0f);
				CollisionPair temp = new CollisionPair(ball, environment[i]);
				collisionPairs.Add(temp);
			}
		}

		//CollisionPair padPair = new CollisionPair(ball, paddle);
		//collisionPairs.Add(padPair);
	}
}
