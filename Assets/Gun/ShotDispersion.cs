using UnityEngine;

[System.Serializable]
public class ShotDispersion {
	public float minDispersion = 0f;
	public float maxDispersion = 0.1f;
	public float increaseDispersionRate = 1f;
	public float decreaseDispersionRate = 2f;

	private float _lastDispersion = 0f;

	public float getDispersionRate(float shootingTime, bool keyPressed) {

		// calculate current dispersion
		float currentDispersion = 0f;
		if (shootingTime == 0) {
			currentDispersion = Mathf.Lerp(_lastDispersion, minDispersion, Time.deltaTime * decreaseDispersionRate);
		} else {
			currentDispersion = Mathf.Lerp(_lastDispersion, maxDispersion, Time.deltaTime * increaseDispersionRate);
		}

		// clamp decimal aproximations
		if (!keyPressed && currentDispersion < minDispersion + 0.01f) {
			currentDispersion = minDispersion;
		}

		if (keyPressed && currentDispersion > maxDispersion - 0.01f) {
			currentDispersion = maxDispersion;
		}

		// store and return
		_lastDispersion = currentDispersion;
		return currentDispersion;
	}
}