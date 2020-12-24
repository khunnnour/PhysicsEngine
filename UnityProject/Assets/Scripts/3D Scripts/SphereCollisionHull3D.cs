using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollisionHull3D : CollisionHull3D
{
	public SphereCollisionHull3D() : base(CollisionHullType3D.hull_sphere) { }
	public SphereCollisionHull3D(Particle3D p, float r) : base(CollisionHullType3D.hull_sphere)
	{
		particle = p;
		radius = r;
	}


	public bool debugDraw = false;

	[Range(0.0f, 100.0f)]
	public float radius = 0.5f;
	   
	// Update is called once per frame
	void Update()
	{
		if (debugDraw)
			DebugDrawHull(Color.green);
	}

	public override void ManualUpdate()
	{
		//if (debugDraw) DebugDrawHull(Color.green);
	}

	public override bool TestCollisionVsSphere(SphereCollisionHull3D other, ref Collision c)
	{
		/* *
		 * If dist between centers <= sum of radii
		 * Optimized: If dist^2 between centers <= (sum of radii)^2
		 * 1. Get 2 centers
		 * 2. Take the difference
		 * 3. dist^2 = dot(diff,diff)
		 * 4. Add the radii
		 * 5. Square the sum
		 * 6. Do the test
		 * Pass = dist^2 <= sumSq
		 * */

		// Step 1/2 - find difference between the 2 centers
		Vector3 diff = particle.GetPosition() - other.GetParticle3D().GetPosition();

		// Step 3 - get actual distance (squared) away
		float sqrdDist = diff.sqrMagnitude;

		// Step 4/5 - get distance needed for collision (squared) away
		float colDistSqr = radius + other.GetRadius();
		colDistSqr *= colDistSqr;

		// Step 6 - test
		if (sqrdDist <= colDistSqr)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			float pen = radius + other.GetRadius() - diff.magnitude;

			// Contact
			c.contactCount = 1;
			c.contacts[0].point = -diff.normalized * radius;
			c.contacts[0].penetration = pen;
			c.contacts[0].normal = diff.normalized;
			c.contacts[0].restitution = 1.0f;
		}
		else
		{
			// No collision
			c.status = false;
		}

		return c.status;
	}

	public override bool TestCollisionVsAABB(AABBCollisionHull3D other, ref Collision c)
	{
		/* *
		 * See AABB
		 * */

		other.TestCollisionVsSphere(this, ref c);

		c.contacts[0].normal *= -1.0f;

		return c.status;
	}
	
	public override bool TestCollisionVsOBB(OBBCollisionHull3D other, ref Collision c)
	{
		/* *
		* See OBB
		* */

		other.TestCollisionVsSphere(this, ref c);

		c.contacts[0].normal *= -1.0f;

		return c.status;
	}

	public override bool TestCollisionVsComplex(ComplexCollisionHull3D other, ref Collision c)
	{
		/* *
		 * See OBB
		 * */
		 
		other.TestCollisionVsSphere(this, ref c);

		return c.status;
	}

	public float GetRadius()
	{
		return radius;
	}

	void DebugDrawHull(Color col)
	{
		Vector3 pos = transform.position;
		Vector3[] corners = {
			new Vector3(pos.x + radius, pos.y + radius, pos.z - radius),
			new Vector3(pos.x + radius, pos.y + radius, pos.z + radius),
			new Vector3(pos.x - radius, pos.y + radius, pos.z + radius),
			new Vector3(pos.x - radius, pos.y + radius, pos.z - radius),
			new Vector3(pos.x - radius, pos.y - radius, pos.z - radius),
			new Vector3(pos.x + radius, pos.y - radius, pos.z - radius),
			new Vector3(pos.x + radius, pos.y - radius, pos.z + radius),
			new Vector3(pos.x - radius, pos.y - radius, pos.z + radius)
		};

		// Draw a basic wireframe thing
		Debug.DrawLine(corners[0], corners[1], col);
		Debug.DrawLine(corners[1], corners[2], col);
		Debug.DrawLine(corners[2], corners[3], col);
		Debug.DrawLine(corners[3], corners[4], col);
		Debug.DrawLine(corners[4], corners[5], col);
		Debug.DrawLine(corners[5], corners[6], col);
		Debug.DrawLine(corners[6], corners[7], col);
	}
}