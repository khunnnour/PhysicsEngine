using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBoundingBoxCollisionHull2D : CollisionHull2D
{
	public ObjectBoundingBoxCollisionHull2D() : base(CollisionHullType2D.hull_obb) { }

	public Vector2[] corners;

	Vector3 bounds;

	// Start is called before the first frame update
	void Start()
	{
		corners = new Vector2[4];
		bounds = GetComponent<Renderer>().bounds.size * 0.5f;
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
		 * See circle
		 * */

		other.TestCollisionVsOBB(this, ref c);

		c.contacts[0].normal *= -1.0f;

		return c.status;
	}
	public override bool TestCollisionVsAABB(AxisAlignedBoundingBoxCollisionHull2D other, ref Collision c)
	{
		/* *
		 * See AABB
		 * */

		other.TestCollisionVsOBB(this, ref c);

		c.contacts[0].normal *= -1.0f;

		return c.status;
	}

	public override bool TestCollisionVsOBB(ObjectBoundingBoxCollisionHull2D other, ref Collision c)
	{
		/* *
		 *  = = = OLD START = = =
		 *  1. Find r normal of box1: r = ( cos,  sin)
		 *  2. Find u normal of box1: u = (-sin,  cos)
		 *  3. Project points of box2 on r: p2' = dot(p2, r) * r
		 *  4. Project points of box1 on r: p1' = dot(p1, r) * r
		 *  
		 *  = = = NEW START = = =
		 *  1. Multiply other box by inverse matrix
		 *  
		 *  = = = REST = = =
		 *  5. Find min/max points for both
		 *  6. Make 2 AABB with repsective max/min values
		 *  7. Run collision test
		 *  8. If PASS: repeat Steps 3-8 with u instead of r
		 *  9. If PASS: repeat steps 1-9 with boxes reversed
		 * 10: PASS if all normals produce collisions
		 */
		 
		// Steps 1-8
		bool pass = StepsOneThroughEight(GetComponent<ObjectBoundingBoxCollisionHull2D>(), other, ref c);
		//Debug.Log("First run: " + pass);
		
		// Step 9
		if (pass)
		{
			pass = StepsOneThroughEight(other, GetComponent<ObjectBoundingBoxCollisionHull2D>(), ref c);
			//Debug.Log("Second run: " + pass);
		}

		if (pass)
		{
			/* - SET COLLISION DATA - */
			// Yes collision
			c.status = true;

			// Calculate penetration
			Vector2 diff = GetParticle2D().GetPosition();
			diff -= other.GetParticle2D().GetPosition();

			// Contact
			//c.contactCount = 1;
			//c.contacts[0].point = maxCorner;
			//c.contacts[0].penetration = bounds.x + other.bounds.x - diff.magnitude;
			c.contacts[0].normal = diff.normalized;
			c.contacts[0].restitution = 1.0f;
		}
		else
		{
			// No collision
			c.status = false;
		}

		return pass;
	}

	bool StepsOneThroughEight(ObjectBoundingBoxCollisionHull2D box1, ObjectBoundingBoxCollisionHull2D box2, ref Collision c)
	{
		/*
				// Step 1/2
				float rot = box1.GetParticle2D().GetRotation();
				Vector2 normR = new Vector2(Mathf.Cos(rot), Mathf.Sin(rot));
				Vector2 normU = new Vector2(-Mathf.Sin(rot), Mathf.Cos(rot));

				// Hold both normals
				Vector2[] norms = new Vector2[2] { normR, normU };

				//Debug.Log("r: " + normR.ToString("F3") + " | u: " + normU.ToString("F3"));
			*/
		// Make holders for the projected points and the two opposite corners
		Vector2[] projCorners1 = new Vector2[4];
		Vector2[] extremeCorners1 = new Vector2[2];
		Vector2[] projCorners2 = new Vector2[4];
		Vector2[] extremeCorners2 = new Vector2[2];

		// Cycle through the corners
		for (int i = 0; i < 4; i++)
		{
			// Step 3
			// projCorners1[i] = DotProd(box1.corners[i], norms[a]) * norms[a];
			projCorners1[i] = box1.gameObject.transform.worldToLocalMatrix * (box1.corners[i] - box1.GetParticle2D().GetPosition());

			// Step 4
			// projCorners2[i] = DotProd(box2.corners[i], norms[a]) * norms[a];
			projCorners2[i] = box1.gameObject.transform.worldToLocalMatrix * (box2.corners[i] - box1.GetParticle2D().GetPosition());

			// Step 5
			if (i == 0)
			{
				// Set initial values
				extremeCorners1[0] = projCorners1[i];
				extremeCorners1[1] = projCorners1[i];
				extremeCorners2[0] = projCorners2[i];
				extremeCorners2[1] = projCorners2[i];
			}
			else
			{
				// When on a line, only need to check x to find end points (except on vertical)
				// Box 1
				if (projCorners1[i].x < extremeCorners1[0].x/* && projCorners1[i].y < extremeCorners1[0].y*/)
					extremeCorners1[0] = projCorners1[i];
				if (projCorners1[i].x > extremeCorners1[1].x/* && projCorners1[i].y > extremeCorners1[1].y*/)
					extremeCorners1[1] = projCorners1[i];
				
				// Box 2
				/*if (projCorners2[i].x < extremeCorners2[0].x)
					extremeCorners2[0] = projCorners2[i];
				if (projCorners2[i].x > extremeCorners2[1].x)
					extremeCorners2[1] = projCorners2[i];*/
			}
		}

		// Draw projected box1
		Debug.DrawLine(projCorners1[0], projCorners1[1], Color.cyan);
		Debug.DrawLine(projCorners1[1], projCorners1[2], Color.cyan);
		Debug.DrawLine(projCorners1[2], projCorners1[3], Color.cyan);
		Debug.DrawLine(projCorners1[3], projCorners1[0], Color.cyan);

		// Draw projected box2
		Debug.DrawLine(projCorners2[0], projCorners2[1], Color.cyan);
		Debug.DrawLine(projCorners2[1], projCorners2[2], Color.cyan);
		Debug.DrawLine(projCorners2[2], projCorners2[3], Color.cyan);
		Debug.DrawLine(projCorners2[3], projCorners2[0], Color.cyan);

		// Step 6
		AxisAlignedBoundingBoxCollisionHull2D one = new AxisAlignedBoundingBoxCollisionHull2D();
		one.SetParticle2D(box1.GetParticle2D());
		one.minCorner = extremeCorners1[0];
		one.maxCorner = extremeCorners1[1];
		one.DebugDrawBound(Color.red);

		ObjectBoundingBoxCollisionHull2D newBox2 = box2;
		for (int i = 0; i < corners.Length; i++)
			newBox2.corners[i] = projCorners2[i];

		// Step 7
		bool pass = one.TestCollisionVsOBB(newBox2, ref c);

		return pass;
	}

	void UpdateCorners()
	{
		Vector2 currPos = particle.GetPosition();
		float rot = particle.GetRotation();
		float x, y;

		/* *
		 * Rot Mat = [ cosθ , -sinθ ]
		 * 			 [ sinθ ,  cosθ ]
		 */

		x = -bounds.x;
		y = -bounds.y;
		corners[0] = new Vector2(Mathf.Cos(rot) * x + -Mathf.Sin(rot) * y, Mathf.Sin(rot) * x + Mathf.Cos(rot) * y);
		corners[0] += currPos;

		x = -bounds.x;
		y = bounds.y;
		corners[1] = new Vector2(Mathf.Cos(rot) * x + -Mathf.Sin(rot) * y, Mathf.Sin(rot) * x + Mathf.Cos(rot) * y);
		corners[1] += currPos;

		x = bounds.x;
		y = bounds.y;
		corners[2] = new Vector2(Mathf.Cos(rot) * x + -Mathf.Sin(rot) * y, Mathf.Sin(rot) * x + Mathf.Cos(rot) * y);
		corners[2] += currPos;

		x = bounds.x;
		y = -bounds.y;
		corners[3] = new Vector2(Mathf.Cos(rot) * x + -Mathf.Sin(rot) * y, Mathf.Sin(rot) * x + Mathf.Cos(rot) * y);
		corners[3] += currPos;

		/**/
		Debug.DrawLine(corners[0], corners[1], Color.green);
		Debug.DrawLine(corners[1], corners[2], Color.green);
		Debug.DrawLine(corners[2], corners[3], Color.green);
		Debug.DrawLine(corners[3], corners[0], Color.green);
	}
}