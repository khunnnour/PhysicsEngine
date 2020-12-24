using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBBCollisionHull3D : CollisionHull3D
{
	public OBBCollisionHull3D() : base(CollisionHullType3D.hull_obb) { }
	public OBBCollisionHull3D(Particle3D p) : base(CollisionHullType3D.hull_obb) { particle = p; }

	public bool debugDraw = false;

	public Vector3[] corners;
	public Vector3[] rotCorners;

	[Tooltip("Leave at (0,0,0) to use renderer bounds")]
	public Vector3 bounds = Vector3.zero;

	// Start is called before the first frame update
	void Start()
	{
		//Debug.Log("OBB start called");
		corners = new Vector3[8];
		rotCorners = new Vector3[8];
		// Only need bounds if none given
		if (bounds == Vector3.zero)
			bounds = GetComponent<Renderer>().bounds.size * 0.5f;
		FindCorners();
	}

	// Update is called once per frame
	void Update()
	{
		UpdateCorners();

		if (debugDraw) DebugDrawHull(Color.cyan);
	}

	public override void ManualUpdate()
	{
		UpdateCorners();

		//if (debugDraw) DebugDrawHull(Color.cyan);
	}

	public override bool TestCollisionVsSphere(SphereCollisionHull3D other, ref Collision c)
	{
		/* *
		 * 1. Multiply circle center inverse matrix
		 * 2. Run AABBvSphere check compared to local corners
		 * 3. PASS if closest point when projected within radius
		 * */

		Matrix4x4 thisTransMat = GetParticle3D().GetInvTransMat();
		Vector3 negativeTranslation = new Vector3(thisTransMat[0, 3], thisTransMat[1, 3], thisTransMat[2, 3]);

		Vector3 newOrigin = GetParticle3D().GetInvTransMat() * other.GetParticle3D().GetPosition();
		newOrigin += negativeTranslation;

		//Debug.Log("New Positions: newOrigin=" + newOrigin.ToString("F2") + ", newPosition=" + newPosition.ToString("F2"));

		Vector3 closestPt = Vector3.zero;
		closestPt.x = Mathf.Max(-bounds.x, Mathf.Min(newOrigin.x, bounds.x));
		closestPt.y = Mathf.Max(-bounds.y, Mathf.Min(newOrigin.y, bounds.y));
		closestPt.z = Mathf.Max(-bounds.z, Mathf.Min(newOrigin.z, bounds.z));

		Vector3 closestPtWorldSpce = GetParticle3D().GetTransMat() * closestPt;
		closestPtWorldSpce += GetParticle3D().GetPosition();

		//Debug.DrawLine(closestPtWorldSpce, other.GetParticle3D().GetPosition(), Color.red);

		// Find difference between point and center of circle
		Vector3 diff = newOrigin - closestPt;

		// Step 3
		float distSq = diff.sqrMagnitude;

		// Step 4
		if (distSq <= other.radius * other.radius)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			float pen = other.radius - diff.magnitude;

			// Contact
			c.contactCount = 1;
			c.contacts[0].point = closestPt;
			c.contacts[0].penetration = pen;
			c.contacts[0].normal = -diff.normalized;
			c.contacts[0].restitution = 0.95f;
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
		 * 1. Find new max corners for box around OBB
		 * 2. Run AABB v AABB on the AABB and new AABB from #1
		 *    IF PASS
		 * 3. Transform OBB into local space and the AABB
		 * 4. Repeat Steps 1-2
		 * */

		bool collision = false;

		// Step 1 - Find new max corners for box around OBB
		Vector3 obbMaxCorner = rotCorners[0];
		Vector3 obbMinCorner = rotCorners[0];
		for (int i = 1; i < rotCorners.Length; i++)
		{
			// If you find a new highest/lowest value then use that
			if (rotCorners[i].x > obbMaxCorner.x)
				obbMaxCorner.x = rotCorners[i].x;
			if (rotCorners[i].x < obbMinCorner.x)
				obbMinCorner.x = rotCorners[i].x;

			if (rotCorners[i].y > obbMaxCorner.y)
				obbMaxCorner.y = rotCorners[i].y;
			if (rotCorners[i].y < obbMinCorner.y)
				obbMinCorner.y = rotCorners[i].y;

			if (rotCorners[i].z > obbMaxCorner.z)
				obbMaxCorner.z = rotCorners[i].z;
			if (rotCorners[i].z < obbMinCorner.z)
				obbMinCorner.z = rotCorners[i].z;
		}
		//Debug.Log(obbMinCorner.ToString("F2") + ", " + obbMaxCorner.ToString("F2"));

		// Step 2 - Run AABB
		if (obbMaxCorner.x >= other.minCorner.x && obbMaxCorner.y >= other.minCorner.y && obbMaxCorner.z >= other.minCorner.z)
		{
			if (other.maxCorner.x >= obbMinCorner.x && other.maxCorner.y >= obbMinCorner.y && other.maxCorner.z >= obbMinCorner.z)
			{
				collision = true;
				//Debug.Log("First AABBvAABB passed");
			}
		}

		// Step 3 - If a success, rebase and try again
		if (collision)
		{
			Matrix4x4 thisTransMat = GetParticle3D().GetInvTransMat();
			Vector3 negativeTranslation = new Vector3(thisTransMat[0, 3], thisTransMat[1, 3], thisTransMat[2, 3]);

			other.FindCorners();
			Vector3 newMaxCorner = thisTransMat * other.allCorners[0];
			newMaxCorner += negativeTranslation;
			Vector3 newMinCorner = newMaxCorner;

			// Step 4 - Transform AABB into obb worldspace & Find AABB based on new corners
			Vector3 tempProjCorner = new Vector3();
			for (int i = 1; i < 8; i++)
			{
				tempProjCorner = thisTransMat * other.allCorners[i];
				tempProjCorner += negativeTranslation;

				if (tempProjCorner.x > newMaxCorner.x)
					newMaxCorner.x = tempProjCorner.x;
				if (tempProjCorner.x < newMinCorner.x)
					newMinCorner.x = tempProjCorner.x;

				if (tempProjCorner.y > newMaxCorner.y)
					newMaxCorner.y = tempProjCorner.y;
				if (tempProjCorner.y < newMinCorner.y)
					newMinCorner.y = tempProjCorner.y;

				if (tempProjCorner.z > newMaxCorner.z)
					newMaxCorner.z = tempProjCorner.z;
				if (tempProjCorner.z < newMinCorner.z)
					newMinCorner.z = tempProjCorner.z;
			}
			//Debug.Log("AABB Projected: "+newMinCorner.ToString("F3") + ", " + newMaxCorner.ToString("F3"));

			// Run AABB
			collision = false;
			if (newMaxCorner.x >= -bounds.x && newMaxCorner.y >= -bounds.y && newMaxCorner.z >= -bounds.z)
			{
				if (bounds.x >= newMinCorner.x && bounds.y >= newMinCorner.y && bounds.z >= newMinCorner.z)
				{
					collision = true;
				}
			}
		}

		// Log collision (or not)
		if (collision)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			Vector3 diff = GetParticle3D().GetPosition();
			diff -= other.GetParticle3D().GetPosition();

			// Contact
			c.contactCount = 1;
			c.contacts[0].point = corners[0];
			c.contacts[0].penetration = bounds.x + other.GetBounds().x - diff.magnitude;
			c.contacts[0].normal = diff.normalized;
			c.contacts[0].restitution = 0.95f;
		}
		else
		{
			// No collision
			c.status = false;
		}

		return c.status;
	}

	public override bool TestCollisionVsOBB(OBBCollisionHull3D other, ref Collision c)
	{
		/* *
		 * 1. Bring both OBB's into one's local space
		 * 2. Do AABBvOBB on that
		 *    IF PASS:
		 * 3. Steps 1-2 vice-versa
		 * */

		Matrix4x4 thisTransMat = GetParticle3D().GetInvTransMat();
		Vector3 negativeTranslation = new Vector3(thisTransMat[0, 3], thisTransMat[1, 3], thisTransMat[2, 3]);

		// Step 1 - Find new max corners for box around other OBB
		Vector3 obbMaxCorner = thisTransMat * other.rotCorners[0];
		obbMaxCorner += negativeTranslation;
		Vector3 obbMinCorner = obbMaxCorner;
		Vector3 tempPrjPt = new Vector3();
		for (int i = 1; i < 8; i++)
		{
			// Project new point
			tempPrjPt = thisTransMat * other.rotCorners[i];
			tempPrjPt += negativeTranslation;

			// If you find a new highest/lowest value then use that
			if (tempPrjPt.x > obbMaxCorner.x)
				obbMaxCorner.x = tempPrjPt.x;
			if (tempPrjPt.x < obbMinCorner.x)
				obbMinCorner.x = tempPrjPt.x;

			if (tempPrjPt.y > obbMaxCorner.y)
				obbMaxCorner.y = tempPrjPt.y;
			if (tempPrjPt.y < obbMinCorner.y)
				obbMinCorner.y = tempPrjPt.y;

			if (tempPrjPt.z > obbMaxCorner.z)
				obbMaxCorner.z = tempPrjPt.z;
			if (tempPrjPt.z < obbMinCorner.z)
				obbMinCorner.z = tempPrjPt.z;
		}
		//Debug.Log(obbMinCorner.ToString("F2") + ", " + obbMaxCorner.ToString("F2"));

		// Step 2 - Run AABBvOBB on that
		bool collision = false;
		if (obbMaxCorner.x >= -bounds.x && obbMaxCorner.y >= -bounds.y && obbMaxCorner.z >= -bounds.z)
		{
			if (bounds.x >= obbMinCorner.x && bounds.y >= obbMinCorner.y && bounds.z >= obbMinCorner.z)
			{
				collision = true;
				//Debug.Log("AABBvOBB part 1 completed");
			}
		}

		// If the first part collides, flip it and reverse it
		if(collision)
		{
			thisTransMat = other.GetParticle3D().GetInvTransMat();
			negativeTranslation = new Vector3(thisTransMat[0, 3], thisTransMat[1, 3], thisTransMat[2, 3]);

			// Step 1 - Find new max corners for box around other OBB
			obbMaxCorner = thisTransMat * rotCorners[0];
			obbMaxCorner += negativeTranslation;
			obbMinCorner = obbMaxCorner;
			for (int i = 1; i < 8; i++)
			{
				// Project new point
				tempPrjPt = thisTransMat * rotCorners[i];
				tempPrjPt += negativeTranslation;

				// If you find a new highest/lowest value then use that
				if (tempPrjPt.x > obbMaxCorner.x)
					obbMaxCorner.x = tempPrjPt.x;
				if (tempPrjPt.x < obbMinCorner.x)
					obbMinCorner.x = tempPrjPt.x;

				if (tempPrjPt.y > obbMaxCorner.y)
					obbMaxCorner.y = tempPrjPt.y;
				if (tempPrjPt.y < obbMinCorner.y)
					obbMinCorner.y = tempPrjPt.y;

				if (tempPrjPt.z > obbMaxCorner.z)
					obbMaxCorner.z = tempPrjPt.z;
				if (tempPrjPt.z < obbMinCorner.z)
					obbMinCorner.z = tempPrjPt.z;
			}
			//Debug.Log(obbMinCorner.ToString("F2") + ", " + obbMaxCorner.ToString("F2"));

			// Step 2 - Run AABBvOBB on that
			collision = false;
			if (obbMaxCorner.x >= -other.bounds.x && obbMaxCorner.y >= -other.bounds.y && obbMaxCorner.z >= -other.bounds.z)
			{
				if (other.bounds.x >= obbMinCorner.x && other.bounds.y >= obbMinCorner.y && other.bounds.z >= obbMinCorner.z)
				{
					collision = true;
					//Debug.Log("AABBvOBB part 2 completed");
				}
			}
		}


		// Log collision (or not)
		if (collision)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			Vector3 diff = GetParticle3D().GetPosition();
			diff -= other.GetParticle3D().GetPosition();

			// Contact
			c.contactCount = 1;
			c.contacts[0].point = corners[0];
			c.contacts[0].penetration = bounds.x + other.bounds.x - diff.magnitude;
			c.contacts[0].normal = diff.normalized;
			c.contacts[0].restitution = 0.95f;
		}
		else
		{
			// No collision
			c.status = false;
		}

		return c.status;
	}

	public override bool TestCollisionVsComplex(ComplexCollisionHull3D other, ref Collision c)
	{
		/* *
		 * See Complex
		 * */

		other.TestCollisionVsOBB(this, ref c);

		return c.status;
	}

	// Should only be called at start
	public void FindCorners()
	{
		float x, y, z;

		x = -bounds.x;
		y = -bounds.y;
		z = -bounds.z;
		corners[0] = new Vector3(x, y, z);

		x = bounds.x;
		y = -bounds.y;
		z = -bounds.z;
		corners[1] = new Vector3(x, y, z);

		x = bounds.x;
		y = -bounds.y;
		z = bounds.z;
		corners[2] = new Vector3(x, y, z);

		x = -bounds.x;
		y = -bounds.y;
		z = bounds.z;
		corners[3] = new Vector3(x, y, z);

		x = -bounds.x;
		y = bounds.y;
		z = bounds.z;
		corners[4] = new Vector3(x, y, z);

		x = -bounds.x;
		y = bounds.y;
		z = -bounds.z;
		corners[5] = new Vector3(x, y, z);

		x = bounds.x;
		y = bounds.y;
		z = -bounds.z;
		corners[6] = new Vector3(x, y, z);

		x = bounds.x;
		y = bounds.y;
		z = bounds.z;
		corners[7] = new Vector3(x, y, z);
	}

	void UpdateCorners()
	{
		Matrix4x4 transMat = particle.GetTransMat();
		//Debug.Log(gameObject.name + " transmat:\n" + transMat.ToString("F3"));

		// Loop through all points
		for (int i = 0; i < corners.Length; i++)
		{
			rotCorners[i] = transMat * corners[i];
			rotCorners[i] += particle.GetPosition();
		}
	}

	void DebugDrawHull(Color col)
	{
		Debug.DrawLine(rotCorners[0], rotCorners[1], col);
		Debug.DrawLine(rotCorners[1], rotCorners[2], col);
		Debug.DrawLine(rotCorners[2], rotCorners[3], col);
		Debug.DrawLine(rotCorners[3], rotCorners[4], col);
		Debug.DrawLine(rotCorners[4], rotCorners[5], col);
		Debug.DrawLine(rotCorners[5], rotCorners[6], col);
		Debug.DrawLine(rotCorners[6], rotCorners[7], col);
	}
}