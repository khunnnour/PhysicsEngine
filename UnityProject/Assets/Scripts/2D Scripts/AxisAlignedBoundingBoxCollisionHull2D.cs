using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisAlignedBoundingBoxCollisionHull2D : CollisionHull2D
{
	public AxisAlignedBoundingBoxCollisionHull2D() : base(CollisionHullType2D.hull_aabb) { }

	public Vector2 minCorner, maxCorner;

	public Vector2 bounds;

	// Start is called before the first frame update
	void Start()
	{
		bounds = particle.GetComponent<Renderer>().bounds.size * 0.5f;
		UpdateCorners();
	}

	// Update is called once per frame
	void Update()
	{
		UpdateCorners();
	}

	public override bool TestCollisionVsCircle(CircleCollisionHull2D other, ref Collision c)
	{
		/* *
		 * 1. Find the closest point on the box
		 *       (Done by clamping center of circle to be within box dimensions)
		 * 2. Take the difference: diff = center - closePt
		 * 3. Get distance: dist^2 = dot(diff,diff)
		 * 4. Pass if distance is w/in radius: dist^2 <= radius^2
		 * */

		// Step 1
		Vector2 AABBDimensions = new Vector2(maxCorner.x - minCorner.x, maxCorner.y - minCorner.y) * 0.5f;
		Vector2 closestPt = Vector2.zero;
		closestPt.x = Mathf.Max(GetParticle2D().GetPosition().x, Mathf.Min(other.GetParticle2D().GetPosition().x, GetParticle2D().GetPosition().x + AABBDimensions.x));
		closestPt.y = Mathf.Max(GetParticle2D().GetPosition().y, Mathf.Min(other.GetParticle2D().GetPosition().y, GetParticle2D().GetPosition().y + AABBDimensions.y));

		// Step 2
		Vector2 diff = other.GetParticle2D().GetPosition() - closestPt;

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
			c.contacts[0].restitution = 1.0f;
		}
		else
		{
			// No collision
			c.status = false;
		}

		return c.status;
	}

	public override bool TestCollisionVsAABB(AxisAlignedBoundingBoxCollisionHull2D other, ref Collision c)
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

		// Step 2
		if (maxCorner.x >= other.minCorner.x)
		{
			// Step 3
			if (maxCorner.y >= other.minCorner.y)
			{
				// Step 4
				if (other.maxCorner.x >= minCorner.x)
				{
					// Step 4
					if (other.maxCorner.y >= minCorner.y)
					{
						collision = true;
					}
				}
			}
		}

		//Debug.Log("Box1: " + minCorner.ToString("F2") + " | " + maxCorner.ToString("F2")+ "\nBox2: " + other.minCorner.ToString("F2") + " | " + other.maxCorner.ToString("F2"));

		if (collision)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			Vector2 diff = GetParticle2D().GetPosition();
			diff -= other.GetParticle2D().GetPosition();

			// Contact
			c.contactCount = 1;
			c.contacts[0].point = maxCorner;
			c.contacts[0].penetration = bounds.x + other.bounds.x - diff.magnitude;
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

	public override bool TestCollisionVsOBB(ObjectBoundingBoxCollisionHull2D other, ref Collision c)
	{
		/* *
		 * Same as above twice
		 * first: find max extents of OBB
		 * do AABB vs this box
		 * then, transform this box into 
		 * */

		// Find max extents of OBB
		float maxX = other.corners[0].x;
		float maxY = other.corners[0].y;
		float minX = other.corners[0].x;
		float minY = other.corners[0].y;
		for (int i = 1; i < other.corners.Length; i++)
		{
			if (other.corners[i].x > maxX)
				maxX = other.corners[i].x;
			if (other.corners[i].x < minX)
				minX = other.corners[i].x;

			if (other.corners[i].y > maxY)
				maxY = other.corners[i].y;
			if (other.corners[i].y < minY)
				minY = other.corners[i].y;
		}

		// Turn it into an AABB
		AxisAlignedBoundingBoxCollisionHull2D newBox = new AxisAlignedBoundingBoxCollisionHull2D();
		newBox.SetParticle2D(other.GetParticle2D());
		newBox.minCorner = new Vector2(minX, minY);
		newBox.maxCorner = new Vector2(maxX, maxY);

		newBox.DebugDrawBound(Color.white);

		/* - SET COLLISION DATA - */
		// Run AABB on new box
		bool collision = TestCollisionVsAABB(newBox, ref c);

		if (collision)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			Vector2 diff = GetParticle2D().GetPosition();
			diff -= other.GetParticle2D().GetPosition();

			// Contact
			c.contactCount = 1;
			c.contacts[0].point = maxCorner;
			c.contacts[0].penetration = bounds.x + (maxCorner.x - minCorner.x) * 0.5f - diff.magnitude;
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

	void UpdateCorners()
	{
		bounds = particle.GetComponent<Renderer>().bounds.size * 0.5f;

		minCorner = particle.GetPosition() - bounds;
		maxCorner = particle.GetPosition() + bounds;

		//Debug.Log(particle.name + "\nMin Corner: " + minCorner + "\nMaxCorner: " + maxCorner);
	}

	public void DebugDrawBound(Color c)
	{
		Vector2 otherCorner1 = new Vector2(minCorner.x, maxCorner.y);
		Vector2 otherCorner2 = new Vector2(maxCorner.x, minCorner.y);
		Debug.DrawLine(minCorner, otherCorner1, c);
		Debug.DrawLine(otherCorner1, maxCorner, c);
		Debug.DrawLine(maxCorner, otherCorner2, c);
		Debug.DrawLine(otherCorner2, minCorner, c);
	}
}