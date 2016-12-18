using UnityEngine;

public class Gun : MonoBehaviour {

	public KeyCode nextWeapon = KeyCode.PageUp;
	public KeyCode previousWeapon = KeyCode.PageDown;
	public Transform spawnPoint;
	public Weapon[] weapons;

	private int activeWeapon = 0;
	Animation animationComponent;

	void Start() {

		animationComponent = GetComponent<Animation>();

		// initiate weapons
		foreach (Weapon weapon in this.weapons) {
			weapon.initiate(this);
		}
	}

	void Update() {
		manageWeaponSelection();
		weapons[activeWeapon].update();
	}

	private void manageWeaponSelection() {
		if (Input.GetKeyDown(nextWeapon)) {
			if (activeWeapon == weapons.Length - 1) {
				activeWeapon = 0;
			} else {
				activeWeapon++;
			}
		}

		if (Input.GetKeyDown(previousWeapon)) {
			if (activeWeapon == 0) {
				activeWeapon = weapons.Length - 1;
			} else {
				activeWeapon--;
			}
		}
	}

	public void fire(ShotProperties shotProperties) {

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
			Instantiate(shotProperties.effects,	spawnPoint.transform.position, spawnPoint.transform.rotation);
		}

		if (shotProperties.recoilAnimation != null && !"".Equals(shotProperties.recoilAnimation)) {
			animationComponent.Stop();
			animationComponent.Play(shotProperties.recoilAnimation);
		}
	}

	public void playReloadEffects(ReloadEffects reloadEffects) {
		if (reloadEffects.reloadEffects != null) {
			Instantiate(reloadEffects.reloadEffects, spawnPoint.transform.position, spawnPoint.transform.rotation);
		}

		if (reloadEffects.reloadAnimation != null && !"".Equals(reloadEffects.reloadAnimation)) {
			animationComponent.Stop();
			animationComponent.Play(reloadEffects.reloadAnimation);
		}
	}
}
