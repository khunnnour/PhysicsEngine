#ifndef THREEDFORCEGENERATOR_H
#define THREEDFORCEGENERATOR_H

#include "Lib.h"
#include "forceVec.h"

#ifdef __cplusplus
extern "C"
{
#else // !__cplusplus

#endif // __cplusplus

// Generate force of gravity
THREEDFORCEGENERATOR_SYMBOL forceVec generateGravity(float g, float m, forceVec up);
// Generate normal force
THREEDFORCEGENERATOR_SYMBOL forceVec generateNormal(float g, float m, forceVec up, forceVec norm);
// Generate friction force
THREEDFORCEGENERATOR_SYMBOL forceVec generateFriction(float g, float sfCoef, float kfCoef, float m, forceVec partVel, forceVec up, forceVec normUnit);
// Generate drag
THREEDFORCEGENERATOR_SYMBOL forceVec generateDrag(forceVec velocity, forceVec flVelocity, float liqDens, float crossArea, float dragCoef);
// Generate spring force
THREEDFORCEGENERATOR_SYMBOL forceVec generateSpring(forceVec objLoc, forceVec anchor, float springLength, float springConst);
// Generate a torque
THREEDFORCEGENERATOR_SYMBOL forceVec generateTorque(forceVec f_Applied, forceVec appPoint, forceVec centerOfMass);

#ifdef __cplusplus
}
#else // !__cplusplus

#endif // __cplusplus

#endif // !THREEDFORCEGENERATOR
