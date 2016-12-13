using UnityEngine;

[System.Serializable]
public class CollisionResolver {

	public LayerMask hitLayers = ~(1 << 8); // layer mask of the objetcs that interact with the bullet
	public bool doRicochet = true;
	public int maxBounces = 2;
	[Range(0f, 1f)]
	public float ricochetFactor = 1f;
	[Range(0f, 1f)]
	public float ricochetSpeedFactor = 0.5f;
	[Range(0f, 180f)]
	public float maxRicochetAngle = 120f;
	public float randomRicochetAngle = 0.1f;

	private int bounces = 0;
	private Vector3 previousPos;
	private Vector3 spawnpoint;
	private RaycastHit hit;

	private BulletBehaviour bulletBehaviour;
	private BulletKinematics bulletKinematics;

	public void Initiate(BulletBehaviour bulletBehaviour, Vector3 spawnpoint, BulletKinematics bulletKinematics) {
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

		if (Physics.Linecast(previousPos, bulletBehaviour.GetTransform().position, out hit, hitLayers) && 
			(bulletBehaviour.GetTime() > 0.001f)) {
			if (hit.collider != bulletBehaviour.GetGameObject()) {

				// ricochet 
				if ((bounces < maxBounces) &&
						doRicochet &&
						(Vector3.Angle(bulletBehaviour.GetTransform().forward, hit.normal) <= maxRicochetAngle &&
						Random.Range(0f, 1f) < ricochetFactor)) {

					// Increment bounce count
					bounces++;

					// Bounce callback
					bulletBehaviour.OnBulletCollision(hit, true);

					// calculate the reflect direction
					Vector3 reflectDirection = Vector3.Reflect(bulletBehaviour.GetTransform().forward, hit.normal);
					reflectDirection += new Vector3(Random.Range(0, randomRicochetAngle),
													Random.Range(-randomRicochetAngle, randomRicochetAngle),
													Random.Range(-randomRicochetAngle, randomRicochetAngle));

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

					// collision stuff
					spawnpoint = bulletBehaviour.GetTransform().position;
					previousPos = bulletBehaviour.GetTransform().position;

				} else {
					//OnSelfHit(hit.collider, hit.point);
					bulletBehaviour.OnBulletCollision(hit, false);
				}
			} // linecast

		}

		previousPos = bulletBehaviour.GetTransform().position;
	}
}
