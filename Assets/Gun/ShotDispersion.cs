using UnityEngine;

[System.Serializable]
public class ShotDispersion {
	public float minDispersion = 0f;
	public float maxDispersion = 0.1f;
	public AnimationCurve scaleFunction;
	public float dispersionTime = 2f;

	public float getDispersionRate(float shootingTime) {

		if (dispersionTime == 0) {
			// avoid get divided by zero and die
			return maxDispersion;
		}

		float normalizedDispersionTime = Mathf.Clamp(shootingTime, 0, dispersionTime) / dispersionTime;
		float currentDispersion = scaleFunction.Evaluate(normalizedDispersionTime) * maxDispersion;
		return currentDispersion;
	}
}