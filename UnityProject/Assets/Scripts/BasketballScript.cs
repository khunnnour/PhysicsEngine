using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasketballScript : MonoBehaviour
{
	[Header("Launcher Settings")]
	public Image launchSlider;
	public float baseLaunchForce;
	[Tooltip("The modifier min/max for launch force 'game'")]
	public Vector2 launchModBounds;
	[Tooltip("Time (sec) per rotation")]
	public float frequency;

	[Header("Pull Back Settings")]
	public Vector3 holdPos;
	[Tooltip("The distance where the basketball is reset")]
	public float closeDist = 1.5f;
	[Tooltip("How aggressively the ball is pulled back")]
	public float pullForce = 5.0f;

	[Header("Reset Settings")]
	public Vector3 minBounds;
	public Vector3 maxBounds;
	public Vector3 resetPos = Vector3.zero;

	private Transform origParent;
	private Particle3D particle;
	private Vector3 holdDir;
	private float launchForceMod;
	private float timer;
	private bool launching, launched, reeling;

	// floats for constructing sin wave for launch game
	float m, n;

	// Start is called before the first frame update
	void Start()
	{
		//origParent = transform.parent;

		origParent = GameObject.FindGameObjectWithTag("Player").transform;
		//holdDir = holdPos - origParent.position;

		particle = GetComponent<Particle3D>();
		particle.gravitySettings.useGravity = false;

		//holdPos = transform.localPosition;

		// Validate launch bounds
		if (launchModBounds.y < 0) launchModBounds.y = 1;

		if (launchModBounds.x < 0) launchModBounds.x = 0.1f;

		if (launchModBounds.x > launchModBounds.y)
			launchModBounds.x = launchModBounds.y - 1;

		// Calculating constanst for sin wave
		m = (launchModBounds.y - launchModBounds.x) / 2.0f;
		n = m + launchModBounds.x;
		//Debug.Log("m = " + m.ToString("F3") + " | n = " + n.ToString("F3"));

		// Set launching bools to false
		launching = false;
		launched = false;
		reeling = false;
	}

	// Update is called once per frame
	void Update()
	{
		GetInput();

		if (launching)
			LaunchGame();

		if (!launched)
			UpdatePos();

		if (reeling)
			ReelIn();

		CheckLocation();
		//Debug.Log("Current Acc = " + particle.GetAcceleration());
	}

	private void CheckLocation()
	{
		bool outCourt = false;

		if (transform.position.x < minBounds.x)
			outCourt = true;
		else if (transform.position.x > maxBounds.x)
			outCourt = true;

		if (transform.position.y < minBounds.y)
			outCourt = true;
		else if (transform.position.y > maxBounds.y)
			outCourt = true;

		if (transform.position.z < minBounds.z)
			outCourt = true;
		else if (transform.position.z > maxBounds.z)
			outCourt = true;

		if (outCourt == true)
		{
			particle.SetPosition(resetPos);
			launching = false;
			launched = false;
			reeling = false;
			launchForceMod = 0.1f;
			LaunchLure();
		}
	}

	void UpdatePos()
	{
		// Calculate position in front of guy
		Vector3 frontPos;

		frontPos = origParent.rotation * holdPos;

		Vector3 newHoldPos = origParent.position + frontPos;

		//Debug.DrawLine(origParent.position, newHoldPos);

		particle.SetPosition(newHoldPos);
	}

	void GetInput()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (!launched)
				LaunchLure();
			launching = false;
		}

		if (Input.GetMouseButtonDown(0) && !launched)
		{
			timer = 0.0f;
			launching = true;
		}

		if (Input.GetMouseButtonDown(1) && launched)
		{
			reeling = true;
		}
		if (Input.GetMouseButtonUp(1) && reeling)
			reeling = false;
	}

	void LaunchGame()
	{
		// Find current mod
		launchForceMod = -m * Mathf.Cos(timer * (1.0f / frequency) * 6.283185f) + n;
		timer += Time.deltaTime;

		// Change color/size of indicator
		float perc = (launchForceMod - launchModBounds.x) / (launchModBounds.y - launchModBounds.x);
		launchSlider.color = new Color(1.0f * (1.0f - perc), 1.0f * perc, 0.0f);
		launchSlider.transform.localScale = new Vector3(perc + 0.5f, perc + 0.5f, 1.0f);

		//Debug.DrawRay(particle.GetPosition(), origParent.forward * baseLaunchForce * launchForceMod, Color.red);
		//Debug.Log("LAUNCH GAME: " + timer.ToString("F3") + "s\nCurrent Mod: " + launchForceMod.ToString("F3") + " = " + perc.ToString("F3"));
	}

	void LaunchLure()
	{
		launched = true;

		// Remove parent so not affected by move/rotation
		//transform.parent = null;
		//transform.position = ;
		//particle.SetPosition(origParent.position + holdPos);
		//particle.SetPosition(holdPos);

		// Reset launcher icon
		launchSlider.color = Color.white;
		launchSlider.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		// Get direction of launch
		Vector3 dir = origParent.transform.forward;
		dir.Normalize();

		// Add force
		particle.KillAllMovement();
		particle.AddForce(dir * baseLaunchForce * launchForceMod);
		particle.gravitySettings.useGravity = true;
	}

	public Vector3 GetCurrentMod()
	{
		// Get direction of launch
		Vector3 dir = origParent.transform.forward;
		dir.Normalize();

		return dir * baseLaunchForce * launchForceMod;
	}

	public void RetrieveLure()
	{
		// Un-launched
		launched = false;
		//transform.parent = origParent;

		// Remove forces/velocities/rotations
		particle.KillAllMovement();

		// No gravity/collsions to not interact with world
		particle.gravitySettings.useGravity = false;

		// Put back in front of player
		UpdatePos();
	}

	void ReelIn()
	{
		particle.AddForce(ForceGenerator3D.instance.GenerateForce_Spring(particle.GetPosition(), origParent.position, 0.15f, pullForce));

		if ((particle.GetPosition() - origParent.position).sqrMagnitude < closeDist * closeDist)
			RetrieveLure();
	}
}