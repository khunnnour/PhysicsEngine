#ifndef FORCEVEC_H
#define FORCEVEC_H

#include "iostream"

/* - MY VERSION OF 3D VECTORS - */
struct forceVec
{
	// Standard components
	float x = 0.0f;
	float y = 0.0f;
	float z = 0.0f;
	
	// Type of force
	char type;
	/* *
	 * f = a dircetional force
	 * t = a torque force
	 * d = a direction
	 * */
	
	forceVec()
	{
		x = 0.0f;
		y = 0.0f;
		z = 0.0f;
		type = 'f';
	}
	forceVec(float a, char t)
	{
		x = a;
		y = a;
		z = a;
		type = t;
	}
	forceVec(float a, float b, float c, char t)
	{
		x = a;
		y = b;
		z = c;
		type = t;
	}

	forceVec operator*(float a);
	forceVec operator/(float a);
	forceVec operator-(forceVec rhs);
	forceVec operator+(forceVec rhs);
	std::ostream& operator<<(std::ostream& os);

	float magnitude();
	float sqrMagnitude();
	forceVec normalized();
};

// Prototypes for other vector based functions
float dotProduct(forceVec a, forceVec b);
forceVec crossProduct(forceVec a, forceVec b);
forceVec projectVec(forceVec projected, forceVec onto);

#endif //!FORCEVEC_H
