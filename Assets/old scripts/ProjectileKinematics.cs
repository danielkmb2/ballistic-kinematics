using UnityEngine;

// Behaviour that moves a projectile over an analytic trajectory.
// Additionally, it uses a stable and efficient custom rotation simulation
// that is only approximate but is still physically plausible.
public class ProjectileKinematics : MonoBehaviour {

	// TRAJECTORY PARAMETERS ##########################################################################

	// show the full trajectory of the bullet
	public bool debugTrajectory = true; 

	// If non-null, use the GameObject's own transform to set the position, and use
	// this rotationTransform to set the orientation. If null before Start() is called,
	// the GameObject's own transform is used to set both the position and the rotation.
	public Transform rotationTransform = null;

	// gravity's strength in m/s^2 (default: earth's 9.81 m/s^2)
	public Vector3 gravity = new Vector3(0, -9.81f, 0);

	// the constant wind velocity during a projectile's flight
	public Vector3 windVelocity = Vector3.zero;

	// Max trajectory time of the bullet
	public float lifeTime = 3f;

	// velocity towards gravitational pull to approach during a windless free fall               
	public float terminalVelocity = 35;

	// update the bullet rotation over time to face the trajectory.
	public bool updateRotation = true;

	// Stiffness of the virtual spring rotating the current heading towards the velocity's direction
	public float rotationStiffness = 3.0f;
	
	// Strength at which to damping any rotating rotate. 1 means critically dampened.
	public float rotationDamping = 0.3f;


	[HideInInspector] 					// The current velocity. This is also used as the initial
	public Vector3 velocity;			// velocity before Start() is called.
	[HideInInspector]
	public BulletPool bulletPool; 		// Asociated bullet pool to recycle self

	private float k;                 	// drag constant of projectile through air
	private Vector3 vInfinity;			// velocity of projectile falling indefinitely
	private Vector3 v0;                 // projectile3D velocity at t = 0
	private Vector3 p0;                 // projectile3D position at t = 0

	private Vector3 _angularVelocity;   // the current world-space angular velocity
	private Vector3 _windVelocity;      // the constaint wind velocity
	private float _headingFrequency;    // similar to a rotational 'spring' strength
	private float _headingDamping;      // the amount of rotation velocity damping
	private bool _isActive;             // true when updating the transform each frame
	private float _time;                // time elapsed since _isActive became true
	private bool initDone = false;
	
	// COLLISION PARAMETERS ##########################################################################

	public LayerMask hitLayers = ~(1 << 8); // layer mask of the objetcs that interact with the bullet

	private Vector3 previousPos;
	private Vector3 spawnpoint;
	private RaycastHit hit;

	// BOUNCE PARAMETERS ##########################################################################

	public bool doRicochet = true;
	public int maxBounces = 2;
	[Range(0f,1f)]public float ricochetFactor = 1f;
	[Range(0f,1f)]public float ricochetSpeedFactor = 20f;
	[Range(0f,180f)]public float maxRicochetAngle = 120f;
	public float randomRicochetAngle = 0.1f ;

	private int bounces = 0;

	// CUSTOM BEHAVIOUR STUFF ##########################################################################

	// Custom behaviour for onHit and onBounce
	public bool replaceOnHit = true; 			// Do we want to replace the bullet for a phisically interactive thing?
	public GameObject replacement;				// The rigidbody replacement for the bullet
	public bool pullRigidbodiesOnBounce = true; // Pull the objects when bounce
	public float mass = 1f; 					// Simulated mass of the bullet when pulling

	// CALLBACK FUNCTIONS ##########################################################################

	public void OnEnable(){
		//Debug.Log ("Enalbe");
	}

	public void OnDisable(){
		//Debug.Log ("Disable");
		initDone = false;
	}

