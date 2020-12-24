using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ThreeDForceGenerator
{
	/* *
	 * Acts as interface with C++ library
	 * Converts the forceVec to Vector3 and vice verse
	 * */

	[StructLayout(LayoutKind.Sequential)]
	public struct forceVec
	{
		// Standard components
		public float x;
		public float y;
		public float z;

		// Type of force
		public char type;
		/* *
		 * f = a dircetional force
		 * t = a torque force
		 * d = a direction
		 * */
	}


	/* - IMPORT DLL FUNCTIONS - */
	[DllImport("ThreeDForceGenerator")]
	static extern forceVec generateGravity(float g, float m, forceVec up);

	[DllImport("ThreeDForceGenerator")]
	static extern forceVec generateNormal(float g, float m, forceVec up, forceVec norm);

	[DllImport("ThreeDForceGenerator")]
	static extern forceVec generateFriction(float g, float sfCoef, float kfCoef, float m, forceVec partVel, forceVec up, forceVec norm);

	[DllImport("ThreeDForceGenerator")]
	static extern forceVec generateDrag(forceVec vel, forceVec flVel, float liqDens, float crossArea, float dragCoef);

	[DllImport("ThreeDForceGenerator")]
	static extern forceVec generateSpring(forceVec objLoc, forceVec anchor, float springLength, float springConst);

	[DllImport("ThreeDForceGenerator")]
	static extern forceVec generateTorque(forceVec f_Applied, forceVec appPoint, forceVec centerOfMass);
	/* - END DLL IMPORTS - */


	/* - Conversion Funcitons - */
	Vector3 ConvertToVec3(forceVec f)
	{
		return new Vector3(f.x, f.y, f.z);
	}
	forceVec ConvertToForceVec(Vector3 f)
	{
		forceVec returnVec = new forceVec
		{
			x = f.x,
			y = f.y,
			z = f.z
		};

		return returnVec;
	}

	/* - TRANSLATION FUNCTIONS - */
	public Vector3 GenerateForce_Gravity(float g, float particleMass, Vector3 worldUp)
	{
		// Convert Vector3 to forceVec	
		forceVec vecUp = ConvertToForceVec(worldUp);

		// Find gravity
		forceVec f_gravity = generateGravity(g, particleMass, vecUp);
		

		// Return result converted back to Vector3
		return ConvertToVec3(f_gravity);
	}

	public Vector3 GenerateForce_Normal(float g, float particleMass, Vector3 worldUp, Vector3 normalUnit)
	{
		// Convert Vector3 to forceVec
		forceVec vecUp = ConvertToForceVec(worldUp);
		forceVec vecNorm = ConvertToForceVec(normalUnit);

		// Find normal
		forceVec f_normal = generateNormal(g, particleMass, vecUp, vecNorm);

		// Return result converted back to Vector3
		return ConvertToVec3(f_normal);
	}

	public Vector3 GenerateForce_Friction(float g, float sfCoef, float kfCoef, float particleMass, Vector3 particleVelocity, Vector3 worldUp, Vector3 normalUnit)
	{
		//Debug.Log("Vel: " + particleVelocity.ToString("F3") + " | norm unit: " + normalUnit.ToString("F3"));
		
		// Convert Vector3 to forceVec
		forceVec partVel = ConvertToForceVec(particleVelocity);
		forceVec vecUp = ConvertToForceVec(worldUp);
		forceVec vecNorm = ConvertToForceVec(normalUnit);

		// Find friction
		forceVec f_friction = generateFriction(g, sfCoef, kfCoef, particleMass, partVel, vecUp, vecNorm);

		// Return result converted back to Vector3
		return ConvertToVec3(f_friction);
	}

	public Vector3 GenerateForce_Drag(Vector3 particleVelocity, Vector3 fluidVelocity, float liqDens, float crossArea, float dragCoef)
	{
		// Convert Vector3 to forceVec
		forceVec vel = ConvertToForceVec(particleVelocity);
		forceVec flVel = ConvertToForceVec(fluidVelocity);

		// Find friction
		forceVec f_drag = generateDrag(vel, flVel, liqDens, crossArea, dragCoef);
		

		// Return result converted back to Vector3
		return ConvertToVec3(f_drag);
	}

	public Vector3 GenerateForce_Spring(Vector3 objectLoc, Vector3 anchor, float springLength, float springConst)
	{
		// Convert Vector3 to forceVec
		forceVec objLoc = ConvertToForceVec(objectLoc);
		forceVec anch = ConvertToForceVec(anchor);

		// Find spring force
		forceVec f_spring = generateSpring(objLoc, anch, springLength, springConst);

		// Return result converted back to Vector3
		return ConvertToVec3(f_spring);
	}

	public Vector3 GenerateForce_Torque(Vector3 f_Applied, Vector3 appPoint, Vector3 centerOfMass)
	{
		// Calculate moment arm
		Vector3 momentArm = appPoint - centerOfMass;

		// Calculate torque
		Vector3 f_torque = Vector3.Cross(momentArm, f_Applied);

		return f_torque;
	}
}