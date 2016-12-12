using UnityEngine;

public class SimpleGun : MonoBehaviour {

	public float rigidBulletPower = 30f;

	public GameObject rigidBullet;
	public GameObject bullet;
	public Transform spawnpoint;
	public Camera cam;

	void Update() {
		if (Input.GetKeyDown("mouse 0")) {
			GameObject bullet = GetRigidBullet(spawnpoint.transform.position, cam.transform.rotation);
			ShotRigidBullet(bullet);
		}
		if (Input.GetKeyDown("mouse 1")) {
			GameObject bullet = GetBullet(spawnpoint.transform.position, cam.transform.rotation);
			ShotBullet(bullet);
		}

	}

	private GameObject GetRigidBullet(Vector3 position, Quaternion rotation) {
		GameObject newBullet = (GameObject)Instantiate(rigidBullet, position, rotation);
		return newBullet;
	}

	private void ShotRigidBullet(GameObject bullet) {
		bullet.GetComponent<Rigidbody>().velocity = spawnpoint.forward * rigidBulletPower;
	}

	private GameObject GetBullet(Vector3 position, Quaternion rotation) {
		GameObject newBullet = (GameObject)Instantiate(bullet, position, rotation);
		return newBullet;
	}

	private void ShotBullet(GameObject bullet) {
		bullet.GetComponent<BulletBehaviour>().StartTrajectory(spawnpoint.forward * rigidBulletPower);
	}
}