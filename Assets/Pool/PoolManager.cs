using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour {

	[System.Serializable]
	public class PrefabPoolProperties {
		public string indexName;
		public GameObject prefab;
		public int initialAmmount;
	}

	public PrefabPoolProperties[] prefabPools;

	private Dictionary<string, ObjectPool> prefabPoolHash;
	private static PoolManager instance = null;

	public static PoolManager getInstance() {
		if (instance == null) {
			instance = new PoolManager ();
			return instance;
		} else {
			return instance;
		}
	}
		
	void Awake () {
		instance = this;

		prefabPoolHash = new Dictionary<string, ObjectPool> ();
		foreach (PrefabPoolProperties prefabPoolProperties in prefabPools) {
			ObjectPool pool = new ObjectPool (prefabPoolProperties.prefab, prefabPoolProperties.initialAmmount, transform);
			prefabPoolHash.Add (prefabPoolProperties.indexName, pool);
		}
	}

	public GameObject getObject(string poolHash) {
		ObjectPool pool = prefabPoolHash [poolHash];
		return pool.getObject();
	}

	public void recycleObject(string poolHash, GameObject go) {
		ObjectPool pool = prefabPoolHash [poolHash];
		pool.recycleObject (go);
	}
}