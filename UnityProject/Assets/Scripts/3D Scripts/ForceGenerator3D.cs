using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ForceGenerator3D : MonoBehaviour
{
	/* *
	 * Pure Unity force generator
	 * Holds gravity constant
	 * Gets Vector3's from library interface
	 * */

	public static ForceGenerator3D instance;

	public float g;

	ThreeDForceGenerator forceLibGenerator;

	private void Awake()
	{
		instance = this;
		forceLibGenerator = new ThreeDForceGenerator();
	}

	public Vector3 GenerateForce_Gravity(float particleMass, Vector3 worldUp)
	{
		Vector3 f_gravity = forceLibGenerator.GenerateForce_Gravity(g, particleMass, worldUp);
		//Debug.Log("Generated f_gravity: " + f_gravity.ToString("F3"));
		return f_gravity;
	}

	public Vector3 GenerateForce_Normal(float particleMass, Vector3 worldUp, Vector3 normalUnit)
	{
		var watch = Stopwatch.StartNew();
		Vector3 f_normal = forceLibGenerator.GenerateForce_Normal(g, particleMass, worldUp, normalUnit);

		StatsTracker.instance.ReportTime(StatsTracker.Type.NORM, (int)watch.ElapsedTicks);
		
		//Debug.Log("Generated f_normal: " + f_normal.ToString("F3"));
		return f_normal;
	}

	public Vector3 GenerateForce_Friction(float sfCoef, float kfCoef, float particleMass, Vector3 particleVelocity, Vector3 worldUp, Vector3 normalUnit)
	{
		var watch = Stopwatch.StartNew();
		Vector3 f_friction = forceLibGenerator.GenerateForce_Friction(g, sfCoef, kfCoef, particleMass, particleVelocity, worldUp, normalUnit);

		StatsTracker.instance.ReportTime(StatsTracker.Type.FRIC, (int)watch.ElapsedTicks);

		//Debug.Log("Generated f_friction: " + f_friction.ToString("F3"));
		return f_friction;
	}

	public Vector3 GenerateForce_Drag(Vector3 velocity, Vector3 flVelocity, float liqDens, float crossArea, float dragCoef)
	{
		var watch = Stopwatch.StartNew();
		Vector3 f_drag = forceLibGenerator.GenerateForce_Drag(velocity, flVelocity, liqDens, crossArea, dragCoef);

		StatsTracker.instance.ReportTime(StatsTracker.Type.DRAG, (int)watch.ElapsedTicks);
		
		//UnityEngine.Debug.Log("Generated f_drag: " + (int)watch.ElapsedTicks);
		return f_drag;
	}

	public Vector3 GenerateForce_Spring(Vector3 objectLoc, Vector3 anchor, float springLength, float springConst)
	{
		Vector3 f_spring = forceLibGenerator.GenerateForce_Spring(objectLoc, anchor, springLength, springConst);
		//Debug.Log("Generated f_spring: " + f_spring.ToString("F3"));
		return f_spring;
	}

	public Vector3 GenerateForce_Torque(Vector3 f_Applied, Vector3 appPoint, Vector3 centerOfMass)
	{
		Vector3 f_torque = forceLibGenerator.GenerateForce_Torque(f_Applied, appPoint, centerOfMass);
		//Debug.Log("Generated f_torque: " + f_torque.ToString("F3"));
		return f_torque;
	}
}