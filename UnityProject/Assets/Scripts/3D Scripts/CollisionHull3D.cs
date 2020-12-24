using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollisionHull3D : MonoBehaviour
{
	public class Collision
	{
		public struct Contact
		{
			public Vector3 point;
			public Vector3 normal;
			public float restitution;
			public float penetration;

			Vector3 CalcSeparateVelocity(Particle3D a, Particle3D b = null)
			{
				// Calculate relative velocity
				Vector3 relVel = a.GetVelocity();

				if (b)
					relVel -= b.GetVelocity();

				// Return rel velocity  * normal
				return relVel;
			}

			public void ResolveVelocity(float duration,Vector3 reflectNorm, Particle3D a, Particle3D b = null)
			{
				Vector3 sepVel = CalcSeparateVelocity(a, b);

				// Check if needs to be resolved
				if (sepVel.sqrMagnitude <= 0)
					return;

				// Calculate new separation velocity based on restitution
				Vector3 newSepVel = -sepVel * restitution;

				// Check vel build-up based on acceleration
				Vector3 accCausedVelocity = a.GetAcceleration();
				if (b) accCausedVelocity -= b.GetAcceleration();

				/*Vector3 accCausedSepVelocity = accCausedVelocity * duration;
				// Remove closing vel from acceleration* normal
				// TODO: figure out how to do these checks
				if (accCausedSepVelocity.sqrMagnitude < 0)
				{
					newSepVel += restitution * accCausedSepVelocity;
					// Check if not overcompenstating
					if (newSepVel.sqrMagnitude < 0) newSepVel = Vector3.zero;
				}*/


				// Calculate change in velocity
				Vector3 deltaVel = newSepVel - sepVel;

				// To accont for mass differences, so heavier things are effected "less"
				float totalInvMass = a.GetInvMass();
				if (b) totalInvMass += b.GetInvMass();

				// If inf mass
				if (totalInvMass <= 0) return;

				// Calculate impulse
				Vector3 impulse = deltaVel * (1 / totalInvMass);

				// Amount of impulse per unit of inv mass
				Vector3 impulsePerIMass = impulse;
				//Vector3 impulsePerIMass = Vector3.Reflect(impulse,normal);
				//Debug.Log("in: " + impulse.ToString("F2") + ", out: " + impulsePerIMass.ToString("F2"));

				// Apply impulses in direction of contact
				//a.SetVelocity(a.GetVelocity() + impulsePerIMass * a.GetInvMass());
				Vector3 reflectedVel = Vector3.Reflect(a.GetVelocity(), normal);
				//Debug.Log(a.GetVelocity() + " -> " + reflectedVel);
				a.SetVelocity(reflectedVel*restitution);

				// If b exists then send it in opposite direction
				if (b)
					b.SetVelocity(b.GetVelocity() + impulsePerIMass * b.GetInvMass() * -1.0f);

				//Debug.Log("= = Resolved Velocity = =\nNew A velocity " + a.GetVelocity().ToString("F3") + " | New B velocity: " + b.GetVelocity().ToString("F3"));
			}

			public void ResolveInterpenetration(float duration, Particle3D a, Particle3D b = null)
			{
				// If no penetration then no resolving needed
				if (penetration <= 0) return;

				float totalInvMass = a.GetInvMass();
				if (b)
					totalInvMass += b.GetInvMass();

				//Debug.Log("totalInvMass = "+totalInvMass);

				// If inf mass: nothing moves
				if (totalInvMass <= 0) return;

				// Find penetration resolution amont per 1/kg
				Vector3 movePerIMass = normal * (penetration / totalInvMass);

				// Calculate movement amounts and apply them
				Vector3 aMoveAmont = Vector3.zero, bMoveAmont = Vector3.zero;

				// For a
				aMoveAmont = movePerIMass * a.GetInvMass();
				a.SetPosition(a.GetPosition() + aMoveAmont);
				//Debug.Log("aMoveAmont = " + movePerIMass + " * " + a.GetInvMass());

				// For b
				// If b, then calculate move amount in opposite direction
				if (b)
				{
					bMoveAmont = -movePerIMass * b.GetInvMass();
					b.SetPosition(b.GetPosition() + bMoveAmont);
				}

				//Debug.Log("= = Resolved Penetration (" + penetration.ToString("F2") + ") = =\nA moved " + aMoveAmont.ToString("F3") + " | B moved: " + bMoveAmont.ToString("F3"));
			}
		}

		public CollisionHull3D a = null, b = null;
		public bool status = false;
		public Contact[] contacts = new Contact[4];
		public int contactCount = 0;

		public float closingVelocity = 0.0f;
	}

	public enum CollisionHullType3D
	{
		hull_sphere,
		hull_aabb,
		hull_obb,
		hull_complex
	}

	CollisionHullType3D type { get; }

	protected Particle3D particle;

	public Particle3D GetParticle3D() { return particle; }
	public void SetParticle3D(Particle3D p) { particle = p; }
	
	protected CollisionHull3D(CollisionHullType3D type_set)
	{
		type = type_set;
	}

	// Start is called before the first frame update
	void Awake()
	{
		particle = gameObject.GetComponent<Particle3D>();
	}

	public abstract void ManualUpdate();

	public static bool TestCollision(CollisionHull3D a, CollisionHull3D b, ref Collision c)
	{
		bool pass = false;

		switch (b.type)
		{
			case CollisionHullType3D.hull_sphere:
				pass = a.TestCollisionVsSphere(b.GetComponent<SphereCollisionHull3D>(), ref c);
				break;
			case CollisionHullType3D.hull_aabb:
				pass = a.TestCollisionVsAABB(b.GetComponent<AABBCollisionHull3D>(), ref c);
				break;
			case CollisionHullType3D.hull_obb:
				pass = a.TestCollisionVsOBB(b.GetComponent<OBBCollisionHull3D>(), ref c);
				break;
			case CollisionHullType3D.hull_complex:
				pass = a.TestCollisionVsComplex(b.GetComponent<ComplexCollisionHull3D>(), ref c);
				break;
			default:
				pass = false;
				break;
		}

		// Objects involved;
		c.a = a;
		c.b = b;
		Vector3 posDiff = a.GetParticle3D().GetPosition() - b.GetParticle3D().GetPosition();
		c.closingVelocity = -1.0f * DotProd(posDiff, posDiff.normalized);

		return pass;
	}

	static float DotProd(Vector3 a, Vector3 b)
	{
		return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
	}

	public abstract bool TestCollisionVsSphere(SphereCollisionHull3D other, ref Collision c);

	public abstract bool TestCollisionVsAABB(AABBCollisionHull3D other, ref Collision c);

	public abstract bool TestCollisionVsOBB(OBBCollisionHull3D other, ref Collision c);

	public abstract bool TestCollisionVsComplex(ComplexCollisionHull3D other, ref Collision c);
}