using UnityEngine;

[System.Serializable]
public class PhysicalInteractor {
	public float mass = 1f;                     // Simulated mass of the bullet when pulling
	public HitObjectGroup[] interactuableObjects;

	[System.Serializable]
	public class HitObjectGroup {
		public string tag;                          // hit tag
		public bool destroyOnHit = true;			// bullet will be destroyed when colliding
		public bool pullRigidbodiesOnBounce = true; // pull the objects when bounce
		public GameObject hitEffectsEnd;			// effects to instantiate when the bullet collides something
		public GameObject hitEffectsBounce;         // effects to instantiate when the bullet collides something and bounces
		public GameObject rigidReplacement;			// rigid object to replace the bullet when trajectory is destroyed
	}

	private BulletBehaviour bulletBehaviour;
	private BulletKinematics bulletKinematics;

	public void Initiate(BulletBehaviour bulletBehaviour, BulletKinematics bulletKinematics) {
		this.bulletBehaviour = bulletBehaviour;
		this.bulletKinematics = bulletKinematics;
	}

	public void OnBulletCollision(RaycastHit hit, bool canBounce) {

		bool pullRigidbodiesOnBounce = false;
		bool destroyOnHit = false;
		GameObject hitEffects = null;
		GameObject rigidReplacement = null;

		foreach (HitObjectGroup hog in interactuableObjects) {
			if (hit.collider.CompareTag(hog.tag)) {
				pullRigidbodiesOnBounce = hog.pullRigidbodiesOnBounce;
				destroyOnHit = hog.destroyOnHit;
				rigidReplacement = hog.rigidReplacement;

				if (canBounce) {
					hitEffects = hog.hitEffectsBounce;
				} else {
					hitEffects = hog.hitEffectsEnd;
				}
			}
		}

		// pull rigidbodies on hit
		if (pullRigidbodiesOnBounce) {
			PullRigidbodies(hit);
		}

		// instantiate collision effects
		if (hitEffects != null) {
			bulletBehaviour.InstantiateEffects(hitEffects, hit);
		}

		// we are not bouncing more, this is the last interaction
		if (!canBounce || destroyOnHit) {
			bulletBehaviour.RemoveBullet(rigidReplacement);
		}
	}

	private void PullRigidbodies(RaycastHit hit) {
		if (hit.rigidbody == null) {
			// no rigidbody
			return;
		}

		float ec = 0.5f * mass * Mathf.Pow(bulletKinematics.VelocityAtTime(
			bulletBehaviour.GetTime()).magnitude, 2); // kinetic energy = 1/2*mass*v²
		hit.rigidbody.AddForceAtPosition(ec * bulletBehaviour.GetTransform().forward, hit.point);

	}
}