	public void OnSelfHit(Collider other, Vector3 hitPoint){
		if (replaceOnHit && (replacement != null)) {
			// Instantiate the rigidBodie replacement in the previous timeStep to make sure its collider is not
			// overlaping the obstacle
			GameObject r = (GameObject)Instantiate (replacement, 
			                                        PositionAtTime (_time - Time.deltaTime), 
			                                        transform.rotation);

			r.GetComponent<Rigidbody> ().velocity = VelocityAtTime (_time);

			// The small rigidbodies can easly pass through slim obstacles when traveling fast. Continuous
			// collision detection is expensive but grants an acurate collision detection.
			r.GetComponent<Rigidbody> ().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		} else {
			// just pull the rigidbody objet
			PullRigidbodies (other, hitPoint);
			// INSTANTIATE HIT EFFECT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		}

		// Return this bullet to the bulletPool
		bulletPool.RecycleBullet (gameObject);
	}

	public void OnBounce(Collider other, Vector3 hitPoint){
		// OnBounceStuff
		PullRigidbodies (other, hitPoint);
	}

	private void PullRigidbodies(Collider other, Vector3 hitPoint){
		if (hit.rigidbody != null && pullRigidbodiesOnBounce) {
			float ec = 0.5f * mass * Mathf.Pow(VelocityAtTime(_time).magnitude, 2); // kinetic energy = 1/2*mass*v²
			hit.rigidbody.AddForceAtPosition(ec * transform.forward, hitPoint);
		}
	}

	// TRAJECTORY FUNCTIONS ##########################################################################
	
	// Start moving the projectile over the trajectory using UsedBallisticsSettings,
	// the current GameObject's position, the rotationTransform's orientation, and 
	// the velocity vector.
	public void Init() {
		_time = 0.0f;
		_isActive = true;
		
		float gravityLength = gravity.magnitude;
		k = 0.5f * gravityLength / terminalVelocity;
		vInfinity = gravity * (terminalVelocity / gravityLength) + windVelocity;
		
		v0 = velocity;
		p0 = transform.position;
		
		_headingFrequency = Mathf.Sqrt(k) * rotationStiffness;
		_headingDamping = rotationDamping;
		_windVelocity = windVelocity;
		
		if (rotationTransform == null) rotationTransform = transform;
		AdvanceTime(0.0f);
		
		// collision stuff
		spawnpoint = transform.position;
		previousPos = transform.position;
		
		// ricochet stuff
		bounces = 0;
		
		initDone = true;
	}

    // Update the position, rotation and velocity for the current frame
    void FixedUpdate() {
		if (!initDone)
			return;

        AdvanceTime(Time.deltaTime);
    }

    // Advance the position, rotation and velocity by the given amount of time
    private void AdvanceTime(float deltaTime) {
        if (_isActive) {

            _time += deltaTime;

			// Check for the remaining life time and recycle the bullet
			if (_time > lifeTime) {
				bulletPool.RecycleBullet (gameObject);
			}

			// Update the position for the (new) current time
			transform.position = PositionAtTime(_time);

			// Update the velocity for the (new) current time
			velocity = VelocityAtTime(_time);

			// Simulate the bullet facing the trajectory
            if(updateRotation) UpdateRotation(deltaTime);

			// Checks for collisions and computes the ricochet
			UpdateCollisions();

			// Debug the whole trajectory
			if(debugTrajectory) DebugDrawLastTrajectory(lifeTime) ;
        }
    }

	// The formula all the other math is based on: get the parameteric 
	// position p in world space at time t. 
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
    private void UpdateRotation(float deltaTime) {
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
        rotationTransform.rotation = q * rotationTransform.rotation;
    }

	// COLLISION FUNCTIONS #############################################################################

	// Using a linecast for every actualization of the bullet position we avoid passing through 
	// small objects. It also makes an alternative of the collider/rigidbody standard solution
	// with is less efficient if we want to create a lot of bullets,
	void UpdateCollisions(){

		// Avoid getting linecast from origin
		if(previousPos==Vector3.zero) {
			previousPos=spawnpoint;  
		}
		
		if (Physics.Linecast(previousPos, transform.position, out hit, hitLayers) && ((_time) > 0.001f)) {
			if(hit.collider != gameObject){

				// ricochet 
				if((bounces < maxBounces) && 
						doRicochet && 
						(Vector3.Angle(transform.forward,hit.normal) <= maxRicochetAngle && 
						Random.Range(0f,1f) < ricochetFactor)) {

					// Increment bounce count
					bounces++;

					// Bounce callback
					OnBounce(hit.collider, hit.point);

					// calculate the reflect direction
					Vector3 reflectDirection = Vector3.Reflect(transform.forward,hit.normal);
					reflectDirection += new Vector3(Random.Range(0,randomRicochetAngle),
					                                Random.Range(-randomRicochetAngle,randomRicochetAngle),
					                                Random.Range(-randomRicochetAngle,randomRicochetAngle));
					
					// redirect bullet
					transform.forward = reflectDirection;
					transform.position = hit.point ;
					
					// new speed
					velocity = VelocityAtTime(_time).magnitude * reflectDirection * ricochetSpeedFactor;

					// Reinitialize bullet parameters for a new trajectory with the bounce
					_time = 0.0f;

					v0 = velocity;
					p0 = transform.position;
					
					if (rotationTransform == null) rotationTransform = transform;
					AdvanceTime(0.0f);
					
					// collision stuff
					spawnpoint = transform.position;
					previousPos = transform.position;

				} else { 
					OnSelfHit(hit.collider, hit.point);
				}
			} // linecast

		}

		previousPos = transform.position;

	}

	// AUXILIAR STUFF ###############################################################################

	// Draw an animated trajectory with the given parameters using Debug.DrawLines.
	// Dont think too much about how this thing works
	public void DebugDrawLastTrajectory(float timeToTarget) {
		int numSegments = 20;
		int numSamples = numSegments * 2 + 4;
		float dt = timeToTarget / (numSamples - 4);
		float t = Time.timeSinceLevelLoad / (dt * 4);
		t = (t - Mathf.Floor(t)) * dt * 4 - dt * 4;
		Color black = new Color(0, 0, 0, 0.75f), white = new Color(1, 1, 1, 0.75f);
		
		for (int i = 0; i < numSamples; ++i) {
			float fromTime = Mathf.Clamp(t, 0.0f, timeToTarget);
			float toTime = Mathf.Clamp(t + dt, 0.0f, timeToTarget);
			if(fromTime>_time)
				Debug.DrawLine(PositionAtTime(fromTime),PositionAtTime(toTime),i % 4 < 2 ? black : white);
			else
				Debug.DrawLine(PositionAtTime(fromTime),PositionAtTime(toTime),Color.red);

			t += dt;
		}
	}
}