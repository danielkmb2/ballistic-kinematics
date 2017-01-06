using UnityEngine;

public class BulletBehaviour : MonoBehaviour {

	public string POOLTAG = "bullet";

	// do we want to see the trajectory?
	public bool debugTrajectory = true;

	// Max trajectory time of the bullet
	public float lifeTime = 5f;

	[System.Serializable]
	public class KinematicProperties {

		// gravity's strength in m/s^2 (default: earth's 9.81 m/s^2)
		public Vector3 gravity = new Vector3(0, -9.81f, 0);

		// the constant wind velocity during a projectile's flight
		public Vector3 windVelocity = Vector3.zero;

		// velocity towards gravitational pull to approach during a windless free fall               
		public float terminalVelocity = 35;

		// update the bullet rotation over time to face the trajectory.
		public bool updateRotation = true;

		// Stiffness of the virtual spring rotating the current heading towards the velocity's direction
		public float rotationStiffness = 3.0f;

		// Strength at which to damping any rotating rotate. 1 means critically dampened.
		public float rotationDamping = 0.3f;
	}
	public KinematicProperties kinematicProperties;
	public CollisionResolver collisionResolver;
	public PhysicalInteractor physicalInteractor;

	BulletKinematics bulletKinematics;
	private float _time;                // time active

	public void Awake() {
		bulletKinematics = new BulletKinematics();	
	}

	public void StartTrajectory(Vector3 initialVelocity) {
		_time = 0f;
		collisionResolver.Initiate(this, transform.position, bulletKinematics);

		physicalInteractor.Initiate(this, bulletKinematics);

		bulletKinematics.Initiate(
			transform.position,
			kinematicProperties.gravity,
			kinematicProperties.windVelocity, 
			initialVelocity,
			kinematicProperties.terminalVelocity,
			kinematicProperties.rotationStiffness,
			kinematicProperties.rotationDamping);
	}
	
	public void OnBulletCollision(RaycastHit hit, bool canBounce) {
		physicalInteractor.OnBulletCollision(hit, canBounce);
	}

	// Update the position, rotation and velocity for the current frame
	void FixedUpdate() {
		AdvanceTime (Time.deltaTime > 1f ? 0.01f : Time.deltaTime);
	}

	public void SetTime(float newTime) {
		_time = newTime;
	}

	public float GetTime() {
		return _time;
	}

	public Transform GetTransform() {
		return transform;
	}

	public GameObject GetGameObject() {
		return gameObject;
	}

	// Advance the position, rotation and velocity by the given amount of time
	public void AdvanceTime(float deltaTime) {
		_time += deltaTime;

		// Check for the remaining life time and recycle the bullet
		if (_time > lifeTime) {
			RemoveBullet(null); // no replacement
		}

		// Update the position for the (new) current time
		transform.position = bulletKinematics.PositionAtTime(_time);

		// Update the velocity for the (new) current time
		bulletKinematics.UpdateVelocity(_time);

		// Simulate the bullet facing the trajectory
		if (kinematicProperties.updateRotation) {
			Quaternion newRotation = bulletKinematics.UpdateRotation(deltaTime, transform);
			transform.rotation = newRotation;
		}

		// Checks for collisions and computes the ricochet
		collisionResolver.UpdateCollisions();

		// Debug the whole trajectory
		if (debugTrajectory)
			bulletKinematics.DebugDrawLastTrajectory(_time, lifeTime);

	}

	public void RemoveBullet(string replacementHash) {
		// TODO: RECYCLE IN A BULLET POOL MAYBE?
		if (replacementHash != null) {
			GameObject go = PoolManager.getInstance().getObject(replacementHash);
			go.GetComponent<ReturnToPoolOverTime> ().reset ();
			go.transform.position = bulletKinematics.PositionAtTime(_time - Time.deltaTime);
			go.transform.rotation = transform.rotation;
			go.GetComponent<Rigidbody>().velocity = bulletKinematics.VelocityAtTime(_time);
		}

		PoolManager.getInstance().recycleObject(POOLTAG, gameObject);
	}

	public void InstantiateEffects(GameObject hitEffects, RaycastHit hit) {
		Instantiate(hitEffects, hit.point, Quaternion.FromToRotation(hit.transform.up, hit.normal));
	}
}