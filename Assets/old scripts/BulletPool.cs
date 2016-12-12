using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BulletPool : MonoBehaviour {

	public int initialSize = 20;
	public int incrementSize = 5;

	public GameObject bulletPrefab;
	 
	public Queue<GameObject> pool;
	//public Stack<GameObject> used;

	void Start () {

		// initiate queue
		pool = new Queue<GameObject> ();
		//used = new Stack<GameObject> ();

		// create initial pool
		AddBulletsToThePool (initialSize); // threadize this!!!!!!!!!!!!!!!!!!!!!!!!!
	}

	// PUBLIC FUNCTIONS #####################################################################
	public GameObject GetBullet(Vector3 pos, Quaternion rot){

		// no more bullets pool? 
		if (pool.Count <= 0) {
			AddBulletsToThePool (incrementSize); // threadize this!!!!!!!!!!!!!!!!!!!!!!!!!
		}

		GameObject bullet = pool.Dequeue ();
		
		bullet.transform.position = pos;
		bullet.transform.rotation = rot;

		bullet.SetActive (true);
		bullet.GetComponent<ProjectileKinematics> ().bulletPool = this;

		return bullet;
	}

	public void RecycleBullet(GameObject bullet){

		bullet.SetActive (false);
		pool.Enqueue (bullet);
	}

	// HELPER FUNCTIONS #####################################################################
	private void AddBulletsToThePool(int n){
		for (int i=0; i<n; i++) {
			GameObject bullet = (GameObject) Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity);
			bullet.SetActive(false);
			bullet.transform.SetParent(transform);

			pool.Enqueue(bullet);
		}
	}
}
