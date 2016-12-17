using UnityEngine;

public class Gun : MonoBehaviour {

	public Transform spawnPoint;
	public Weapon[] weapons;

	private Weapon activeWeapon;

	void Start() {
		// initiate weapons
		foreach (Weapon weapon in this.weapons) {
			weapon.initiate(this);
		}

		// use the first weapon by default
		if (weapons.Length > 0) {
			activeWeapon = weapons[0];
		}
	}

	void Update() {
		activeWeapon.update();
	}

	public void fire(ShotProperties shotProperties) {
		Debug.Log(shotProperties.bulletPrefab);
		// instantiate bullet
		GameObject bullet = (GameObject) Instantiate(
			shotProperties.bulletPrefab, 
			spawnPoint.transform.position, 
			spawnPoint.transform.rotation);

		Vector3 shotDirection = spawnPoint.forward;
		shotDirection += new Vector3(
			Random.Range(0, shotProperties.dispersionAngle),
			Random.Range(-shotProperties.dispersionAngle / 2, shotProperties.dispersionAngle / 2),
			Random.Range(-shotProperties.dispersionAngle / 2, shotProperties.dispersionAngle / 2));


		bullet.GetComponent<BulletBehaviour>().StartTrajectory(shotDirection * shotProperties.bulletInitialPower);

	}
}
