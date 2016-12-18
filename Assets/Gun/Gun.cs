using UnityEngine;

public class Gun : MonoBehaviour {

	public Transform spawnPoint;
	public Weapon[] weapons;

	private Weapon activeWeapon;
	Animation animationComponent;

	void Start() {

		animationComponent = GetComponent<Animation>();

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

		for (int i = 0; i < shotProperties.shells; i++) {
			// instantiate bullet
			GameObject bullet = (GameObject)Instantiate(
				shotProperties.bulletPrefab,
				spawnPoint.transform.position,
				spawnPoint.transform.rotation);

			Vector3 shotDirection = spawnPoint.forward;
			shotDirection += new Vector3(
				Random.Range(0, shotProperties.dispersionAngle),
				Random.Range(-shotProperties.dispersionAngle / 2, shotProperties.dispersionAngle / 2),
				Random.Range(-shotProperties.dispersionAngle / 2, shotProperties.dispersionAngle / 2));

			// set bulletbehaviour properties
			bullet.GetComponent<BulletBehaviour>().StartTrajectory(shotDirection * shotProperties.bulletInitialPower);
		}

		if (shotProperties.effects != null) {
			GameObject bullet = (GameObject)Instantiate(
	shotProperties.effects,
	spawnPoint.transform.position,
	spawnPoint.transform.rotation);
		}

		if (shotProperties.recoilAnimation != null) {
			animationComponent.Stop();
			animationComponent.Play(shotProperties.recoilAnimation);
		}
	}
}
