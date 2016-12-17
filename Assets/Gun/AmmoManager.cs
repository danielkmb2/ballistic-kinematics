using UnityEngine;

[System.Serializable]
public class AmmoManager {
	public GameObject bulletPrefab;
	public int ammo = 100;
	public int rounds = 20;				// 0 means infinite
	public float reloadTime = 2f;
	public AnimationClip reloadAnimation;
	public GameObject reloadEffects;

	public bool isLoaded() {
		return true;
	}

	public GameObject getBullet() {
		return bulletPrefab;
	}
}