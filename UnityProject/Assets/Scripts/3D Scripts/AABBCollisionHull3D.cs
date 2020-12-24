using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABBCollisionHull3D : CollisionHull3D
{
	public AABBCollisionHull3D() : base(CollisionHullType3D.hull_aabb) { }
	public AABBCollisionHull3D(Particle3D p) : base(CollisionHullType3D.hull_aabb) { particle = p; }

	public bool debugDraw = false;

	public Vector3 minCorner, maxCorner;
	public Vector3[] allCorners;

	[Tooltip("Leave at (0,0,0) to use renderer bounds")]
	public Vector3 bounds;
	public Vector3 GetBounds() { return bounds; }

	// Start is called before the first frame update
	void Start()
	{
		// Only need bounds if none given
		if (bounds == Vector3.zero)
			bounds = particle.GetComponent<Renderer>().bounds.size * 0.5f;

		allCorners = new Vector3[8];

		FindCorners();
		UpdateCorners();
	}

	// Update is called once per frame
	void Update()
	{
		UpdateCorners();

		if (debugDraw)
			DebugDrawBound(Color.red);
	}

	public override void ManualUpdate()
	{
		UpdateCorners();

		//if (debugDraw) DebugDrawBound(Color.cyan);
	}

	public override bool TestCollisionVsSphere(SphereCollisionHull3D other, ref Collision c)
	{
		/* *
		 * 1. Find the closest point on the box
		 *       (Done by clamping center of circle to be within box dimensions)
		 * 2. Take the difference: diff = center - closePt
		 * 3. Get distance: dist^2 = dot(diff,diff)
		 * 4. Pass if distance is w/in radius: dist^2 <= radius^2
		 * */

		// Step 1
		Vector3 closestPt = Vector3.zero;
		closestPt.x = Mathf.Max(GetParticle3D().GetPosition().x - bounds.x, Mathf.Min(other.GetParticle3D().GetPosition().x, GetParticle3D().GetPosition().x + bounds.x));
		closestPt.y = Mathf.Max(GetParticle3D().GetPosition().y - bounds.y, Mathf.Min(other.GetParticle3D().GetPosition().y, GetParticle3D().GetPosition().y + bounds.y));
		closestPt.z = Mathf.Max(GetParticle3D().GetPosition().z - bounds.z, Mathf.Min(other.GetParticle3D().GetPosition().z, GetParticle3D().GetPosition().z + bounds.z));

		//Debug.DrawLine(closestPt, other.GetParticle3D().GetPosition(), Color.red);

		// Step 2
		Vector3 diff = other.GetParticle3D().GetPosition() - closestPt;

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
		 * pass if, for all axis, max extent of A is greater than the min extent of B
		 * 1. Get 2 oposite corners for both boxes
		 * 2. Compare max1.x to min2.x to see if there's overlap
		 * 3. If (#2): Compare max1.y to min2.y to see if there is overlap
		 * 4. Compare in the same way with the box's reverse
		 * 5. If all are true: then PASS
		 * */

		// Otherwise false if it does not make every check
		bool collision = false;

		// Check all axes
		if (maxCorner.x >= other.minCorner.x && maxCorner.y >= other.minCorner.y && maxCorner.z >= other.minCorner.z)
		{
			if (other.maxCorner.x >= minCorner.x && other.maxCorner.y >= minCorner.y && other.maxCorner.z >= minCorner.z)
			{
				collision = true;
			}
		}

		//Debug.Log("Box1: " + minCorner.ToString("F2") + " | " + maxCorner.ToString("F2")+ "\nBox2: " + other.minCorner.ToString("F2") + " | " + other.maxCorner.ToString("F2"));

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
			c.contacts[0].point = maxCorner;
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

	public override bool TestCollisionVsOBB(OBBCollisionHull3D other, ref Collision c)
	{
		/* *
		 * See OBB
		 * */

		other.TestCollisionVsAABB(this, ref c);

		return c.status;
	}

	public override bool TestCollisionVsComplex(ComplexCollisionHull3D other, ref Collision c)
	{
		/* *
		 * See OBB
		 * */

		other.TestCollisionVsAABB(this, ref c);

		return c.status;
	}

	void UpdateCorners()
	{
		//bounds = particle.GetComponent<Renderer>().bounds.size * 0.5f;

		minCorner = particle.GetPosition() - bounds;
		maxCorner = particle.GetPosition() + bounds;

		//Debug.Log(particle.name + "\nMin Corner: " + minCorner + "\nMaxCorner: " + maxCorner);
	}

	public void FindCorners()
	{
		float x, y, z;

		x = -bounds.x;
		y = -bounds.y;
		z = -bounds.z;
		allCorners[0] = new Vector3(x, y, z);
		allCorners[0] += particle.GetPosition();

		x = bounds.x;
		y = -bounds.y;
		z = -bounds.z;
		allCorners[1] = new Vector3(x, y, z);
		allCorners[1] += particle.GetPosition();

		x = bounds.x;
		y = -bounds.y;
		z = bounds.z;
		allCorners[2] = new Vector3(x, y, z);
		allCorners[2] += particle.GetPosition();

		x = -bounds.x;
		y = -bounds.y;
		z = bounds.z;
		allCorners[3] = new Vector3(x, y, z);
		allCorners[3] += particle.GetPosition();

		x = -bounds.x;
		y = bounds.y;
		z = bounds.z;
		allCorners[4] = new Vector3(x, y, z);
		allCorners[4] += particle.GetPosition();

		x = -bounds.x;
		y = bounds.y;
		z = -bounds.z;
		allCorners[5] = new Vector3(x, y, z);
		allCorners[5] += particle.GetPosition();

		x = bounds.x;
		y = bounds.y;
		z = -bounds.z;
		allCorners[6] = new Vector3(x, y, z);
		allCorners[6] += particle.GetPosition();

		x = bounds.x;
		y = bounds.y;
		z = bounds.z;
		allCorners[7] = new Vector3(x, y, z);
		allCorners[7] += particle.GetPosition();
	}

	public void DebugDrawBound(Color c)
	{
		Vector3 otherCorner1 = new Vector3(minCorner.x, maxCorner.y, minCorner.z);
		Vector3 otherCorner2 = new Vector3(maxCorner.x, maxCorner.y, minCorner.z);
		Debug.DrawLine(minCorner, otherCorner1, c);
		Debug.DrawLine(otherCorner1, maxCorner, c);
		Debug.DrawLine(maxCorner, otherCorner2, c);
		Debug.DrawLine(otherCorner2, minCorner, c);
	}
}