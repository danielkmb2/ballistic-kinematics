using UnityEngine;

[System.Serializable]
public class AmmoManager {
	public GameObject bulletPrefab;
	public int ammo = 100;
	public int rounds = 20;				// 0 means infinite
	public float reloadTime = 2f;
	public AnimationClip reloadAnimation;
	public GameObject reloadEffects;

	public int remainingLoadedBullets = 0;
	private bool reloading = false;
	private float reloadingStart = 0f;

	public void initiate() {
		remainingLoadedBullets = rounds;
	}

	public bool isLoaded() {
		return (remainingLoadedBullets > 0) && !reloading;
	}

	public bool isReloading() {
		return reloading;
	}

	public GameObject getBullet() {
		if (isLoaded()) {
			remainingLoadedBullets--;
			return bulletPrefab;
		} else {
			// we need to reload. 
			Debug.Log("No more bullets. We need to reload");
			return null;
		}
	}

	public void reload() {
		if (!reloading) {
			reloading = true;
			reloadingStart = Time.time;
		} else {
			Debug.Log("already reloading");
		}
	}

	public void update() {
		if (reloading && (Time.time > reloadTime + reloadingStart)) {
			reloading = false;
			int takenBullets = 0;
			if (ammo >= rounds) {
				takenBullets = rounds;
			} else {
				takenBullets = ammo;
			}

			ammo = ammo - takenBullets;
			remainingLoadedBullets = takenBullets;
		}
	}
}