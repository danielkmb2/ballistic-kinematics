using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool {
	
	private ArrayList objects = new ArrayList();
	private GameObject originalPrefab;
	private Transform parentTransform;

	public ObjectPool(GameObject prefab, int amount, Transform parentTransform) {
		originalPrefab = prefab;
		this.parentTransform = parentTransform;

		for (int i = 0; i < amount; i++) {
			GameObject go = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
			go.transform.parent = parentTransform;
			go.SetActive (false);
			objects.Add(go);
		}
	}

	public GameObject getObject() {
		GameObject go = getFirstActiveObject();

		if (go == null) {
			Debug.Log ("No available objects in pool. Instantiating a new one");
			go = GameObject.Instantiate (originalPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			go.transform.parent = parentTransform;
			objects.Add(go);
		}

		go.SetActive (true);
		return go;
	}

	public void recycleObject(GameObject obj) {
		obj.SetActive (false);
	}

	private GameObject getFirstActiveObject() {
		foreach (GameObject go in objects) {
			if (!go.activeSelf) {
				return go;
			}
		}

		return null;
	}
}
