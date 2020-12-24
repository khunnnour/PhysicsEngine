using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollisionHull2D : MonoBehaviour
{
	public class Collision
	{
		public struct Contact
		{
			public Vector2 point;
			public Vector2 normal;
			public float restitution;
			public float penetration;

			Vector2 CalcSeparateVelocity(Particle2D a, Particle2D b = null)
			{
				// Calculate relative velocity
				Vector2 relVel = a.GetVelocity();

				if (b)
					relVel -= b.GetVelocity();

				// Return rel velocity 
				return relVel * normal;
			}

			public void ResolveVelocity(float duration, Particle2D a, Particle2D b = null)
			{
				Vector2 sepVel = CalcSeparateVelocity(a, b);

				// Check if needs to be resolved
				/*if (sepVel.sqrMagnitude > 0)
					return;*/

				// Calculate new separation velocity based on restitution
				Vector2 newSepVel = -sepVel * restitution;

				// Check vel build-up based on acceleration
				Vector2 accCausedVelocity = a.GetAcceleration();
				if (b) accCausedVelocity -= b.GetAcceleration();

				Vector2 accCausedSepVelocity = accCausedVelocity * normal * duration;
				// Remove closing vel from acceleration
				//if(accCausedSepVelocity<0)
				// TODO: figure out how to do these checks
				/* {
				 * newSepVel+=restitution*accCausedSepVelocity;
				 * // Check if not overcompenstating
				 * if(newSepVal<0)newSepVel=0;
				 * }
				 */

				// Calculate change in velocity
				Vector2 deltaVel = newSepVel - sepVel;

				// To accont for mass differences, so heavier things are effected "less"
				float totalInvMass = a.GetInvMass();
				if (b) totalInvMass += b.GetInvMass();
				
				// If inf mass
				if (totalInvMass <= 0) return;
				 
				// Calculate impulse
				Vector2 impulse = deltaVel * (1 / totalInvMass);

				// Amount of impulse per unit of inv mass
				Vector2 impulsePerIMass = normal * impulse;

				// Apply impulses in direction of contact
				a.SetVelocity(a.GetVelocity() + impulsePerIMass * a.GetInvMass());

				// If b exists then send it in opposite direction
				if (b)
					b.SetVelocity(b.GetVelocity() + impulsePerIMass * b.GetInvMass() * -1.0f);

				//Debug.Log("= = Resolved Velocity = =\nNew A velocity " + a.GetVelocity().ToString("F3") + " | New B velocity: " + b.GetVelocity().ToString("F3"));
			}

			public void ResolveInterpenetration(float duration, Particle2D a, Particle2D b = null)
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
				Vector2 movePerIMass = normal * (penetration / totalInvMass);

				// Calculate movement amounts and apply them
				Vector2 aMoveAmont = Vector2.zero, bMoveAmont = Vector2.zero;

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

		public CollisionHull2D a = null, b = null;
		public bool status = false;
		public Contact[] contacts = new Contact[4];
		public int contactCount = 0;

		public float closingVelocity = 0.0f;
	}

	public enum CollisionHullType2D
	{
		hull_circle,
		hull_aabb,
		hull_obb
	}

	CollisionHullType2D type { get; }

	protected Particle2D particle;

	public Particle2D GetParticle2D() { return particle; }
	public void SetParticle2D(Particle2D p) { particle = p; }

	protected CollisionHull2D(CollisionHullType2D type_set)
	{
		type = type_set;
	}

	// Start is called before the first frame update
	void Awake()
	{
		particle = gameObject.GetComponent<Particle2D>();
	}

	// Update is called once per frame
	void Update()
	{
	}

	public static bool TestCollision(CollisionHull2D a, CollisionHull2D b, ref Collision c)
	{
		bool pass;

		switch (b.type)
		{
			case CollisionHullType2D.hull_circle:
				pass = a.TestCollisionVsCircle(b.GetComponent<CircleCollisionHull2D>(), ref c);
				break;
			case CollisionHullType2D.hull_aabb:
				pass = a.TestCollisionVsAABB(b.GetComponent<AxisAlignedBoundingBoxCollisionHull2D>(), ref c);
				break;
			case CollisionHullType2D.hull_obb:
				pass = a.TestCollisionVsOBB(b.GetComponent<ObjectBoundingBoxCollisionHull2D>(), ref c);
				break;
			default:
				pass = false;
				Debug.Log("CollisionHull2D | TestCollision(): Unknown Collider");
				break;
		}

		// Objects involved;
		c.a = a;
		c.b = b;
		Vector2 posDiff = a.GetParticle2D().GetPosition() - b.GetParticle2D().GetPosition();
		c.closingVelocity = -1.0f * DotProd(posDiff, posDiff.normalized);

		return pass;
	}

	static float DotProd(Vector2 a, Vector2 b)
	{
		return (a.x * b.x) + (a.y * b.y);
	}

	public abstract bool TestCollisionVsCircle(CircleCollisionHull2D other, ref Collision c);

	public abstract bool TestCollisionVsAABB(AxisAlignedBoundingBoxCollisionHull2D other, ref Collision c);

	public abstract bool TestCollisionVsOBB(ObjectBoundingBoxCollisionHull2D other, ref Collision c);
}