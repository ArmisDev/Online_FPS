using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armis.Utilities.ObjectPool
{
	[CreateAssetMenu(menuName = "Utilities/ObjectPooler")]
	public class ObjectPoolAsset : ScriptableObject
	{
		public Pool[] pools;
		[System.NonSerialized]
		Dictionary<string, Pool> poolDict = new Dictionary<string, Pool>();
		[System.NonSerialized]
		Transform parentTransform;
		public bool prewarm;

		public void Init()
		{
			poolDict.Clear();
			parentTransform = new GameObject("Object Pool").transform;

			for (int i = 0; i < pools.Length; i++)
			{
				poolDict.Add(pools[i].poolName, pools[i]);
				pools[i].ClearObjects();

				if (prewarm)
				{
					pools[i].PrewarmObject(parentTransform);
				}
			}
		}

		public GameObject GetObject(string id)
		{
			poolDict.TryGetValue(id, out Pool value);
			return value.GetObject(parentTransform);
		}
	}

	[System.Serializable]
	public class Pool {

		public string poolName;
		public GameObject prefab;
		public int budget;

		[System.NonSerialized]
		List<GameObject> createdObjects = new List<GameObject>();
		[System.NonSerialized]
		int index;

		public void ClearObjects()
		{
			createdObjects.Clear();
		}

		public GameObject GetObject(Transform parent)
		{
			GameObject retVal = null;

			if (createdObjects.Count < budget)
			{
				GameObject go = GameObject.Instantiate(prefab) as GameObject;
				go.transform.parent = parent;
				createdObjects.Add(go);
				retVal = go;
			}
			else
			{
				if (createdObjects[index] == null)
				{
					createdObjects[index] = GameObject.Instantiate(prefab);
					createdObjects[index].transform.parent = parent;
				}

				retVal = createdObjects[index];
				index++;
				if (index > createdObjects.Count - 1)
				{
					index = 0;
				}
			}

			retVal.SetActive(false);

			return retVal;
		}

		public void PrewarmObject(Transform parent)
		{
			for (int i = 0; i < budget; i++)
			{
				GameObject go = GameObject.Instantiate(prefab) as GameObject;
				go.SetActive(false);
				go.transform.parent = parent;
				createdObjects.Add(go);
			}
		}
	}
}
