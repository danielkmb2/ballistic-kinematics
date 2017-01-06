using UnityEngine;

[System.Serializable]
public class PhysicalInteractor {
	public HitObjectGroup defaultInteraction;
	public HitObjectGroup[] interactuableObjects;

	[System.Serializable]
	public class HitObjectGroup {
		public string tag;                          // hit tag
		public float collisionMass = 5f;            // Simulated mass of the bullet when pulling
		public bool destroyOnHit = true;			// bullet will be destroyed when colliding
		public bool pullRigidbodiesOnBounce = true; // pull the objects when bounce
		public GameObject hitEffectsEnd;			// effects to instantiate when the bullet collides something
		public GameObject hitEffectsBounce;         // effects to instantiate when the bullet collides something and bounces
		public string rigidReplacement;				// rigid object to replace the bullet when trajectory is destroyed
	}

	private BulletBehaviour bulletBehaviour;
	private BulletKinematics bulletKinematics;

	public void Initiate(BulletBehaviour bulletBehaviour, BulletKinematics bulletKinematics) {
		this.bulletBehaviour = bulletBehaviour;
		this.bulletKinematics = bulletKinematics;
	}

	public void OnBulletCollision(RaycastHit hit, bool canBounce) {

		GameObject hitEffects = null;
		HitObjectGroup hitObject = getHitObjectProperties(hit);
		if (canBounce) {
			hitEffects = hitObject.hitEffectsBounce;
		} else {
			hitEffects = hitObject.hitEffectsEnd;
		}

		// pull rigidbodies on hit
		if (hitObject.pullRigidbodiesOnBounce) {
			PullRigidbodies(hit, hitObject.collisionMass);
		}

		// instantiate collision effects
		if (hitEffects != null) {
			bulletBehaviour.InstantiateEffects(hitEffects, hit);
		}

		// we are not bouncing more, this is the last interaction
		if (!canBounce || hitObject.destroyOnHit) {
			bulletBehaviour.RemoveBullet(hitObject.rigidReplacement);
		}
	}

	private HitObjectGroup getHitObjectProperties(RaycastHit hit) {
		foreach (HitObjectGroup hog in interactuableObjects) {
			if (hit.collider.CompareTag(hog.tag)) {
				return hog;
			}
		}

		return defaultInteraction;
	}

	private void PullRigidbodies(RaycastHit hit, float mass) {
		if (hit.rigidbody == null) {
			// no rigidbody
			return;
		}

		float ec = 0.5f * mass * Mathf.Pow(bulletKinematics.VelocityAtTime(
			bulletBehaviour.GetTime()).magnitude, 2); // kinetic energy = 1/2*mass*v²
		hit.rigidbody.AddForceAtPosition(ec * bulletBehaviour.GetTransform().forward, hit.point);

	}
}
