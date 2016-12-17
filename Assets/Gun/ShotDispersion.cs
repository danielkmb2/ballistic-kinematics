using UnityEngine;

[System.Serializable]
public class ShotDispersion {
	public float minDispersion = 0f;
	public float maxDispersion = 0.1f;
	public AnimationCurve scaleFunction;
	public float dispersionTime = 2f;

	public float getDispersionRate() {
		return 0f;
	}
}