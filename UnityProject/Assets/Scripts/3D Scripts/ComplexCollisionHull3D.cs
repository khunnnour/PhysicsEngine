using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexCollisionHull3D : CollisionHull3D
{
	public ComplexCollisionHull3D() : base(CollisionHullType3D.hull_complex) { }

	[System.Serializable]
	public struct HullPiece
	{
		public CollisionHullType3D hullType;

		[Tooltip("offset from parent object center")]
		public Vector3 center;

		[Tooltip("x component is used for the sphere radius")]
		public Vector3 dimensions;

		public Color debugFrameColor;

		private CollisionHull3D hull;
		public CollisionHull3D GetHull() { return hull; }

		public void CreateHull(Particle3D part/*, bool debug = false*/)
		{
			if (hullType == CollisionHullType3D.hull_obb)
			{
				OBBCollisionHull3D tempHull = new OBBCollisionHull3D(part)
				{
					bounds = dimensions * 0.5f,

					corners = new Vector3[8],
					rotCorners = new Vector3[8]
				};

				tempHull.FindCorners();

				hull = tempHull;
			}
			else if (hullType == CollisionHullType3D.hull_aabb)
			{
				AABBCollisionHull3D tempHull = new AABBCollisionHull3D(part)
				{
					bounds = dimensions * 0.5f,

					allCorners = new Vector3[8]
				};

				tempHull.FindCorners();

				hull = tempHull;
			}
			else if (hullType == CollisionHullType3D.hull_sphere)
			{
				SphereCollisionHull3D tempHull = new SphereCollisionHull3D(part, dimensions.x * 0.5f);

				hull = tempHull;
			}
			else
				Debug.Log("ERROR: Cannot add complex hull type to complex hull.");
		}
	}

	public HullPiece[] hullPieces;
	public bool debugDraw = true;

	// Start is called before the first frame update
	void Start()
	{
		InitHulls();
	}

	// Update is called once per frame
	void Update()
	{
		UpdateHulls();
		DebugDraw();
	}

	public override void ManualUpdate()
	{
	}

	private void InitHulls()
	{
		// For every hull piece, create a hull
		for (int i = 0; i < hullPieces.Length; i++)
		{
			hullPieces[i].CreateHull(particle);
		}
	}

	private void UpdateHulls()
	{
		for (int i = 0; i < hullPieces.Length; i++)
		{
			hullPieces[i].GetHull().ManualUpdate();
		}
	}

	void DebugDraw()
	{
		for (int i = 0; i < hullPieces.Length; i++)
		{
			Color col = hullPieces[i].debugFrameColor;

			Vector3 pos = particle.GetRotation() * hullPieces[i].center;

			if (hullPieces[i].hullType == CollisionHullType3D.hull_sphere)
			{
				float radius = hullPieces[i].dimensions.x * 0.5f;
				Vector3[] corners = {
					new Vector3(pos.x-radius,pos.y,pos.z),
					new Vector3(pos.x,pos.y-radius,pos.z),
					new Vector3(pos.x+radius,pos.y,pos.z),
					new Vector3(pos.x,pos.y+radius,pos.z),
					new Vector3(pos.x,pos.y,pos.z-radius),
					new Vector3(pos.x,pos.y-radius,pos.z),
					new Vector3(pos.x,pos.y,pos.z+radius),
					new Vector3(pos.x,pos.y+radius,pos.z)
				};

				//Debug.Log("radius: " + radius + ", pos: " + pos);

				// Draw wireframe
				//Color col = hullPieces[i].debugFrameColor;
				Debug.DrawLine(corners[0], corners[1], col);
				Debug.DrawLine(corners[1], corners[2], col);
				Debug.DrawLine(corners[2], corners[3], col);
				Debug.DrawLine(corners[3], corners[4], col);
				Debug.DrawLine(corners[4], corners[5], col);
				Debug.DrawLine(corners[5], corners[6], col);
				Debug.DrawLine(corners[6], corners[7], col);
				Debug.DrawLine(corners[7], corners[0], col);
			}
			else if (hullPieces[i].hullType == CollisionHullType3D.hull_aabb)
			{
				AABBCollisionHull3D aabb = (AABBCollisionHull3D)hullPieces[i].GetHull();

				Vector3[] corners = {
					pos + aabb.minCorner,
					new Vector3(aabb.minCorner.x, aabb.minCorner.y, aabb.maxCorner.z)+pos,
					new Vector3(aabb.minCorner.x, aabb.maxCorner.y, aabb.maxCorner.z)+pos,
					new Vector3(aabb.minCorner.x, aabb.maxCorner.y, aabb.minCorner.z)+pos,
					pos + aabb.maxCorner,
					new Vector3(aabb.maxCorner.x, aabb.minCorner.y, aabb.maxCorner.z)+pos,
					new Vector3(aabb.maxCorner.x, aabb.minCorner.y, aabb.minCorner.z)+pos,
					new Vector3(aabb.maxCorner.x, aabb.maxCorner.y, aabb.minCorner.z)+pos
				};


				Debug.DrawLine(corners[0], corners[1], col);
				Debug.DrawLine(corners[1], corners[2], col);
				Debug.DrawLine(corners[2], corners[3], col);
				Debug.DrawLine(corners[3], corners[0], col);
				Debug.DrawLine(corners[4], corners[5], col);
				Debug.DrawLine(corners[5], corners[6], col);
				Debug.DrawLine(corners[6], corners[7], col);
				Debug.DrawLine(corners[7], corners[4], col);
			}
			else if (hullPieces[i].hullType == CollisionHullType3D.hull_obb)
			{
				OBBCollisionHull3D obb = (OBBCollisionHull3D)hullPieces[i].GetHull();

				//Vector3[] rotCorners = hulls[i].GetComponent<OBBCollisionHull3D>().rotCorners;
				Vector3[] rotCorners = obb.rotCorners;
				//Color col = hullPieces[i].debugFrameColor;

				//Debug.Log(transform.position.ToString("F2") + " + " + hullPieces[i].center + " ?= " + pos.ToString("F2"));

				// Draw wireframe
				Debug.DrawLine(pos + rotCorners[0], pos + rotCorners[1], col);
				Debug.DrawLine(pos + rotCorners[1], pos + rotCorners[2], col);
				Debug.DrawLine(pos + rotCorners[2], pos + rotCorners[3], col);
				Debug.DrawLine(pos + rotCorners[3], pos + rotCorners[4], col);
				Debug.DrawLine(pos + rotCorners[4], pos + rotCorners[5], col);
				Debug.DrawLine(pos + rotCorners[5], pos + rotCorners[6], col);
				Debug.DrawLine(pos + rotCorners[6], pos + rotCorners[7], col);
			}
		}
	}

	public override bool TestCollisionVsSphere(SphereCollisionHull3D other, ref Collision c)
	{
		/* *
		 * Cycle through all hulls and have them test against
		 * Whatever hull
		 * */

		bool somethingTrue = false;

		Vector3 origPos = transform.position;

		for (int i = 0; i < hullPieces.Length; i++)
		{
			Vector3 pos = particle.GetRotation() * hullPieces[i].center;

			// Move particle to center of hull piece
			particle.SetPosition(pos + particle.GetPosition());
			particle.CalculateTransMat();

			// Update the hull then test
			hullPieces[i].GetHull().ManualUpdate();
			hullPieces[i].GetHull().TestCollisionVsSphere(other, ref c);

			// Break on first collision
			if (c.status == true)
			{
				Debug.Log(other.name + " hit the sphere in slot " + i);
				break;
			}
		}

		// Reset position
		particle.SetPosition(origPos);

		// Return if anyhting was true
		return somethingTrue;
	}

	public override bool TestCollisionVsAABB(AABBCollisionHull3D other, ref Collision c)
	{
		/* *
		 * Cycle through all hulls and have them test against
		 * Whatever hull
		 * */

		bool somethingTrue = false;

		// Store original position
		Vector3 origPos = transform.position;

		for (int i = 0; i < hullPieces.Length; i++)
		{
			Vector3 pos = particle.GetRotation() * hullPieces[i].center;

			// Move particle to center of hull piece
			particle.SetPosition(pos + particle.GetPosition());
			particle.CalculateTransMat();

			// Update the hull then test
			hullPieces[i].GetHull().ManualUpdate();
			hullPieces[i].GetHull().TestCollisionVsAABB(other, ref c);

			if (c.status == true)
			{
				Debug.Log(other.name + " hit the aabb in slot " + i);
				break;
			}
		}

		// Reset position and everything
		particle.SetPosition(origPos);
		particle.CalculateTransMat();

		// Return if anyhting was true
		return somethingTrue;
	}

	public override bool TestCollisionVsOBB(OBBCollisionHull3D other, ref Collision c)
	{
		/* *
		 * Cycle through all hulls and have them test against
		 * Whatever hull
		 * */

		bool somethingTrue = false;

		Vector3 origPos = transform.position;

		for (int i = 0; i < hullPieces.Length; i++)
		{
			Vector3 pos = particle.GetRotation() * hullPieces[i].center;

			particle.SetPosition(pos + particle.GetPosition());
			hullPieces[i].GetHull().ManualUpdate();
			hullPieces[i].GetHull().TestCollisionVsOBB(other, ref c);

			if (c.status == true)
			{
				//Debug.Log(other.name + " hit the obb in slot " + i);
				break;
			}
		}

		// Reset position
		particle.SetPosition(origPos);

		// Return if anyhting was true
		return somethingTrue;
	}

	public override bool TestCollisionVsComplex(ComplexCollisionHull3D other, ref Collision c)
	{
		/* *
		 * Probably make temp objects and test those individually 
		 * against the other complex hull
		 * */

		bool somethingTrue = false;

		// TODO: implement complex v complex

		// Return if anyhting was true
		return somethingTrue;
	}
}