using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle3D : MonoBehaviour
{
	/* - - VARIABLES - - */
	public enum UpdateType { Euler, Kinematic }
	public enum Shapes { Sphere, HollowSphere, Box, HollowBox, Cylinder, Cone }

	[Tooltip("Leave at (0,0,0) to use renderer bounds")]
	public Vector3 size = Vector3.zero;

	[Tooltip("How far on the axis before it wraps")]
	public Vector3 wrapBounds = new Vector3(30f, 30f, 30f);

	[Header("Force Settings")]
	public float startingMass = 1.0f;

	[Header("Position Settings")]
	public UpdateType PosUpdateType;
	public Vector3 startPosition, startVelocity, startAcceleration;
	private Vector3 position, velocity, acceleration;
	public bool lockPosition = false;

	[Header("Rotation Settings")]
	//public UpdateType RotUpdateType;
	public Quaternion startRotation, rotation;
	public Vector3 startAngVelocity, startAngAcceleration;
	private Vector3 angVelocity, angAcceleration;

	[Header("Force Test Vars")]
	public Vector3 forceApplied;
	public Vector3 tauForce, tauAppliedPoint;


	[System.Serializable]
	public struct GravitySettings
	{
		public bool useGravity;
		public bool useNormal;
	}

	[System.Serializable]
	public struct FrictionSettings
	{
		public bool useFriction;

		public float staticFrictionCoef;
		public float kineticFrictionCoef;
	}

	[System.Serializable]
	public struct DragSettings
	{
		public bool useDrag;
		public Vector3 fluidVelocity;
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
		public Vector3 anchorPoint;
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
		public Vector3 localCenterMass;
		public bool lockXRot;
		public bool lockYRot;
		public bool lockZRot;
	}

	private float maxSpeed = 50.0f;

	private float mass, massInv;
	private Matrix4x4 momentI, momentIInv, modMomentIInv;
	private Matrix4x4 transMat, transMatInv;
	private Vector3 worldCenterMass;

	// HOLD CURRENT FORCE VALUES
	Vector3 f_gravity, f_normal;
	Vector3 force;
	Vector3 torque;

	public GravitySettings gravitySettings;
	public FrictionSettings frictionSettings;
	public DragSettings dragSettings;
	public SpringSettings springSettings;
	public TorqueSettings torqueSettings;


	// MASS/MOMENT EQUATIONS
	public void SetMass(float newMass)
	{
		//mass = newMass > 0.0f ? newMass : 0.0f;
		mass = Mathf.Max(0.0f, newMass);
		massInv = mass > 0.0f ? 1.0f / mass : 0.0f;
	}
	public float GetMass() { return mass; }
	public float GetInvMass() { return massInv; }

	public void SetMoment()
	{
		momentI = Matrix4x4.zero;
		momentI[3, 3] = 1.0f;

		float radius = size.x / 2f;
		float radiusSqr = radius * radius;

		switch (torqueSettings.shape)
		{
			case Shapes.Sphere:
				/* 3D Sphere: I =	[ 2/5mr^2  0  0 ]
				 *					[ 0	 2/5mr^2  0 ]
				 *					[ 0	 0  2/5mr^2 ]
				 */
				float val = 0.4f * mass * radiusSqr;
				momentI[0, 0] = val;
				momentI[1, 1] = val;
				momentI[2, 2] = val;
				break;
			case Shapes.HollowSphere:
				/* 3D Hollow Sphere: I = [ 2/3mr^2  0  0 ]
				 *						 [ 0  2/3mr^2  0 ]
				 *						 [ 0  0  2/3mr^2 ]
				 */
				val = (2f / 3f) * mass * radiusSqr;
				momentI[0, 0] = val;
				momentI[1, 1] = val;
				momentI[2, 2] = val;
				break;
			case Shapes.Box:
				/* 3D Box: I =	[ m(h^2+d^2)/12   0   0 ]
				 *				[ 0	  m(w^2+d^2)/12   0 ]
				 *				[ 0	  0   m(w^2+h^2)/12 ]
				 */
				momentI[0, 0] = mass * (size.y * size.y + size.z * size.z) / 12.0f;
				momentI[1, 1] = mass * (size.x * size.x + size.z * size.z) / 12.0f;
				momentI[2, 2] = mass * (size.x * size.x + size.y * size.y) / 12.0f;
				break;
			case Shapes.HollowBox:
				/* 3D Box: I =	[ (5/3)m(h^2+d^2)   0   0 ]
				 *				[ 0	  (5/3)m(w^2+d^2)   0 ]
				 *				[ 0	  0   (5/3)m(w^2+h^2) ]
				 */
				val = 5f / 3f;
				momentI[0, 0] = val * mass * (size.y * size.y + size.z * size.z);
				momentI[1, 1] = val * mass * (size.x * size.x + size.z * size.z);
				momentI[2, 2] = val * mass * (size.x * size.x + size.y * size.y);
				break;
			case Shapes.Cylinder:
				/* 3D Cylinder: I =	[ m(3r^2+h^2)/12   0   0 ]
				 *					[ 0	  m(3r^2+h^2)/12   0 ]
				 *					[ 0	  0         0.5*mr^2 ]
				 */
				momentI[0, 0] = mass * (3f * radiusSqr + size.y * size.y) / 12.0f;
				momentI[1, 1] = mass * (3f * radiusSqr + size.y * size.y) / 12.0f;
				momentI[2, 2] = mass * radiusSqr / 2.0f;
				break;
			case Shapes.Cone:
				/* 3D Cone: I =	[ (3/5)m*h^2+(3/20)mr^2   0   0 ]
				 *				[ 0	  (3/5)m*h^2+(3/20)mr^2   0 ]
				 *				[ 0	  0             (3/10)*mr^2 ]
				 */
				momentI[0, 0] = 0.6f * mass * size.y * size.y + 0.15f * mass * radiusSqr;
				momentI[1, 1] = 0.6f * mass * size.y * size.y + 0.15f * mass * radiusSqr;
				momentI[2, 2] = 0.3f * mass * radiusSqr;
				break;
			default:
				break;
		}

		// Calculate inverse moment
		momentIInv = Matrix4x4.zero;
		momentIInv[0, 0] = 1.0f / momentI[0, 0];
		momentIInv[1, 1] = 1.0f / momentI[1, 1];
		momentIInv[2, 2] = 1.0f / momentI[2, 2];
		momentIInv[3, 3] = 0.0f;
	}

	void CalculateModInvMoment()
	{
		// Calculate modified moment inverse
		modMomentIInv = transMat * momentIInv * transMatInv;
	}

	// TRANSFORMATION MATRIX
	public void CalculateTransMat()
	{
		// Update center of mass
		worldCenterMass = position + torqueSettings.localCenterMass;

		/* - Calculate Transformation Matrix - */
		transMat[0, 0] = rotation.w * rotation.w + rotation.x * rotation.x - rotation.y * rotation.y - rotation.z * rotation.z;
		transMat[0, 1] = 2f * (rotation.x * rotation.y - rotation.w * rotation.z);
		transMat[0, 2] = 2f * (rotation.x * rotation.z + rotation.w * rotation.y);
		transMat[0, 3] = position.x;

		transMat[1, 0] = 2f * (rotation.x * rotation.y + rotation.w * rotation.z);
		transMat[1, 1] = rotation.w * rotation.w - rotation.x * rotation.x + rotation.y * rotation.y - rotation.z * rotation.z;
		transMat[1, 2] = 2f * (rotation.y * rotation.z - rotation.w * rotation.x);
		transMat[1, 3] = position.y;

		transMat[2, 0] = 2f * (rotation.x * rotation.z - rotation.w * rotation.y);
		transMat[2, 1] = 2f * (rotation.y * rotation.z + rotation.w * rotation.x);
		transMat[2, 2] = rotation.w * rotation.w - rotation.x * rotation.x - rotation.y * rotation.y + rotation.z * rotation.z;
		transMat[2, 3] = position.z;

		/* - Calculate inverse - */
		// Transpose rotation
		transMatInv[0, 0] = transMat[0, 0];
		transMatInv[0, 1] = transMat[1, 0];
		transMatInv[0, 2] = transMat[2, 0];

		transMatInv[1, 0] = transMat[0, 1];
		transMatInv[1, 1] = transMat[1, 1];
		transMatInv[1, 2] = transMat[2, 1];

		transMatInv[2, 0] = transMat[0, 2];
		transMatInv[2, 1] = transMat[1, 2];
		transMatInv[2, 2] = transMat[2, 2];

		// Negate translation
		transMatInv[0, 3] = -(transMatInv[0, 0] * position.x + transMatInv[0, 1] * position.y + transMatInv[0, 2] * position.z);
		transMatInv[1, 3] = -(transMatInv[1, 0] * position.x + transMatInv[1, 1] * position.y + transMatInv[1, 2] * position.z);
		transMatInv[2, 3] = -(transMatInv[2, 0] * position.x + transMatInv[2, 1] * position.y + transMatInv[2, 2] * position.z);
	}
	public Matrix4x4 GetTransMat() { return transMat; }
	public Matrix4x4 GetInvTransMat() { return transMatInv; }

	// ACCERLERATION FUNCTIONS
	public void AddForce(Vector3 newForce)
	{
		// D'Alembert
		force += newForce;
	}

	public void UpdateAcceleration()
	{
		acceleration = force * massInv;

		//Debug.Log("Current Acc: " + acceleration.ToString("F3"));

		// reset force
		force.Set(0.0f, 0.0f, 0.0f);
	}
	public Vector3 GetAcceleration() { return acceleration; }

	// TORQUE FUNCTIONS
	public void AddTorque(Vector3 newTorque)
	{
		// Add new torque
		torque += newTorque;
	}
	public Vector3 GetTorque() { return torque; }

	public void UpdateAngAcceleration()
	{
		// Newton 2 using pre-change-of-based matrix
		angAcceleration = modMomentIInv * torque;

		//Debug.Log("Final T: " + torqueSettings.torque.ToString("F3"));
		//Debug.Log(gameObject.name + " Ang Acc: " + angAcceleration.ToString("F3") + " = " + torqueSettings.torque.ToString("F3") + " * " + momentIInv.ToString("F3"));

		// Reset torque
		torque.Set(0.0f, 0.0f, 0.0f);
	}


	// RESET FUNCTIONS
	private void ResetPosAndRot()
	{
		position = startPosition;
		velocity = startVelocity;
		acceleration = startAcceleration;

		rotation = startRotation.normalized;
		angVelocity = startAngVelocity;
		angAcceleration = startAngAcceleration;
	}

	/**/
	private void OnValidate()
	{
		// Reset stuff if you change variables
		SetMass(startingMass);
		ResetPosAndRot();
		SetMoment();
	}

	// Reset position if off screen
	private void CheckBounds()
	{
		if (Mathf.Abs(transform.position.x) > wrapBounds.x || Mathf.Abs(transform.position.y) > wrapBounds.y || Mathf.Abs(transform.position.z) > wrapBounds.z)
			Wrap();
	}

	private void Wrap()
	{
		if (position.x > wrapBounds.x)
			position = new Vector3(-wrapBounds.x + 1.0f, position.y, position.z);
		if (position.x < -wrapBounds.x)
			position = new Vector3(wrapBounds.x - 1.0f, position.y, position.z);

		if (position.y > wrapBounds.y)
			position = new Vector3(position.x, -wrapBounds.y + 1.0f, position.z);
		if (position.y < -wrapBounds.y)
			position = new Vector3(position.x, wrapBounds.y - 1.0f, position.z);

		if (position.z > wrapBounds.z)
			position = new Vector3(position.x, wrapBounds.y, -position.z + 1.0f);
		if (position.z < -wrapBounds.z)
			position = new Vector3(position.x, wrapBounds.y, position.z - 1.0f);
	}

	// POSITION FUNCTIONS
	void UpdatePositionEulerExplicit(float dt)
	{
		// x(t+dt) = x(t) + v(t)dt <- derivative of x(t)
		//				 (+ dx/dt * dt)
		if (!lockPosition)
			position += velocity * dt;
		else
			position = startPosition;
	}

	void UpdatePositionKinematic(float dt)
	{
		// x(t+dt) = x(t) + v(dt) + 0.5a(dt^2)
		if (!lockPosition)
			position += (velocity * dt) + (0.5f * acceleration * dt * dt);
		else
			position = startPosition;
	}
	public Vector3 GetPosition() { return position; }
	public void SetPosition(Vector3 newP) { position = newP; transform.position = newP; }

	// VELOCITY FUNCTIONS
	void UpdateVelocityEulerExplicit(float dt)
	{
		// v(t+dt) = v(t) + a(t)dt
		velocity += acceleration * dt;

		// Cap Velocity
		if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
			velocity = velocity.normalized * maxSpeed;
	}
	public Vector3 GetVelocity() { return velocity; }
	public void SetVelocity(Vector3 newV) { velocity = newV; }

	// ROTATION FUNCTIONS
	void UpdateRotationEulerExplicit(float dt)
	{
		// EULER EXPLICIT FORMULA
		// x(t+dt) = x(t) + v(t)dt <- derivative of x(t)
		//				 (+ dx/dt * dt)

		// angVel as a quaternion
		Vector3 AngVelocityQuat = new Vector3(angVelocity.x, angVelocity.y, angVelocity.z);

		// q(t+dt) = q(t) + angVel(t) * q(t) * 0.5
		AngVelocityQuat *= 0.5f;

		// Lock rotation
		if (torqueSettings.lockXRot)
			AngVelocityQuat.x = 0.0f;
		if (torqueSettings.lockYRot)
			AngVelocityQuat.y = 0.0f;
		if (torqueSettings.lockZRot)
			AngVelocityQuat.z = 0.0f;

		rotation = AddQuats(rotation, MultiplyQuatVec3(rotation, AngVelocityQuat));

		// Normalize result to get a pure quaternion
		rotation.Normalize();

		angVelocity += angAcceleration * dt;
	}
	public Quaternion GetRotation() { return rotation; }

	public void KillAllMovement()
	{
		velocity = Vector3.zero;
		acceleration = Vector3.zero;
		angVelocity = Vector3.zero;
		angAcceleration = Vector3.zero;
		rotation = Quaternion.Euler(0f, 0f, 0f);
	}

	void Start()
	{
		if (size == Vector3.zero)
			size = GetComponent<Renderer>().bounds.size;

		SetMass(startingMass);
		SetMoment();
		ResetPosAndRot();

		transMat = Matrix4x4.zero;
		transMat[3, 3] = 1.0f;
		transMatInv = Matrix4x4.zero;
		transMatInv[3, 3] = 1.0f;
		CalculateTransMat();
		//Debug.Log(gameObject.name + " transmat:\n" + transMat.ToString("F3"));

		modMomentIInv = Matrix4x4.zero;
		modMomentIInv[3, 3] = 1.0f;
		CalculateModInvMoment();
	}

	/* - - MAIN UPDATE - - */
	void FixedUpdate()
	{
		// Update transformation matrices
		CalculateTransMat();
		CalculateModInvMoment();

		// Update position
		if (PosUpdateType == UpdateType.Euler)
			UpdatePositionEulerExplicit(Time.fixedDeltaTime);
		else
			UpdatePositionKinematic(Time.fixedDeltaTime);

		UpdateVelocityEulerExplicit(Time.fixedDeltaTime);

		transform.position = position;

		// Update rotation
		UpdateRotationEulerExplicit(Time.fixedDeltaTime);
		transform.rotation = rotation;

		/* - APPLY FORCES - */
		if (gravitySettings.useGravity)
		{
			f_gravity = ForceGenerator3D.instance.GenerateForce_Gravity(mass, Vector3.up);
			AddForce(f_gravity);
			//Debug.Log("Added f_gravity: " + f_gravity.ToString("F3"));
		}

		if (gravitySettings.useNormal)
		{
			f_normal = ForceGenerator3D.instance.GenerateForce_Normal(mass, Vector3.up, transform.up);
			AddForce(f_normal);
			//Debug.Log("Added f_normal: " + f_normal.ToString("F3"));
		}

		if (frictionSettings.useFriction)
			AddForce(ForceGenerator3D.instance.GenerateForce_Friction(frictionSettings.staticFrictionCoef, frictionSettings.kineticFrictionCoef, mass, velocity, Vector3.up, transform.up));

		if (dragSettings.useDrag)
			AddForce(ForceGenerator3D.instance.GenerateForce_Drag(velocity, dragSettings.fluidVelocity, dragSettings.liquidDensity, dragSettings.crossArea, dragSettings.dragCoef));

		if (springSettings.useSpring)
			AddForce(ForceGenerator3D.instance.GenerateForce_Spring(transform.position, springSettings.anchorPoint, springSettings.springLength, springSettings.springConstant));

		if (torqueSettings.useTorque)
			AddTorque(ForceGenerator3D.instance.GenerateForce_Torque(tauForce, tauAppliedPoint, worldCenterMass));

		// Update accelerations
		UpdateAcceleration();
		UpdateAngAcceleration();

		// Display stuff
		/*Debug.Log("= = = " + gameObject.name + " = = =\n"
		+ momentI.ToString("F4") + "\n"
		+ "Ang acc: " + angAcceleration.ToString("F3"));*/

		// Reset position if off screen
		CheckBounds();
	}

	/* - QUATERNION FUNCTIONS - */
	public void ScalarMultiplyQuat(ref Quaternion q, float f)
	{
		q.w *= f;
		q.x *= f;
		q.y *= f;
		q.z *= f;
	}

	// Multiply two quaternions together
	public Quaternion ConcatenateQuats(Quaternion lhs, Quaternion rhs)
	{
		Vector3 oneVecComp = new Vector3(lhs.x, lhs.y, lhs.z);
		Vector3 twoVecComp = new Vector3(rhs.x, rhs.y, rhs.z);

		Quaternion newQuat = new Quaternion
		{
			// wN = w0w1 - v0 dot v1
			w = lhs.w * rhs.w - Vector3.Dot(oneVecComp, twoVecComp),
			// vN = w0v1 + w1v0 + v0 cross v1	
			eulerAngles = (lhs.w * twoVecComp) + (rhs.w * oneVecComp) + (Vector3.Cross(oneVecComp, twoVecComp))
		};

		return newQuat;
	}

	// Add two quaternions together
	public Quaternion AddQuats(Quaternion one, Quaternion two)
	{
		Quaternion newQuat = new Quaternion(
			one.x + two.x,
			one.y + two.y,
			one.z + two.z,
			one.w + two.w
			);

		return newQuat;
	}

	public Quaternion MultiplyQuatVec3(Quaternion lhs, Vector3 rhs)
	{
		// Simplified concat
		Vector3 oneVecComp = new Vector3(lhs.x, lhs.y, lhs.z);

		Quaternion newQuat = new Quaternion();
		// wN = w0w1 - v0 dot v1
		newQuat.w = -Vector3.Dot(oneVecComp, rhs);
		// vN = w0v1 + w1v0 + v0 cross v1
		Vector3 newVecCom = (lhs.w * rhs) + Vector3.Cross(oneVecComp, rhs);

		newQuat.x = newVecCom.x;
		newQuat.y = newVecCom.y;
		newQuat.z = newVecCom.z;

		return newQuat;
	}
}