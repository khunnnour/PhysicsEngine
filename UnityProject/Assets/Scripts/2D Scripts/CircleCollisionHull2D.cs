using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleCollisionHull2D : CollisionHull2D
{
	public CircleCollisionHull2D() : base(CollisionHullType2D.hull_circle) { }

	[Range(0.0f, 100.0f)]
	public float radius;


	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		/*
		// Show a bunch of rays to show radius-ish
		Debug.DrawRay(transform.position, Vector3.up * radius, Color.green);
		Debug.DrawRay(transform.position, Vector3.left * radius, Color.green);
		Debug.DrawRay(transform.position, Vector3.right * radius, Color.green);
		Debug.DrawRay(transform.position, Vector3.forward * radius, Color.green);
		*/
	}

	public float GetRadius()
	{
		return radius;
	}

	public override bool TestCollisionVsCircle(CircleCollisionHull2D other, ref Collision c)
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
		Vector2 diff = particle.GetPosition() - other.GetParticle2D().GetPosition();

		// Step 3 - get actual distance (squared) away
		float actDist = diff.sqrMagnitude;

		// Step 4/5 - get distance needed for collision (squared) away
		float colDistSqr = radius + other.GetRadius();
		colDistSqr *= colDistSqr;

		// Step 6 - test
		if (actDist <= colDistSqr)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			float pen = radius + other.GetRadius() - diff.magnitude;

			// Contact
			c.contactCount = 1;
			c.contacts[0].point = -diff.normalized*radius;
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

	public override bool TestCollisionVsAABB(AxisAlignedBoundingBoxCollisionHull2D other, ref Collision c)
	{
		/* *
		 * 1. Find the closest point on the box
		 *      (Done by clamping center of circle to be within box dimensions)
		 * 2. Take the difference: diff = center - closePt
		 * 3. Get distance: dist^2 = dot(diff,diff)
		 * 4. Pass if distance is w/in radius: dist^2 <= radius^2
		 * */

		// Step 1
		Vector2 AABBDimensions = new Vector2(other.maxCorner.x - other.minCorner.x, other.maxCorner.y - other.minCorner.y) * 0.5f;
		Vector2 closestPt = Vector2.zero;
		closestPt.x = Mathf.Max(GetParticle2D().GetPosition().x, Mathf.Min(other.GetParticle2D().GetPosition().x, GetParticle2D().GetPosition().x + AABBDimensions.x));
		closestPt.y = Mathf.Max(GetParticle2D().GetPosition().y, Mathf.Min(other.GetParticle2D().GetPosition().y, GetParticle2D().GetPosition().y + AABBDimensions.y));
		//Debug.Log("ClosestPt = " + closestPt.ToString("F2"));

		// Step 2
		Vector2 diff = GetParticle2D().GetPosition() - closestPt;

		// Step 3
		float distSq = diff.sqrMagnitude;

		// Step 4
		if (distSq <= radius * radius)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			float pen = radius - diff.magnitude;

			// Contact
			c.contactCount = 1;
			c.contacts[0].point = closestPt;
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

	public override bool TestCollisionVsOBB(ObjectBoundingBoxCollisionHull2D other, ref Collision c)
	{
		/* *
		 * 1. Multiply circle center by OBV inv world matrix
		 * 2. Find the closest point on the box
		 *       (Done by clamping center of circle to be within box dimensions)
		 * 3. Take the difference
		 * 4. dist^2 = dot(diff,diff)
		 * 5. Pass: dist^2 <= radius^2
		 * */
		
		// Step 1
		Vector2 projCenter = other.gameObject.transform.worldToLocalMatrix * transform.position;

		Vector2 newMinCorner = other.gameObject.transform.worldToLocalMatrix * other.corners[0];
		Vector2 newMaxCorner = other.gameObject.transform.worldToLocalMatrix * other.corners[2];

		// Step 2
		Vector2 closestPt = Vector2.zero;
		closestPt.x = Mathf.Max(newMinCorner.x, Mathf.Min(particle.GetPosition().x, newMaxCorner.x));
		closestPt.y = Mathf.Max(newMinCorner.y, Mathf.Min(particle.GetPosition().y, newMaxCorner.y));
		//Debug.Log("ClosestPt = " + closestPt.ToString("F2"));

		// Step 3
		Vector2 diff = projCenter - closestPt;
		
		// Step 4
		float distSq = diff.sqrMagnitude;
		
		// Step 5
		if (distSq <= radius * radius)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			float pen = radius - diff.magnitude;

			// Contact
			c.contactCount = 1;
			c.contacts[0].point = closestPt;
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
}