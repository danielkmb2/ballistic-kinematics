using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToPoolOverTime : MonoBehaviour {

	public string poolHash = "default";
	public float lifeTime = 4f;

	private float _time = 0f;

	public void reset() {
		_time = 0f;
	}

	void Update() {
		_time += Time.deltaTime;
		if (_time > lifeTime) {
			PoolManager.getInstance ().recycleObject (poolHash, gameObject);
		}
	}
}
