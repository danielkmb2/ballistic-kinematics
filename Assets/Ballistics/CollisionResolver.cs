using UnityEngine;

[System.Serializable]
public class CollisionResolver {

	public LayerMask hitLayers = ~(1 << 8); 	// layer mask of the objetcs that interact with the bullet
	public bool doRicochet = true;				// can this object bounce on hit?
	public int maxBounces = 2;					// max number of bounces
	[Range(0f, 1f)]
	public float ricochetFactor = 1f;			// bounce probability
	[Range(0f, 1f)]
	public float ricochetSpeedFactor = 0.5f;	// speed loss factor on bounce
	[Range(0f, 180f)]
	public float maxRicochetAngle = 120f;		// max angle in wich the bullet will bounce
	public float randomRicochetAngle = 0.1f;	// random bounce angle

	private int bounces = 0;					
	private Vector3 previousPos;
	private Vector3 spawnpoint;
	private RaycastHit hit;

	private BulletBehaviour bulletBehaviour;
	private BulletKinematics bulletKinematics;

	public void Initiate(BulletBehaviour bulletBehaviour, Vector3 spawnpoint, BulletKinematics bulletKinematics) {
		this.bounces = 0;
		this.previousPos = spawnpoint;

		this.bulletBehaviour = bulletBehaviour;
		this.spawnpoint = spawnpoint;
		this.bulletKinematics = bulletKinematics;
	}

	// Using a linecast for every actualization of the bullet position we avoid passing through 
	// small objects. It also makes an alternative of the collider/rigidbody standard solution
	// with is less efficient if we want to create a lot of bullets,
	public void UpdateCollisions() {

		// Avoid getting linecast from origin
		if (previousPos == Vector3.zero) {
			previousPos = spawnpoint;
		}

		if (doLinecast() && (bulletBehaviour.GetTime() > 0.001f)) {
			if (hit.collider != bulletBehaviour.GetGameObject()) {

				bool maxBouncesNotReached = bounces < maxBounces;
				bool angleCondition = Vector3.Angle(bulletBehaviour.GetTransform().forward, hit.normal) <= maxRicochetAngle;
				bool ricochetFactorCondition = Random.Range(0f, 1f) < ricochetFactor;

				// ricochet? 
				if (maxBouncesNotReached && doRicochet && (angleCondition && ricochetFactorCondition)) {
					// calculate ricochet and notify BulletBehaviour that we have bounced
					doBounce();
				} else {
					// bullet is destroyed without bounce
					bulletBehaviour.OnBulletCollision(hit, false);
				}
			}
		}

		previousPos = bulletBehaviour.GetTransform().position;
	}

	private void doBounce() {

		// Increment bounce count
		bounces++;

		// Bounce callback
		bulletBehaviour.OnBulletCollision(hit, true/*is bouncing*/);

		// calculate the reflect direction
		Vector3 reflectDirection = getReflectDirection();

		// redirect bullet
		bulletBehaviour.GetTransform().forward = reflectDirection;
		bulletBehaviour.GetTransform().position = hit.point;

		// new speed
		bulletKinematics.velocity = bulletKinematics.VelocityAtTime(
			bulletBehaviour.GetTime()).magnitude * reflectDirection * ricochetSpeedFactor;

		// Reinitialize bullet parameters for a new trajectory with the bounce
		bulletBehaviour.SetTime(0.0f);
		bulletKinematics.v0 = bulletKinematics.velocity;
		bulletKinematics.p0 = bulletBehaviour.GetTransform().position;

		bulletBehaviour.AdvanceTime(0.0f);

		// update collision stuff
		spawnpoint = bulletBehaviour.GetTransform().position;
		previousPos = bulletBehaviour.GetTransform().position;
	}

	private bool doLinecast() {
		return Physics.Linecast(previousPos, bulletBehaviour.GetTransform().position, out hit, hitLayers);
	}

	private Vector3 getReflectDirection() {
		Vector3 reflectDirection = Vector3.Reflect(bulletBehaviour.GetTransform().forward, hit.normal);

		reflectDirection += new Vector3(
			Random.Range(0, randomRicochetAngle),
			Random.Range(-randomRicochetAngle, randomRicochetAngle),
			Random.Range(-randomRicochetAngle, randomRicochetAngle));

		return reflectDirection;
	}
}
