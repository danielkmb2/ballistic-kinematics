using UnityEngine;

[System.Serializable]
public class Weapon {
	public string name;
	public KeyCode selectionKey;
	public KeyCode primaryFireButton = KeyCode.Mouse0;
	public KeyCode reloadKey;
	public float bulletInitialPower = 30f;
	public float fireRate = 1f;
	public int bulletsPerShot = 1;
	public bool automatic = false;
	public ShotDispersion dispersionProperties;
	public ShotEffects shotEffects;
	public AmmoManager ammoProperties;

	private float lastShotTime = 0f;
	private float shootingTime = 0f;
	private Gun mainGun;

	public void initiate(Gun gun) {
		this.mainGun = gun;
	}

	public void update() {
		fireOperations();
		// reload?
		// things...
	}

	private void fireOperations() {
		// update the shooting time
		if (Input.GetKey(primaryFireButton)) {
			shootingTime += Time.deltaTime;
		} else {
			shootingTime = 0f;
		}

		// update shot
		if (automatic) {
			if (Input.GetKey(primaryFireButton)) {
				primaryFire();
			}
		} else { // semiautomatic
			if (Input.GetKeyDown(primaryFireButton)) {
				primaryFire();
			}
		}
	}

	private void primaryFire() {
		if (lastShotTime + fireRate < Time.time) {
			doPrimaryFire();
			lastShotTime = Time.time;
		}
	}

	private void doPrimaryFire() {
		Debug.Log("PEW");

		if (ammoProperties.isLoaded()) {
			// calculate shot properties
			GameObject bulletPrefab = ammoProperties.getBullet();
			float dispersionAngle = dispersionProperties.getDispersionRate();
			ShotProperties shotProperties = new ShotProperties(
				bulletPrefab, bulletInitialPower, dispersionAngle, bulletsPerShot);
			// notify to fire!
			mainGun.fire(shotProperties);

		} else {
			// TODO: we need to reload. can we reload?
		}
	}
}

public class ShotProperties {
	public GameObject bulletPrefab;
	public float bulletInitialPower;
	public float dispersionAngle;
	public int shells;

	public ShotProperties(GameObject bulletPrefab, float bulletInitialPower, 
							float dispersionAngle, int shells) {
		this.bulletPrefab = bulletPrefab;
		this.bulletInitialPower = bulletInitialPower;
		this.dispersionAngle = dispersionAngle;
		this.shells = shells;
	}
}