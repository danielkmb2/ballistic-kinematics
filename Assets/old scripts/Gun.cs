using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

	public WindZone bulletWind;
	public GameObject bullet;
	public GameObject rigidBullet;
	public Transform spawnpoint;
	public Camera cam;
	public bool useWind = true;
	public bool aim = false;
	public BulletPool bulletPool;

	public float power = 50f;

	void Start() {
		//ObjectPoolingManager.Instance.CreatePool (this.bullet, 5, 10, true);
	}

	void Update () {		

		if(Input.GetKeyDown("mouse 0")) {
			//GameObject b = (GameObject) Instantiate(bullet, spawnpoint.transform.position, cam.transform.rotation);

			GameObject bullet = bulletPool.GetBullet(spawnpoint.transform.position, cam.transform.rotation);
			ProjectileKinematics projectileKinematics = bullet.GetComponent<ProjectileKinematics>();
			
			projectileKinematics.velocity = spawnpoint.forward * power;
			projectileKinematics.windVelocity = bulletWind.transform.forward * bulletWind.windMain;
			projectileKinematics.Init();


		}

		if(Input.GetKeyDown("mouse 2")) {
			GameObject b = (GameObject) Instantiate(rigidBullet, spawnpoint.transform.position, cam.transform.rotation);
			
			b.GetComponent<Rigidbody>().velocity = spawnpoint.forward * power;

		}

		if(Input.GetKeyDown("mouse 1")){ aim = !aim;}
		if(aim) {transform.localPosition = new Vector3(0.014f,-0.226f,0.86f);}
		if(!aim){transform.localPosition = new Vector3(0.72f,-0.37f,0.86f);}
	}
}
