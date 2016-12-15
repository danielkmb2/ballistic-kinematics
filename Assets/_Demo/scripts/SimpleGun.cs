using UnityEngine;

public class SimpleGun : MonoBehaviour {

	public float fireRate = 0.05f;
	public float rigidBulletPower = 30f;

	public GameObject shotEffects;
	public GameObject rigidBullet;
	public GameObject bullet;
	public Transform spawnpoint;
	public Camera cam;

	private float lastShotTime = 0f;

	void Update() {


		if (lastShotTime + fireRate < Time.time) {

			if (Input.GetKey("mouse 0")) {
				GameObject bullet = GetRigidBullet(spawnpoint.transform.position, cam.transform.rotation);
				ShotRigidBullet(bullet);
				lastShotTime = Time.time;
			}
			if (Input.GetKey("mouse 1")) {
				GameObject bullet = GetBullet(spawnpoint.transform.position, cam.transform.rotation);
				ShotBullet(bullet);
				lastShotTime = Time.time;
			}

		}

	}

	private GameObject GetRigidBullet(Vector3 position, Quaternion rotation) {
		GameObject newBullet = (GameObject)Instantiate(rigidBullet, position, rotation);
		return newBullet;
	}

	private void ShotRigidBullet(GameObject bullet) {
		bullet.GetComponent<Rigidbody>().velocity = spawnpoint.up * rigidBulletPower;
	}

	private GameObject GetBullet(Vector3 position, Quaternion rotation) {
		GameObject newBullet = (GameObject)Instantiate(bullet, position, rotation);
		return newBullet;
	}

	private void ShotBullet(GameObject bullet) {
		if (shotEffects != null) {
			Instantiate(shotEffects, spawnpoint.position, spawnpoint.rotation);
		}

		bullet.GetComponent<BulletBehaviour>().StartTrajectory(spawnpoint.up * rigidBulletPower);
	}
}