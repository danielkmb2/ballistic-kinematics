using UnityEngine;

public class BulletKinematics {

	public Vector3 velocity;           // The current velocity. This is also used as the initial

	private float k;                    // drag constant of projectile through air
	private Vector3 vInfinity;          // velocity of projectile falling indefinitely
	public Vector3 v0;                  // projectile3D velocity at t = 0
	public Vector3 p0;                  // projectile3D position at t = 0

	private Vector3 _angularVelocity;   // the current world-space angular velocity
	private Vector3 _windVelocity;      // the constaint wind velocity
	private float _headingFrequency;    // similar to a rotational 'spring' strength
	private float _headingDamping;      // the amount of rotation velocity damping

	public void Initiate(Vector3 currentPosition, Vector3 gravity, Vector3 windVelocity, Vector3 initialVelocity, 
		float terminalVelocity, float rotationStiffness, float rotationDamping) {

		float gravityLength = gravity.magnitude;
		k = 0.5f * gravityLength / terminalVelocity;
		vInfinity = gravity * (terminalVelocity / gravityLength) + windVelocity;

		velocity = initialVelocity;
		v0 = velocity;
		p0 = currentPosition;

		_headingFrequency = Mathf.Sqrt(k) * rotationStiffness;
		_headingDamping = rotationDamping;
		_windVelocity = windVelocity;
	}

	public void UpdateVelocity(float t) {
		velocity = VelocityAtTime(t);
	}

	public Vector3 PositionAtTime(float t) {
		float kt = k * t;
		return (v0 + vInfinity * kt) * t / (1 + kt) + p0;
	}

	// Use the analytical derivative of PositionAtTime at t to calculate v(t)
	public Vector3 VelocityAtTime(float t) {
		float kt = k * t;
		float h = 1 + kt;
		return (v0 + kt * (2 + kt) * vInfinity) / (h * h);
	}

	// Advance an efficient but approximate rotation simulation by deltaTime seconds
	public Quaternion UpdateRotation(float deltaTime, Transform rotationTransform) {
		Vector3 forward = rotationTransform.forward;
		Vector3 relativeVelocity = velocity - _windVelocity;
		Vector3 sin = Vector3.Cross(relativeVelocity, forward);
		float cos = Vector3.Dot(relativeVelocity, forward);

		// Convert the difference between velocity and heading in terms of sin and cos to an
		// approximation of this as an angle-times-axis rotational acceleration. This approximation 
		// is strictly conservative, steadily climbs up to 120 degrees, and then drops off 
		// to 0 again. The approximation is a bit like sin itself, but has an extremum not at
		// +/- 90 degrees but at 120 degrees. Consequently, it will force the headings of
		// of projectiles that are even more than 90 degrees off to self correct, while 
		// still avoiding any discontinuity (and thus unstability) at +180 and -180.
		Vector3 angularError = 3 * sin / (2 * relativeVelocity.magnitude + cos + Mathf.Epsilon);

		// Use an inherently stable implicit Euler integration to update _angularVelocity
		// and angularDelta. Note that the angular velocity and angularDelta are in world-space
		// (which is physically incorrect but faster to computer, and close enough for our 
		// purposes as there's virtually no need to consider spin here.
		float wt = _headingFrequency * deltaTime;
		_angularVelocity = (_angularVelocity - _headingFrequency * wt * angularError) /
						   (1 + wt * (2 * _headingDamping + wt));
		Vector3 angularDelta = _angularVelocity * deltaTime;

		// Conservatively and efficiently approximate the delta quaternion q = [
		// angularDelta.normalized() * sin(angularDelta.magnitude()/2), cos(angularDelta.magnitude()/2] 
		// by approximating tan(x/4) with x/4 and using the trigonometric identities 
		// cos(2x)=(1-tan(x)^2)/(1+tan(x)^2) and sin(2x)=(2*tan(x)^2)/(1+tan(x)^2). Note
		// that the result isn't normalized, but that's handled by Unity automatically.
		Quaternion q = new Quaternion(angularDelta.x, angularDelta.y, angularDelta.z,
									  2.0f - 0.125f * angularDelta.sqrMagnitude);

		// Apply the rotation delta. Again, this is done (physically incorrectly) in world space.
		return q * rotationTransform.rotation;
	}

	// Draw an animated trajectory with the given parameters using Debug.DrawLines.
	// Dont think too much about how this thing works
	public void DebugDrawLastTrajectory(float _time, float timeToTarget) {
		int numSegments = 20;
		int numSamples = numSegments * 2 + 4;
		float dt = timeToTarget / (numSamples - 4);
		float t = Time.timeSinceLevelLoad / (dt * 4);
		t = (t - Mathf.Floor(t)) * dt * 4 - dt * 4;
		Color black = new Color(0, 0, 0, 0.75f), white = new Color(1, 1, 1, 0.75f);

		for (int i = 0; i < numSamples; ++i) {
			float fromTime = Mathf.Clamp(t, 0.0f, timeToTarget);
			float toTime = Mathf.Clamp(t + dt, 0.0f, timeToTarget);
			if (fromTime > _time)
				Debug.DrawLine(PositionAtTime(fromTime), PositionAtTime(toTime), i % 4 < 2 ? black : white);
			else
				Debug.DrawLine(PositionAtTime(fromTime), PositionAtTime(toTime), Color.red);

			t += dt;
		}
	}
}
