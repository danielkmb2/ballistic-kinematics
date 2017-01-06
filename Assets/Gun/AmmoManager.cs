using UnityEngine;

[System.Serializable]
public class AmmoManager {
	public string bulletPrefab;
	public int ammo = 100;
	public int rounds = 20;				// 0 means infinite
	public float reloadTime = 2f;
	public string reloadAnimation;
	public GameObject reloadEffects;

	public int remainingLoadedBullets = 0;
	private bool reloading = false;
	private float reloadingStart = 0f;
	private Gun mainGun;

	public void initiate(Gun gun) {
		remainingLoadedBullets = rounds;
		this.mainGun = gun;
	}

	public bool isLoaded() {
		return (remainingLoadedBullets > 0) && !reloading;
	}

	public bool isReloading() {
		return reloading;
	}

	public bool chargerIsFull() {
		return (remainingLoadedBullets == rounds);
	}

	public string getBullet() {
		if (isLoaded()) {
			remainingLoadedBullets--;
			return bulletPrefab;
		} else {
			// we need to reload. 
			Debug.Log("No more loaded bullets.");
			return null;
		}
	}

	public void reload() {
		if (!reloading) {
			mainGun.playReloadEffects(new ReloadEffects(reloadAnimation, reloadEffects));
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

	public bool bulletsRemaining() {
		return (isLoaded() || ammo > 0);
	}
}

public class ReloadEffects {
	public string reloadAnimation;
	public GameObject reloadEffects;

	public ReloadEffects(string reloadAnimation, GameObject reloadEffects) {
		this.reloadEffects = reloadEffects;
		this.reloadAnimation = reloadAnimation;
	}
}