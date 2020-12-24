using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePathScript : MonoBehaviour
{
	public float time = 2.0f;
	public int steps = 5;

	private LineRenderer line;
	private BasketballScript ball;
	private Transform head;
	private Particle3D ballPart;
	private Vector3 v_prevFrame, prevPos;
	private Vector3 f_gravity;
	private float timeStep, ballMass;

	// Start is called before the first frame update
	void Start()
	{
		line = gameObject.GetComponent<LineRenderer>();
		line.positionCount = steps;

		ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<BasketballScript>();
		head = GameObject.FindGameObjectWithTag("Player").transform;
		ballPart = ball.gameObject.GetComponent<Particle3D>();

		timeStep = time / steps;
		ballMass = ballPart.GetMass();

		f_gravity = ForceGenerator3D.instance.GenerateForce_Gravity(ballMass, Vector3.up);
	}

	// Update is called once per frame
	void Update()
	{
		// Initialize forces	
		Vector3 f_launch = ball.GetCurrentMod();
		Vector3 f_drag;

		Vector3 f_frame = f_launch + f_gravity;
		Vector3 a_frame, v_frame = Vector3.zero, p_frame;

		p_frame = Vector3.zero;
		line.SetPosition(0, p_frame);
		gameObject.transform.position = ball.gameObject.transform.position;

		for (int i = 1; i < steps; i++)
		{
			// Calculate a,v,p
			a_frame = f_frame / ballMass;
			v_frame += a_frame * timeStep;
			p_frame += v_frame * timeStep;

			// Add the point
			line.SetPosition(i, p_frame);

			f_drag = ForceGenerator3D.instance.GenerateForce_Drag(v_frame, Vector3.zero, 1.225f, 0.89f, 0.47f);
			f_frame = f_gravity + f_drag;
		}
	}
}
