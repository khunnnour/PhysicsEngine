#include "ThreeDForceGenerator.h"

// Generate force of gravity
forceVec generateGravity(float g, float m, forceVec up)
{
	forceVec f_gravity;
	f_gravity.type = 'f';

	f_gravity = up * (g * m);

	return f_gravity;
}

// Generate normal force
forceVec generateNormal(float g, float m, forceVec up, forceVec norm)
{
	// Calc gravity
	forceVec f_gravity = generateGravity(g, m, up);

	// Fn = proj(g, surfaceNormal_unit)
	forceVec f_normal = projectVec(f_gravity, norm);

	// Reverse direction
	f_normal = f_normal * -1.0f;

	return f_normal;
}

// Friction generator(s)
forceVec generateKfriction(forceVec f_normal, forceVec particleVelocity, float kfCoef)
{
	// ONLY FOR FRICTION FUNCTION NOT PUBLIC
	forceVec f_k_friction = particleVelocity.normalized() * -1.0f * kfCoef * f_normal.magnitude();

	return f_k_friction;
}
forceVec generateFriction(float g, float sfCoef, float kfCoef, float m, forceVec partVel, forceVec up, forceVec normUnit)
{
	// Get gravity force
	forceVec f_gravity = generateGravity(g, m, up);

	// Get normal force
	forceVec f_normal = generateNormal(g, m, up, normUnit);

	// Get max static force
	float maxStaticForceSqr = (f_normal * sfCoef).sqrMagnitude();

	// Find sliding force
	forceVec f_sliding = f_normal + f_gravity;

	// Check if sliding force is over threshold
	forceVec f_friction;
	if (f_sliding.sqrMagnitude() > maxStaticForceSqr)
	{
		// If over threshold then kinetic friction
		f_friction = generateKfriction(f_normal, partVel, kfCoef);
	}
	else
	{
		// Otherwise the opposite of sliding force (shouldn't move)
		f_friction = f_sliding * -1.0f;
	}

	return f_friction;
}

forceVec generateDrag(forceVec velocity, forceVec flVelocity, float liqDens, float crossArea, float dragCoef)
{
	// Get initial direction
	forceVec f_drag = velocity.normalized() * -1.0f;

	forceVec newVel = velocity - flVelocity;
	float mag = 0.5f * liqDens * newVel.magnitude() * newVel.magnitude() * crossArea * dragCoef;

	f_drag = f_drag * mag;

	return f_drag;
}

forceVec generateSpring(forceVec objLoc, forceVec anchor, float springLength, float springConst)
{
	// Get initial direction
	forceVec springDir = objLoc - anchor;
	float length = springDir.magnitude();

	// Account for weird behavior at zero length
	if (length == 0.0f)
		length = 0.001f;

	float mag = -springConst * (length - springLength);

	forceVec f_spring = springDir.normalized() * mag;

	return f_spring;
}

forceVec generateTorque(forceVec f_Applied, forceVec appPoint, forceVec centerOfMass)
{
	// Calculate moment arm
	forceVec momentArm = appPoint - centerOfMass;

	// Calculate torque
	forceVec f_torque = crossProduct(momentArm, f_Applied);
	f_torque.type = 't';

	return f_torque;
}