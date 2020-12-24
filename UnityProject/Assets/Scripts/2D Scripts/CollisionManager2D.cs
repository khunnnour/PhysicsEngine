using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager2D : MonoBehaviour
{
	[System.Serializable]
	public struct CollisionPair
	{
		public GameObject one;
		public GameObject two;
	}

	public CollisionPair[] collisionPairs;
	
	CollisionHull2D.Collision col;

	// Start is called before the first frame update
	void Start()
	{
		col = new CollisionHull2D.Collision();
	}

	// Update is called once per frame
	void Update()
	{
		for(int i=0;i<collisionPairs.Length;i++)
		{
			CollisionHull2D.TestCollision(collisionPairs[i].one.GetComponent<CollisionHull2D>(), collisionPairs[i].two.GetComponent<CollisionHull2D>(),ref col);
			RespondToCollision(ref col);
		}
		
		//BroadCollisionSearch();
		//NarrowCollisionCheck();
	}

	private void NarrowCollisionCheck()
	{
		/* *
		 * 1. Move through the heirarchy
		 * 2. Check all potential collisions
		 * */
	}

	private void BroadCollisionSearch()
	{
		/* *
		 * 1. Create a bonding sphere for one particle
		 * 2. Create a new BS for another particle
		 * 3. Create a big BS for the two partciles
		 * 4. Contine through all particles parenting like this
		 * */
	}

	void RespondToCollision(ref CollisionHull2D.Collision col)
	{
		if (col.status)
		{
			col.a.GetComponent<Renderer>().material.color = Color.green;
			col.contacts[0].ResolveVelocity(0.1f, col.a.GetParticle2D(), col.b.GetParticle2D());
			col.contacts[0].ResolveInterpenetration(0.1f, col.a.GetParticle2D(), col.b.GetParticle2D());
		}
		else
			col.a.GetComponent<Renderer>().material.color = Color.white;
	}

	float DotProd(Vector2 a, Vector2 b)
	{
		return (a.x * b.x) + (a.y * b.y);
	}
}