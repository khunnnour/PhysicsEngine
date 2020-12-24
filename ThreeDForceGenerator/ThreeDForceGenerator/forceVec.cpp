#include "forceVec.h"
#include "math.h"
#include <intrin.h>

// Overloaded operators
forceVec forceVec::operator*(float a)
{
	forceVec newV;

	newV.x = x * a;
	newV.y = y * a;
	newV.z = z * a;

	return newV;
}
forceVec forceVec::operator-(forceVec rhs)
{
	float thisVec[4] = { x,y,z,0 };
	float otherVec[4] = { rhs.x, rhs.y, rhs.z,0 };
	
	_mm_store_ps(thisVec, _mm_sub_ps(_mm_load_ps(thisVec), _mm_load_ps(otherVec)));

	return forceVec(thisVec[0], thisVec[1], thisVec[2], 'f');
}
forceVec forceVec::operator+(forceVec rhs)
{
	float thisVec[4] = { x,y,z,0 };
	float otherVec[4] = { rhs.x, rhs.y, rhs.z,0 };
	
	_mm_store_ps(thisVec, _mm_add_ps(_mm_load_ps(thisVec), _mm_load_ps(otherVec)));

	return forceVec(thisVec[0], thisVec[1], thisVec[2], 'f');
}
forceVec forceVec::operator/(float a)
{
	forceVec newV;

	newV.x = x / a;
	newV.y = y / a;
	newV.z = z / a;

	return newV;
}
std::ostream& forceVec::operator<<(std::ostream& os)
{
	os << type << ": (" << x << ", "
		<< y << ", "
		<< z << ")\n";
	return os;
}

// Other member functions
float forceVec::magnitude()
{
	float thisVec[4] = { x,y,z,0 };
	return 	_mm_cvtss_f32(_mm_sqrt_ps(_mm_dp_ps(_mm_load_ps(thisVec), _mm_load_ps(thisVec), 0xFF)));
}

float forceVec::sqrMagnitude()
{
	return (x * x + y * y + z * z);
}

forceVec forceVec::normalized()
{
	float mag = this->magnitude();
	if (mag == 0.0f)
		return forceVec(0.0f, this->type);
	else
		return *this / mag;
}

// OTHER VECTOR FUNCTIONS DEFS
float dotProduct(forceVec a, forceVec b)
{
	float thisVec[4] = { a.x, a.y, a.z, 0 };
	float otherVec[4] = { b.x, b.y, b.z, 0 };

	return _mm_cvtss_f32(_mm_dp_ps(_mm_load_ps(thisVec), _mm_load_ps(otherVec), 0xFF));
}

forceVec crossProduct(forceVec a, forceVec b)
{
	float x = (a.y * b.z) - (a.z * b.y);
	float y = (a.z * b.x) - (a.x * b.z);
	float z = (a.x * b.y) - (a.y * b.x);

	return forceVec(x, y, z, a.type);
}

forceVec projectVec(forceVec projected, forceVec onto)
{
	float dot = dotProduct(projected, onto);
	float ontoMag2 = onto.magnitude();
	ontoMag2 *= ontoMag2;

	return onto * (dot / ontoMag2);
}