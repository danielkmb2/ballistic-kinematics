using UnityEngine;
using System.Collections;

public class DestroyOverTime : MonoBehaviour {

	public float lifeTime = 4f;

	void Start () {
		Destroy (gameObject, lifeTime);
	}
}
