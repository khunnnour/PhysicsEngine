using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle2D : MonoBehaviour
{
	/* - - VARIABLES - - */
	// LAB 1 STEP 1
	public enum UpdateType { Euler, Kinematic }
	public enum Shapes { Circle, Rectangle, ThinRod }

	[Tooltip("How far on the axis before it wraps")]
	public Vector2 wrapBounds = new Vector2(30, 30);

	[Header("Position Settings")]
	public UpdateType PosUpdateType;
	public Vector2 startPosition, startVelocity, startAcceleration;
	private Vector2 position, velocity, acceleration;

	[Header("Rotation Settings")]
	public UpdateType RotUpdateType;
	public float startRotation, startAngVelocity, startAngAcceleration;
	private float rotation, angVelocity, angAcceleration;


	// LAB 2 STEP 1
	[Header("Force Settings")]
	public float startingMass = 1.0f;

	[System.Serializable]
	public struct GravitySettings
	{
		public bool useGravity;
		public float gravity;
		public bool useNormal;
		public bool useSlide;
	}

	[System.Serializable]
	public struct FrictionSettings
	{
		public bool useStaticFriction;
		public float staticFrictionCoef;

		public bool useKineticFriction;
		public float kineticFrictionCoef;
	}

	[System.Serializable]
	public struct DragSettings
	{
		public bool useDrag;
		public Vector2 fluidVelocity;
		public float liquidDensity;
		/* water: 997 kg/m3
		 * air: 1.225 kg/m3 (15C at sea level)
		 * air: 1.2754 kg/m3 (0C, 100kPa for dry air)
		 */
		public float crossArea;
		public float dragCoef;
		/* Angled Cube: 0.80
		 * Cube: 1.05
		 * Streamlined body: 0.04
		 * Sphere: 0.47
		 */
	}

	[System.Serializable]
	public struct SpringSettings
	{
		public bool useSpring;
		public Vector2 anchorPoint;
		public float springConstant;
		public float springLength;
		public float springDampening;
	}

	// LAB 3 STEP 1
	[System.Serializable]
	public struct TorqueSettings
	{
		public bool useTorque;
		public Shapes shape;
		public Vector2 localCenterMass;
		public Vector2 forceApplied;
		public Vector2 pointApplied;
		public float torque;
	}

	private float mass, massInv;
	private float momentI, momentIInv;
	readonly float maxSpeed = 15.0f;

	// HOLD CURRENT FORCE VALUES
	Vector2 f_gravity, f_normal;

	// LAB 2 STEP 2
	Vector2 force;

	public GravitySettings gravitySettings;
	public FrictionSettings frictionSettings;
	public DragSettings dragSettings;
	public SpringSettings springSettings;
	public TorqueSettings torqueSettings; // LAB 3 STEP 1


	/* - - START-UP KIND OF STUFF - - */
	public void SetMass(float newMass)
	{
		//mass = newMass > 0.0f ? newMass : 0.0f;
		mass = Mathf.Max(0.0f, newMass);
		massInv = mass > 0.0f ? 1.0f / mass : 0.0f;
	}
	public float GetMass()
	{
		return mass;
	}
	public float GetInvMass()
	{
		return massInv;
	}

	public void SetMoment()
	{
		Vector3 size = GetComponent<Renderer>().bounds.size;

		switch (torqueSettings.shape)
		{
			case Shapes.Circle:
				// Circle:  Iz = pi/2 * r^4
				//momentI = Mathf.PI * 0.5f * size.x * size.x * size.x * size.x;
				/* 3D Sphere: I =	[ 2/5mr^2  0  0 ]
				 *					[ 0	 2/5mr^2  0 ]
				 *					[ 0	 0  2/5mr^2 ]
				 */
				momentI = 0.5f * mass * size.x * size.x;
				break;
			case Shapes.Rectangle:
				// Rectangle:  Ix = bh^3/12  Iy = b^3h/12
				//momentI = Mathf.PI * 0.5f * size.x * size.x * size.x * size.x;
				/* 3D Rect: I =	[ m(h^2+d^2)/12   0   0 ]
				 *				[ 0	  m(w^2+d^2)/12   0 ]
				 *				[ 0	  0   m(w^2+h^2)/12 ]
				 */
				momentI = mass * (size.x * size.x + size.y * size.y) / 12.0f;
				break;
			case Shapes.ThinRod:
				momentI = mass * (size.y * size.y) / 12.0f;
				break;
			default:
				break;
		}

		momentIInv = 1.0f / momentI;
	}

	public Vector2 GetPosition() { return position; }
	public void SetPosition(Vector2 newP) { position = newP; }

	public Vector2 GetVelocity() { return velocity; }
	public void SetVelocity(Vector2 newV) { velocity = newV; }

	public Vector2 GetAcceleration() { return acceleration; }

	public float GetRotation() { return rotation; }

	private void ResetPosAndRot()
	{
		position = startPosition;
		velocity = startVelocity;
		acceleration = startAcceleration;

		rotation = startRotation;
		angVelocity = startAngVelocity;
		angAcceleration = startAngAcceleration;
	}

	void Start()
	{
		SetMass(startingMass);
		SetMoment();
		ResetPosAndRot();

		// Randomize acceleration
		//acceleration.x = UnityEngine.Random.Range(-250.0f, 250.0f);
		//acceleration.y = UnityEngine.Random.Range(-250.0f, 250.0f);
	}


	/* - - UPDATE FUNCTIONS - - */
	public void AddForce(Vector2 newForce)
	{
		// D'Alembert
		force += newForce;
	}

	public void UpdateAcceleration()
	{
		acceleration = force * massInv;

		//Debug.Log("Current Acc: " + acceleration.ToString("F4"));

		// reset force
		force.Set(0.0f, 0.0f);
	}

	// LAB 3 STEP 2
	public void AddTorque(float newTorque)
	{
		torqueSettings.torque += newTorque;
	}

	public void UpdateAngAcceleration()
	{
		// Newton 2
		angAcceleration = torqueSettings.torque * momentIInv;

		//Debug.Log("Final T: " + torqueSettings.torque.ToString("F4"));
		//Debug.Log(gameObject.name + " Ang Acc: " + angAcceleration.ToString("F2") + " = " + torqueSettings.torque.ToString("F2") + " * " + momentIInv.ToString("F2"));

		// Reset torque
		torqueSettings.torque = 0.0f;
	}

	// LAB 1 STEP 2
	void UpdatePositionEulerExplicit(float dt)
	{
		// x(t+dt) = x(t) + v(t)dt
		// To find the next step, add current velocity to current position
		position += velocity * dt;

		// v(t+dt) = v(t) + a(t)dt
		velocity += acceleration * dt;

		// Cap Velocity
		
		if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
			velocity = velocity.normalized * maxSpeed;
	}

	void UpdatePositionKinematic(float dt)
	{
		position += (velocity * dt) + (0.5f * acceleration * dt * dt);
		velocity += acceleration * dt;
		//Debug.Log("Current Vel: " + velocity.ToString("F4"));

		// Cap Velocity
		if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
			velocity = velocity.normalized * maxSpeed;
	}

	void UpdateRotationEulerExplicit(float dt)
	{
		rotation += angVelocity * dt;
		angVelocity += angAcceleration * dt;

		//Debug.Log(gameObject.name+" ROT: " + rotation.ToString("F2") + " += " + angVelocity.ToString("F2") + " * " + dt.ToString("F2"));
	}

	void UpdateRotationKinematic(float dt)
	{
		rotation += (angVelocity * dt) + (0.5f * angAcceleration * dt * dt);
		angVelocity += angAcceleration * dt;
	}

	// Reset stuff if you change variables
	private void OnValidate()
	{
		SetMass(startingMass);
		ResetPosAndRot();
		SetMoment();
	}

	// Reset position if off screen
	private void CheckBounds()
	{
		if (Mathf.Abs(transform.position.x) > wrapBounds.x || Mathf.Abs(transform.position.y) > wrapBounds.y)
			Wrap();
	}

	private void Wrap()
	{
		if (position.x > wrapBounds.x)
			position = new Vector2(-wrapBounds.x + 1.0f, position.y);
		if (position.x < -wrapBounds.x)
			position = new Vector2(wrapBounds.x - 1.0f, position.y);

		if (position.y > wrapBounds.y)
			position = new Vector2(position.x, -wrapBounds.y + 1.0f);
		if (position.y < -wrapBounds.y)
			position = new Vector2(position.x, wrapBounds.y - 1.0f);
	}


	/* - - MAIN UPDATE - - */
	void FixedUpdate()
	{
		// LAB 1 STEP 3
		// Choose integration
		// Position
		if (PosUpdateType == UpdateType.Euler)
			UpdatePositionEulerExplicit(Time.fixedDeltaTime);
		else
			UpdatePositionKinematic(Time.fixedDeltaTime);

		// Rotation
		if (RotUpdateType == UpdateType.Euler)
			UpdateRotationEulerExplicit(Time.fixedDeltaTime);
		else
			UpdateRotationKinematic(Time.fixedDeltaTime);


		// LAB 2 STEP 3
		if (mass > 0)
		{
			UpdateAcceleration();
			UpdateAngAcceleration();
		}

		// Update position
		transform.position = position;

		// Update rotation
		float deg = (rotation * 180f) / Mathf.PI;
		transform.rotation = Quaternion.Euler(0.0f, 0.0f, deg);
		//Debug.Log("deg = (" + rotation.ToString("F2") + " * 180.0f) / " + Mathf.PI.ToString("F2"));

		// LAB 1 STEP 4
		/*
		acceleration.x = -Mathf.Sin(Time.time) * 2.0f;
        acceleration.y = -Mathf.Cos(Time.time) * 2.0f;

        angAcceleration = -Mathf.Sin(Time.time) * Mathf.PI;
		*/

		// LAB 2 Apply forces
		if (gravitySettings.useGravity)
		{
			f_gravity = ForceGenerator.GenerateForce_Gravity(mass, gravitySettings.gravity, Vector2.up);
			AddForce(f_gravity);
		}

		if (gravitySettings.useNormal)
		{
			f_normal = ForceGenerator.GenerateForce_Normal(mass, gravitySettings.gravity, rotation, transform.up);
			AddForce(f_normal);
		}

		if (gravitySettings.useSlide)
			AddForce(ForceGenerator.GenerateForce_Sliding(mass, gravitySettings.gravity, rotation, Vector2.up, transform.up));

		// TODO: FRICTION FORCE STUFF IMPLEMENT MAX OPPOSING FORCE

		if (frictionSettings.useStaticFriction)
			AddForce(ForceGenerator.GenerateForce_Friction_Static(mass, gravitySettings.gravity, rotation, frictionSettings.staticFrictionCoef, Vector2.up, transform.up));

		if (frictionSettings.useKineticFriction)
			AddForce(ForceGenerator.GenerateForce_Friction_Kinetic(f_normal, velocity, frictionSettings.kineticFrictionCoef));

		if (dragSettings.useDrag)
			AddForce(ForceGenerator.GenerateForce_Drag(velocity, dragSettings.fluidVelocity, dragSettings.liquidDensity, dragSettings.crossArea, dragSettings.dragCoef));

		if (springSettings.useSpring)
			AddForce(ForceGenerator.GenerateForce_Spring(transform.position, springSettings.anchorPoint, springSettings.springLength, springSettings.springConstant, springSettings.springDampening, Time.time));

		if (torqueSettings.useTorque)
			AddTorque(ForceGenerator.GenerateForce_Torque(torqueSettings.forceApplied, torqueSettings.pointApplied, torqueSettings.localCenterMass));

		// Reset if way off screen
		CheckBounds();
	}
}