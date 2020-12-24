using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceGenerator : MonoBehaviour
{
	public static Vector3 GenerateForce_Gravity(float particleMass, float g, Vector3 worldUp)
	{
		// F = m * g
		float weight = particleMass * g;

		Vector3 f_gravity = worldUp;
		f_gravity *= weight;
		/*
		Debug.Log("World-up: " + worldUp);
		Debug.Log("Weight: " + weight);
		Debug.Log("Gravity Vector: " + f_gravity);
		*/
		return f_gravity;
	}

	public static Vector3 GenerateForce_Normal(float particleMass, float g, float rot, Vector3 normalUnit)
	{
		// Fn = proj(g, surfaceNormal_unit)
		//Vector2 f_gravity = GenerateForce_Gravity(particleMass, g, worldUp);
		//Vector2 f_normal = (f_gravity * normalUnit) * normalUnit;
		// Reverse to make push up
		//f_normal *= -1.0f;

		float normalScalar = particleMass * -g * Mathf.Cos(rot);

		Vector3 f_normal = normalUnit * normalScalar;
		/*
		Debug.Log("Local Up: " + normalUnit);
		Debug.Log("Normal Force: " + f_normal);
		*/
		return f_normal;
	}

	public static Vector3 GenerateForce_Sliding(float particleMass, float g, float rot, Vector3 worldUp, Vector3 normalUnit)
	{
		// Fslide = g + norm
		Vector3 f_gravity = GenerateForce_Gravity(particleMass, g, worldUp);
		Vector3 f_normal = GenerateForce_Normal(particleMass, g, rot, normalUnit);

		Vector3 f_slide = f_gravity + f_normal;
		/*
		Debug.Log("Local Up: " + normalUnit.ToString("F4"));
		Debug.Log("Normal Force: " + f_normal.ToString("F4"));
		Debug.Log("Slide Vector: " + f_slide.ToString("F4"));
		*/
		return f_slide;
	}

	public static Vector2 GenerateForce_Friction_Static(float particleMass, float g, float rot, float sfCoef, Vector3 worldUp, Vector3 normalUnit)
	{
		// Get the magnitude of the normal force
		Vector3 f_normal = GenerateForce_Normal(particleMass, g, rot, normalUnit);
		float normalScalar = f_normal.magnitude;

		// Get, normalize, and reverse the slide vector to get friction direction
		Vector3 frictionDir = GenerateForce_Sliding(particleMass, g, rot, worldUp, normalUnit);
		frictionDir.Normalize();
		frictionDir *= -1.0f;

		// Find the scalar of static friction
		float staticFrictionScalar = normalScalar * sfCoef;

		// Give force a direction
		Vector3 f_s_friction = frictionDir * staticFrictionScalar;

		/*
		Debug.Log("Friction Scalar: " + staticFrictionScalar);
		Debug.Log("Friction Vector: " + f_s_friction);
		*/

		return f_s_friction;
	}

	public static Vector3 GenerateForce_Friction_Kinetic(Vector3 f_normal, Vector3 particleVelocity, float kfCoef)
	{
		Vector3 f_k_friction = -kfCoef * f_normal.magnitude * particleVelocity.normalized;

		// Debug.Log("Friction Vector: " + f_k_friction.ToString("F3"));

		return f_k_friction;
	}

	public static Vector3 GenerateForce_Drag(Vector3 velocity, Vector3 flVelocity, float liqDens, float crossArea, float dragCoef)
	{
		// Get initial direction
		Vector3 f_drag = velocity.normalized * -1.0f;

		Vector3 newVel = velocity - flVelocity;
		float mag = 0.5f * liqDens * newVel.magnitude * newVel.magnitude * crossArea * dragCoef;

		f_drag *= mag;
		/*
		Debug.Log("Velocity: " + velocity.ToString("F4"));
		Debug.Log("Drag: " + f_drag.ToString("F4"));
		*/
		return f_drag;
	}

	public static Vector3 GenerateForce_Spring(Vector3 objectLoc, Vector3 anchor, float springLength, float springConst, float damp, float time)
	{
		// Get initial direction
		Vector3 springDir = objectLoc - anchor;
		float length = springDir.magnitude;

		// Account for weird behavior at zero length
		if (length == 0.0f)
			length = 0.01f;

		float mag = -springConst * (length - springLength);
		
		Vector3 f_spring = springDir.normalized * mag;

		//Debug.Log("Spring Force: " + f_spring.ToString("F4"));

		return f_spring;
	}

	public static Vector3 GenerateForce_Directional(Vector3 direction, float magnitude)
	{
		return direction * magnitude;
	}

	public static float GenerateForce_Torque(Vector2 f_Applied, Vector3 appPoint, Vector3 centerOfMass)
	{
		Vector3 dir = appPoint - centerOfMass;

		// Torque cross product
		float f_torque = (dir.x * f_Applied.y) - (dir.y * f_Applied.x);

		//Debug.Log("Moment Arm: " + dir.ToString("F4"));
		//Debug.Log("Torque Force: " + f_torque.ToString("F4"));

		return f_torque;
	}
}